using System.Collections.Generic;

namespace AcademicAI.Core.Models;

public class AppSettings
{
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "Dark";
    public bool MinimizeToTray { get; set; } = false;
    public bool ClipboardMonitorEnabled { get; set; } = false;
    public bool FirstRun { get; set; } = true;

    public double WindowLeft { get; set; } = -1;
    public double WindowTop { get; set; } = -1;
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
    public bool IsMaximized { get; set; } = false;

    public Dictionary<string, FeatureProviderConfig> FeatureDefaults { get; set; } = new()
    {
        ["StudyHub"] = new() { Feature = "StudyHub", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["ResearchLibrary"] = new() { Feature = "ResearchLibrary", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Citations"] = new() { Feature = "Citations", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["AcademicWriter"] = new() { Feature = "AcademicWriter", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Translator"] = new() { Feature = "Translator", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Humanizer"] = new() { Feature = "Humanizer", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Summarizer"] = new() { Feature = "Summarizer", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Detector"] = new() { Feature = "Detector", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Flashcards"] = new() { Feature = "Flashcards", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Quiz"] = new() { Feature = "Quiz", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["EssayOutline"] = new() { Feature = "EssayOutline", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["Paraphraser"] = new() { Feature = "Paraphraser", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["GrammarCheck"] = new() { Feature = "GrammarCheck", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["MathSolver"] = new() { Feature = "MathSolver", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
        ["NoteOrganizer"] = new() { Feature = "NoteOrganizer", Provider = "OpenRouter", Model = "openai/gpt-4o-mini" },
    };
}
