using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class FlashcardGenerator
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public FlashcardGenerator(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("Flashcards") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> GenerateAsync(string topic, int count, string difficulty)
    {
        var agent = ResolveAgent();
        var prompt = $"Generate {count} flashcards about '{topic}' at {difficulty} difficulty. Format each as:\n**Card {{n}}**\nFront: [question/term]\nBack: [answer/definition]\nHint: [optional hint]";
        return await agent.GenerateAsync(prompt);
    }
}
