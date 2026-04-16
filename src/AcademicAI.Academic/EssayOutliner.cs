using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class EssayOutliner
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public EssayOutliner(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("EssayOutline") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> GenerateAsync(string topic, string essayType, string academicLevel)
    {
        var agent = ResolveAgent();
        var prompt = $"Create a detailed essay outline for a {essayType} essay at the {academicLevel} level on the topic: {topic}. Include: I. Introduction (hook, background, thesis), II-N. Body sections (topic sentence, evidence, analysis), Conclusion (restatement, synthesis, call to action). Be specific with arguments and evidence suggestions.";
        return await agent.GenerateAsync(prompt);
    }
}
