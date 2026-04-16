using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Academic;

public class AcademicTextProcessor
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;

    public AcademicTextProcessor(IAgentFactory agentFactory, IAppSettingsService settingsService, ISecretStore secretStore)
    {
        _agentFactory = agentFactory;
        _settingsService = settingsService;
        _secretStore = secretStore;
    }

    private IAIAgent ResolveAgent(string feature)
    {
        var config = _settingsService.GetFeatureDefault(feature) ?? new FeatureProviderConfig();
        var agent = _agentFactory.GetAgent(config.Provider);
        agent.SetModel(config.Model);
        var apiKey = _secretStore.Load($"apikey_{config.Provider}");
        if (apiKey != null) agent.SetApiKey(apiKey);
        return agent;
    }

    public async Task<string> GenerateWritingAsync(string prompt, string type, string tone)
    {
        var agent = ResolveAgent("AcademicWriter");
        var systemPrompt = $"You are an expert academic writer. Write a {type} in a {tone} tone based on the following:\n\n{prompt}";
        return await agent.GenerateAsync(systemPrompt);
    }

    public async Task<string> GenerateCitationAsync(string text, string style)
    {
        var agent = ResolveAgent("Citations");
        var systemPrompt = $"Generate properly formatted {style} citations for the following sources:\n\n{text}";
        return await agent.GenerateAsync(systemPrompt);
    }

    public async Task<string> SearchResearchAsync(string query)
    {
        var agent = ResolveAgent("ResearchLibrary");
        var systemPrompt = $"Search for academic papers and research related to: {query}. Provide title, authors, year, and abstract for each result.";
        return await agent.GenerateAsync(systemPrompt);
    }

    public async Task<string> GenerateFlashcardsAsync(string topic, int count)
    {
        var agent = ResolveAgent("StudyHub");
        var systemPrompt = $"Generate {count} flashcards about '{topic}'. Format each as:\nQ: [question]\nA: [answer]\nH: [hint]";
        return await agent.GenerateAsync(systemPrompt);
    }

    public async Task<string> SummarizeAsync(string text, string length = "Medium")
    {
        var agent = ResolveAgent("Summarizer");
        var lengthInstruction = length switch
        {
            "Brief" => "Provide a very brief summary in 1-2 sentences.",
            "Detailed" => "Provide a detailed summary covering all major points, examples, and nuances.",
            _ => "Summarize concisely while preserving key information."
        };
        var systemPrompt = $"{lengthInstruction}\n\n{text}";
        return await agent.GenerateAsync(systemPrompt);
    }

    public async Task<string> TranslateAsync(string text, string sourceLang, string targetLang)
    {
        var agent = ResolveAgent("Translator");
        var systemPrompt = $"Translate the following text from {sourceLang} to {targetLang}. Only return the translation:\n\n{text}";
        return await agent.GenerateAsync(systemPrompt);
    }
}
