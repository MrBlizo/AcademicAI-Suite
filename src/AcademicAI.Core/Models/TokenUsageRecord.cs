namespace AcademicAI.Core.Models;

public class TokenUsageRecord
{
    public string Provider { get; set; } = "";
    public string Model { get; set; } = "";
    public int TotalCalls { get; set; }
    public int TotalPromptTokens { get; set; }
    public int TotalCompletionTokens { get; set; }
    public double EstimatedCost { get; set; }
}
