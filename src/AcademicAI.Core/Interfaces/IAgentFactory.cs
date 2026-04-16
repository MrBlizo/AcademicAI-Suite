namespace AcademicAI.Core.Interfaces;

public interface IAgentFactory
{
    IAIAgent GetAgent(string providerName);
    IReadOnlyList<string> GetAvailableProviders();
    IReadOnlyList<string> GetModelsForProvider(string providerName);
}
