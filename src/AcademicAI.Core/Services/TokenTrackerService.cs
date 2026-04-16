using System.Collections.Concurrent;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Core.Services;

public class TokenTrackerService : ITokenTrackerService
{
    private readonly ConcurrentDictionary<string, TokenUsageRecord> _records = new();

    public void RecordUsage(string provider, string model, int promptTokens, int completionTokens, double estimatedCost)
    {
        var key = $"{provider}|{model}";
        _records.AddOrUpdate(key,
            _ => new TokenUsageRecord
            {
                Provider = provider,
                Model = model,
                TotalCalls = 1,
                TotalPromptTokens = promptTokens,
                TotalCompletionTokens = completionTokens,
                EstimatedCost = estimatedCost
            },
            (_, existing) =>
            {
                existing.TotalCalls++;
                existing.TotalPromptTokens += promptTokens;
                existing.TotalCompletionTokens += completionTokens;
                existing.EstimatedCost += estimatedCost;
                return existing;
            });
    }

    public TokenUsageRecord GetTotalUsage()
    {
        var total = new TokenUsageRecord();
        foreach (var r in _records.Values)
        {
            total.TotalCalls += r.TotalCalls;
            total.TotalPromptTokens += r.TotalPromptTokens;
            total.TotalCompletionTokens += r.TotalCompletionTokens;
            total.EstimatedCost += r.EstimatedCost;
        }
        return total;
    }

    public IReadOnlyList<TokenUsageRecord> GetUsageByProvider()
    {
        return _records.Values.ToList().AsReadOnly();
    }

    public void Reset()
    {
        _records.Clear();
    }
}
