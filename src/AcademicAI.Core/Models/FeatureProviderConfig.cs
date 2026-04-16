namespace AcademicAI.Core.Models;

public class FeatureProviderConfig
{
    public string Feature { get; set; } = "";
    public string Provider { get; set; } = "OpenRouter";
    public string Model { get; set; } = "openai/gpt-4o-mini";
}
