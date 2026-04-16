using System.IO;
using System.Text.Json;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.Core.Services;

public class LocalizationManager : ILocalizationManager
{
    private static LocalizationManager? _instance;
    private readonly Dictionary<string, Dictionary<string, string>> _languages = new();
    private string _currentLanguage = "en";
    private Dictionary<string, string> _currentStrings = new();

    public static ILocalizationManager Instance => _instance ??= new LocalizationManager();

    public string CurrentLanguage => _currentLanguage;
    public bool IsRightToLeft => _currentLanguage == "ar";

    public string this[string key]
    {
        get
        {
            if (_currentStrings.TryGetValue(key, out var value)) return value;
            if (_languages.TryGetValue("en", out var enStrings) && enStrings.TryGetValue(key, out var enValue)) return enValue;
            return key;
        }
    }

    public void SetLanguage(string languageCode)
    {
        _currentLanguage = languageCode;
        if (_languages.TryGetValue(languageCode, out var strings))
            _currentStrings = strings;
        else if (_languages.TryGetValue("en", out var enStrings))
            _currentStrings = enStrings;
    }

    public void Reload()
    {
        _languages.Clear();
        var langDir = Path.Combine(AppContext.BaseDirectory, "Lang");
        if (!Directory.Exists(langDir)) return;

        foreach (var file in Directory.GetFiles(langDir, "*.json"))
        {
            var langCode = Path.GetFileNameWithoutExtension(file);
            var json = File.ReadAllText(file);
            var strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (strings != null)
                _languages[langCode] = strings;
        }

        SetLanguage(_currentLanguage);
    }
}
