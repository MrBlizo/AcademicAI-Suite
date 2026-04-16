namespace AcademicAI.Core.Interfaces;

public interface ILocalizationManager
{
    static ILocalizationManager? Instance { get; }
    string this[string key] { get; }
    string CurrentLanguage { get; }
    bool IsRightToLeft { get; }
    void SetLanguage(string languageCode);
    void Reload();
}
