using System.IO;
using System.Text.Json;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Core.Services;

public class AppSettingsService : IAppSettingsService
{
    private readonly string _filePath;
    private readonly ISecretStore _secretStore;
    private AppSettings _settings = new();

    public AppSettings Settings => _settings;

    public AppSettingsService(ISecretStore secretStore)
    {
        _secretStore = secretStore;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcademicAI");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "settings.json");
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            _settings = new AppSettings();
            Save();
            return;
        }

        var json = File.ReadAllText(_filePath);
        _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public FeatureProviderConfig? GetFeatureDefault(string feature)
    {
        return _settings.FeatureDefaults.TryGetValue(feature, out var config) ? config : null;
    }

    public void SetFeatureDefault(string feature, string provider, string model)
    {
        if (_settings.FeatureDefaults.ContainsKey(feature))
        {
            _settings.FeatureDefaults[feature].Provider = provider;
            _settings.FeatureDefaults[feature].Model = model;
        }
        else
        {
            _settings.FeatureDefaults[feature] = new FeatureProviderConfig
            {
                Feature = feature,
                Provider = provider,
                Model = model
            };
        }
        Save();
    }
}
