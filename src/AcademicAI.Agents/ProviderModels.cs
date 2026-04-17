namespace AcademicAI.Agents;

public static class ProviderModels
{
    public static readonly Dictionary<string, string[]> All = new()
    {
        ["OpenRouter"] = ["openai/gpt-4o-mini", "anthropic/claude-3.5-sonnet", "google/gemini-2.0-flash-001", "meta-llama/llama-3.3-70b-instruct", "qwen/qwen-2.5-72b-instruct"],
        ["Fireworks"] = ["accounts/fireworks/models/llama-v3p3-70b-instruct", "accounts/fireworks/models/qwen2p5-72b-instruct", "accounts/fireworks/models/mixtral-8x22b-instruct", "accounts/fireworks/models/deepseek-v3"],
    };

    public static double EstimateCost(string provider, string model, int promptTokens, int completionTokens)
    {
        var key = $"{provider}|{model}";
        var (promptPer1K, completionPer1K) = Pricing.GetValueOrDefault(key, (0.0005, 0.0015));
        return (promptTokens / 1000.0) * promptPer1K + (completionTokens / 1000.0) * completionPer1K;
    }

    private static readonly Dictionary<string, (double Prompt, double Completion)> Pricing = new()
    {
        ["OpenRouter|openai/gpt-4o-mini"] = (0.00015, 0.0006),
        ["OpenRouter|anthropic/claude-3.5-sonnet"] = (0.003, 0.015),
        ["OpenRouter|google/gemini-2.0-flash-001"] = (0.0, 0.0),
        ["OpenRouter|meta-llama/llama-3.3-70b-instruct"] = (0.00085, 0.00085),
        ["OpenRouter|qwen/qwen-2.5-72b-instruct"] = (0.00035, 0.00035),
        ["Fireworks|accounts/fireworks/models/llama-v3p3-70b-instruct"] = (0.0009, 0.0009),
        ["Fireworks|accounts/fireworks/models/qwen2p5-72b-instruct"] = (0.0009, 0.0009),
        ["Fireworks|accounts/fireworks/models/mixtral-8x22b-instruct"] = (0.0008, 0.0008),
        ["Fireworks|accounts/fireworks/models/deepseek-v3"] = (0.0009, 0.0009),
    };
}
