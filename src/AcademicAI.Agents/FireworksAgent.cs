using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.Agents;

public class FireworksAgent : IAIAgent
{
    private readonly HttpClient _http;
    private readonly ITokenTrackerService? _tokenTracker;
    private string _apiKey = "";
    private string _model = "accounts/fireworks/models/llama-v3p3-70b-instruct";

    public string Name => "Fireworks";

    public FireworksAgent(ITokenTrackerService? tokenTracker = null)
    {
        _tokenTracker = tokenTracker;
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.fireworks.ai/inference/v1/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

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
                    var estimatedCost = ProviderModels.EstimateCost("Fireworks", _model, promptTokens, completionTokens);
                    _tokenTracker.RecordUsage("Fireworks", _model, promptTokens, completionTokens, estimatedCost);
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
