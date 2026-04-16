using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class Paraphraser
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public Paraphraser(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("Paraphraser") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> ParaphraseAsync(string text, string style)
    {
        var agent = ResolveAgent();
        var prompt = $"Paraphrase the following text in a {style} style. Preserve the original meaning but use different words and sentence structure. Only return the paraphrased text:\n\n{text}";
        return await agent.GenerateAsync(prompt);
    }
}
