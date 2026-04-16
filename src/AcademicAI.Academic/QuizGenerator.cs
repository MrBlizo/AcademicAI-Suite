using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class QuizGenerator
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public QuizGenerator(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("Quiz") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> GenerateAsync(string topic, int questionCount, string quizType)
    {
        var agent = ResolveAgent();
        var prompt = $"Generate a {quizType} quiz with {questionCount} questions about: {topic}. Number each question. For multiple choice, provide 4 options (A-D) and indicate the correct answer. For true/false, state whether T or F. For short answer, provide the expected answer.";
        return await agent.GenerateAsync(prompt);
    }
}
