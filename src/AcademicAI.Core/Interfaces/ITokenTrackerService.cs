using AcademicAI.Core.Models;

namespace AcademicAI.Core.Interfaces;

public interface ITokenTrackerService
{
    void RecordUsage(string provider, string model, int promptTokens, int completionTokens, double estimatedCost);
    TokenUsageRecord GetTotalUsage();
    IReadOnlyList<TokenUsageRecord> GetUsageByProvider();
    void Reset();
}
