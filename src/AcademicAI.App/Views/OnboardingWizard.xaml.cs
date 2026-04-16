using System.Windows;
using AcademicAI.Agents;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.App.Views;

public partial class OnboardingWizard : Window
{
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;
    private readonly IAgentFactory _agentFactory;
    private string _selectedProvider = "OpenRouter";
    private string _selectedLanguage = "en";

    public OnboardingWizard(IAppSettingsService settingsService, ISecretStore secretStore, IAgentFactory agentFactory)
    {
        _settingsService = settingsService;
        _secretStore = secretStore;
        _agentFactory = agentFactory;
        InitializeComponent();
        LoadProviders();
        LoadLanguages();
    }

    private void LoadProviders()
    {
        foreach (var provider in _agentFactory.GetAvailableProviders())
            ProviderCombo.Items.Add(provider);
        ProviderCombo.SelectedIndex = 0;
    }

    private void LoadLanguages()
    {
        LanguageCombo.Items.Add("English");
        LanguageCombo.Items.Add("العربية");
        LanguageCombo.Items.Add("Français");
        LanguageCombo.Items.Add("Español");
        LanguageCombo.Items.Add("Deutsch");
        LanguageCombo.SelectedIndex = 0;
    }

    private void ProviderCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ProviderCombo.SelectedItem is string provider)
        {
            _selectedProvider = provider;
            ModelCombo.Items.Clear();
            foreach (var model in _agentFactory.GetModelsForProvider(provider))
                ModelCombo.Items.Add(model);
            if (ModelCombo.Items.Count > 0) ModelCombo.SelectedIndex = 0;

            CloudflareFields.Visibility = provider == "Cloudflare" ? Visibility.Visible : Visibility.Collapsed;
            StandardKeyField.Visibility = provider != "Cloudflare" ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void LanguageCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _selectedLanguage = LanguageCombo.SelectedIndex switch
        {
            0 => "en", 1 => "ar", 2 => "fr", 3 => "es", 4 => "de", _ => "en"
        };
    }

    private void SkipBtn_Click(object sender, RoutedEventArgs e)
    {
        _settingsService.Settings.Language = _selectedLanguage;
        _settingsService.Save();
        Close();
    }

    private void GetStartedBtn_Click(object sender, RoutedEventArgs e)
    {
        _settingsService.Settings.Language = _selectedLanguage;

        if (_selectedProvider == "Cloudflare")
        {
            var accountId = CloudflareAccountId.Text.Trim();
            var apiToken = CloudflareApiToken.Password.Trim();
            if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(apiToken))
            {
                _secretStore.Save("apikey_Cloudflare", $"{accountId}|{apiToken}");
                _secretStore.Save("cloudflare_accountid", accountId);
                _secretStore.Save("cloudflare_apitoken", apiToken);
            }
        }
        else
        {
            var apiKey = ApiKeyBox.Password.Trim();
            if (!string.IsNullOrEmpty(apiKey))
                _secretStore.Save($"apikey_{_selectedProvider}", apiKey);
        }

        if (ModelCombo.SelectedItem is string model)
        {
            foreach (var feature in _settingsService.Settings.FeatureDefaults.Keys.ToList())
                _settingsService.Settings.FeatureDefaults[feature] = new Core.Models.FeatureProviderConfig
                {
                    Feature = feature,
                    Provider = _selectedProvider,
                    Model = model
                };
        }

        _settingsService.Save();
        Close();
    }
}
