using AcademicAI.Core.Interfaces;

namespace AcademicAI.Agents;

public class AgentFactory : IAgentFactory
{
    private readonly Dictionary<string, Func<IAIAgent>> _providers = new();

    public AgentFactory(ITokenTrackerService tokenTracker)
    {
        _providers["OpenRouter"] = () => new OpenAiCompatibleAgent(
            "OpenRouter", "https://openrouter.ai/api/v1", "openai/gpt-4o-mini", tokenTracker,
            req => { req.Headers.Add("HTTP-Referer", "https://academicai.app"); req.Headers.Add("X-Title", "AcademicAI Suite"); });

        _providers["Fireworks"] = () => new FireworksAgent(tokenTracker);
    }

    public IAIAgent GetAgent(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var factory))
            return factory();

        throw new ArgumentException($"Unknown provider: {providerName}");
    }

    public IReadOnlyList<string> GetAvailableProviders() => _providers.Keys.ToList().AsReadOnly();

    public IReadOnlyList<string> GetModelsForProvider(string providerName)
    {
        if (ProviderModels.All.TryGetValue(providerName, out var models))
            return models;
        return [];
    }
}
