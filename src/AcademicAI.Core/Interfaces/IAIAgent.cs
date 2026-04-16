namespace AcademicAI.Core.Interfaces;

public interface IAIAgent
{
    string Name { get; }
    void SetApiKey(string apiKey);
    void SetModel(string model);
    Task<string> GenerateAsync(string prompt);
    Task<(bool Success, string Error)> TestConnectionWithDetailsAsync();
}
