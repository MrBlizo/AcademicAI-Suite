namespace AcademicAI.Agents;

public static class ProviderModels
{
    public static readonly Dictionary<string, string[]> All = new()
    {
        ["OpenRouter"] = ["openai/gpt-4o-mini", "anthropic/claude-3.5-sonnet", "google/gemini-2.0-flash-001", "meta-llama/llama-3.3-70b-instruct", "qwen/qwen-2.5-72b-instruct"],
        ["Fireworks"] = ["accounts/fireworks/models/llama-v3p3-70b-instruct", "accounts/fireworks/models/qwen2p5-72b-instruct", "accounts/fireworks/models/mixtral-8x22b-instruct", "accounts/fireworks/models/deepseek-v3"],
    };
}
