using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.Agents;

public class OpenAiCompatibleAgent : IAIAgent
{
    private readonly HttpClient _http;
    private readonly ITokenTrackerService? _tokenTracker;
    private string _apiKey = "";
    private string _model = "";
    private readonly string _baseUrl;
    private readonly string _providerName;
    private readonly Action<HttpRequestMessage>? _configureRequest;

    public string Name => _providerName;

    public OpenAiCompatibleAgent(string providerName, string baseUrl, string defaultModel, ITokenTrackerService? tokenTracker = null, Action<HttpRequestMessage>? configureRequest = null)
    {
        _providerName = providerName;
        _baseUrl = baseUrl;
        _model = defaultModel;
        _tokenTracker = tokenTracker;
        _configureRequest = configureRequest;
        _http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    public void SetApiKey(string apiKey) => _apiKey = apiKey;
    public void SetModel(string model) => _model = model;

    public async Task<string> GenerateAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _configureRequest?.Invoke(request);

        using var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

        if (_tokenTracker != null)
        {
            try
            {
                if (doc.RootElement.TryGetProperty("usage", out var usage))
                {
                    var promptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : 0;
                    var completionTokens = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : 0;
                    var estimatedCost = ProviderModels.EstimateCost(_providerName, _model, promptTokens, completionTokens);
                    _tokenTracker.RecordUsage(_providerName, _model, promptTokens, completionTokens, estimatedCost);
                }
            }
            catch { }
        }

        return content;
    }

    public async Task<(bool Success, string Error)> TestConnectionWithDetailsAsync()
    {
        try
        {
            var result = await GenerateAsync("Hello, respond with just 'OK'.");
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
