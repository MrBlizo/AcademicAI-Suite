using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class MathSolver
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public MathSolver(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("MathSolver") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> SolveAsync(string problem)
    {
        var agent = ResolveAgent();
        var prompt = $"Solve the following math problem step by step. Show each step clearly with explanations. At the end, box the final answer.\n\n{problem}";
        return await agent.GenerateAsync(prompt);
    }
}
