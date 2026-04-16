using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class GrammarChecker
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public GrammarChecker(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("GrammarCheck") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> CheckAsync(string text)
    {
        var agent = ResolveAgent();
        var prompt = $"Check the following text for grammar, spelling, and punctuation errors. List each error found with: 1) The incorrect text, 2) The correction, 3) A brief explanation. At the end, provide the corrected version of the full text.\n\n{text}";
        return await agent.GenerateAsync(prompt);
    }
}
