using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class NoteOrganizer
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public NoteOrganizer(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent()
    {
        var config = _settingsService.GetFeatureDefault("NoteOrganizer") ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> OrganizeAsync(string notes)
    {
        var agent = ResolveAgent();
        var prompt = $"Organize the following raw notes into a well-structured format. Create clear headings, group related points, add bullet points, highlight key terms, and create a summary at the end:\n\n{notes}";
        return await agent.GenerateAsync(prompt);
    }
}
