using System.Collections.ObjectModel;
using System.Windows;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class FeatureDefaultItem : ObservableObject
{
    [ObservableProperty] private string _feature = "";
    [ObservableProperty] private string _provider = "OpenRouter";
    [ObservableProperty] private string _model = "openai/gpt-4o-mini";

    public List<string> AvailableProviders { get; }
    public List<string> AvailableModels { get; private set; }

    public FeatureDefaultItem(string feature, string provider, string model, IAgentFactory agentFactory)
    {
        Feature = feature;
        Provider = provider;
        Model = model;
        AvailableProviders = new List<string>(agentFactory.GetAvailableProviders());
        AvailableModels = new List<string>(agentFactory.GetModelsForProvider(provider));
    }

    partial void OnProviderChanged(string value)
    {
        try
        {
            var agentFactory = App.Services.GetRequiredService<IAgentFactory>();
            AvailableModels = new List<string>(agentFactory.GetModelsForProvider(value));
            Model = AvailableModels.FirstOrDefault() ?? "";
            OnPropertyChanged(nameof(AvailableModels));
        }
        catch { }
    }
}

public partial class ProviderConfigItem : ObservableObject
{
    [ObservableProperty] private string _providerName = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private bool _isConfigured;
    [ObservableProperty] private string _apiKey = "";
    [ObservableProperty] private string _testResult = "";

    public bool HasApiKey => !string.IsNullOrEmpty(ApiKey);

    public ProviderConfigItem(string name, string description, ISecretStore secretStore)
    {
        ProviderName = name;
        Description = description;
        var key = secretStore.Load($"apikey_{name}");
        IsConfigured = !string.IsNullOrEmpty(key);
        ApiKey = key ?? "";
    }
}

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _selectedLanguage = "en";
    [ObservableProperty] private string _selectedTheme = "Dark";
    [ObservableProperty] private bool _minimizeToTray;
    [ObservableProperty] private bool _clipboardMonitorEnabled;

    partial void OnMinimizeToTrayChanged(bool value) => SaveSettings();
    partial void OnClipboardMonitorEnabledChanged(bool value) => SaveSettings();

    private void SaveSettings()
    {
        try
        {
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            settingsService.Settings.MinimizeToTray = MinimizeToTray;
            settingsService.Settings.ClipboardMonitorEnabled = ClipboardMonitorEnabled;
            settingsService.Save();
        }
        catch { }
    }
    [ObservableProperty] private string _licenseKey = "";
    [ObservableProperty] private string _appVersion = "";
    [ObservableProperty] private string _lastUpdateCheck = "Never";
    [ObservableProperty] private int _totalCalls;
    [ObservableProperty] private int _totalTokens;
    [ObservableProperty] private string _estimatedCost = "$0.00";

    public ObservableCollection<FeatureDefaultItem> FeatureDefaultItems { get; } = [];
    public ObservableCollection<ProviderConfigItem> ProviderConfigItems { get; } = [];

    private static readonly List<(string Name, string Description)> Providers =
    [
        ("OpenRouter", "Access multiple AI models through one API"),
        ("Fireworks", "Fast open-source model inference")
    ];

    public SettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            var settings = settingsService.Settings;
            SelectedLanguage = settings.Language;
            SelectedTheme = settings.Theme;
            MinimizeToTray = settings.MinimizeToTray;
            ClipboardMonitorEnabled = settings.ClipboardMonitorEnabled;

            var licenseService = App.Services.GetRequiredService<ILicenseService>();
            var licenseInfo = licenseService.GetLicenseInfo();
            LicenseKey = licenseInfo.Key;

            AppVersion = typeof(SettingsViewModel).Assembly.GetName().Version?.ToString() ?? "1.0.0";

            var tokenTracker = App.Services.GetRequiredService<ITokenTrackerService>();
            var usage = tokenTracker.GetTotalUsage();
            TotalCalls = usage.TotalCalls;
            TotalTokens = usage.TotalPromptTokens + usage.TotalCompletionTokens;
            EstimatedCost = $"${usage.EstimatedCost:F4}";

            var agentFactory = App.Services.GetRequiredService<IAgentFactory>();
            var secretStore = App.Services.GetRequiredService<ISecretStore>();

            FeatureDefaultItems.Clear();
            foreach (var fd in settings.FeatureDefaults.Values)
            {
                FeatureDefaultItems.Add(new FeatureDefaultItem(fd.Feature, fd.Provider, fd.Model, agentFactory));
            }

            ProviderConfigItems.Clear();
            foreach (var (name, desc) in Providers)
            {
                ProviderConfigItems.Add(new ProviderConfigItem(name, desc, secretStore));
            }
        }
        catch { }
    }

    [RelayCommand]
    private void SaveLanguage()
    {
        try
        {
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            settingsService.Settings.Language = SelectedLanguage;
            settingsService.Save();
        }
        catch { }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        try
        {
            var newTheme = SelectedTheme.Equals("Light", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
            SelectedTheme = newTheme;
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            settingsService.Settings.Theme = newTheme;
            settingsService.Save();
            App.ApplyTheme(newTheme);
        }
        catch { }
    }

    [RelayCommand]
    private void SaveFeatureDefaults()
    {
        try
        {
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            foreach (var item in FeatureDefaultItems)
            {
                settingsService.SetFeatureDefault(item.Feature, item.Provider, item.Model);
            }
            settingsService.Save();
        }
        catch { }
    }

    [RelayCommand]
    private void SaveProviderKey(ProviderConfigItem item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.ApiKey)) return;
        try
        {
            var secretStore = App.Services.GetRequiredService<ISecretStore>();
            secretStore.Save($"apikey_{item.ProviderName}", item.ApiKey.Trim());
            item.IsConfigured = true;
            item.TestResult = "Key saved successfully";
        }
        catch (Exception ex)
        {
            item.TestResult = $"Failed to save: {ex.Message}";
        }
    }

    public string MaskedLicenseKey
    {
        get
        {
            if (string.IsNullOrEmpty(LicenseKey) || LicenseKey.Length < 8) return LicenseKey;
            return LicenseKey[..4] + "****-****" + LicenseKey[^4..];
        }
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        try
        {
            var remoteService = App.Services.GetRequiredService<IRemoteControlService>();
            var result = await remoteService.CheckAsync();
            LastUpdateCheck = DateTime.Now.ToString("g");
            if (result.HasUpdate)
            {
                var notificationService = App.Services.GetRequiredService<INotificationService>();
                notificationService.Show("Update Available", $"Version {result.LatestVersion} is available.");
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task TestProvider(ProviderConfigItem item)
    {
        if (item == null) return;
        try
        {
            var agentFactory = App.Services.GetRequiredService<IAgentFactory>();
            var agent = agentFactory.GetAgent(item.ProviderName);
            agent.SetApiKey(item.ApiKey);
            var (success, error) = await agent.TestConnectionWithDetailsAsync();
            item.TestResult = success ? "Connection successful!" : $"Failed: {error}";
        }
        catch (Exception ex)
        {
            item.TestResult = $"Test failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CopyLicense()
    {
        if (!string.IsNullOrEmpty(LicenseKey))
            Clipboard.SetText(LicenseKey);
    }
}
