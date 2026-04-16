using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.App.Views;

public partial class OnboardingWizard : Window
{
    private readonly IAppSettingsService _settingsService;
    private readonly ISecretStore _secretStore;
    private readonly IAgentFactory _agentFactory;
    private string _selectedProvider = "OpenRouter";
    private string _selectedLanguage = "en";
    private int _currentStep = 1;

    private static readonly SolidColorBrush ActiveBrush = new(Color.FromRgb(0x63, 0x66, 0xF1));
    private static readonly SolidColorBrush InactiveBrush = SystemColors.ControlBrush;

    public OnboardingWizard(IAppSettingsService settingsService, ISecretStore secretStore, IAgentFactory agentFactory)
    {
        _settingsService = settingsService;
        _secretStore = secretStore;
        _agentFactory = agentFactory;
        InitializeComponent();
        LoadProviders();
        LoadLanguages();
        UpdateStepUI();
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

    private void ProviderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProviderCombo.SelectedItem is string provider)
        {
            _selectedProvider = provider;
            ModelCombo.Items.Clear();
            foreach (var model in _agentFactory.GetModelsForProvider(provider))
                ModelCombo.Items.Add(model);
            if (ModelCombo.Items.Count > 0) ModelCombo.SelectedIndex = 0;
        }
    }

    private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedLanguage = LanguageCombo.SelectedIndex switch
        {
            0 => "en", 1 => "ar", 2 => "fr", 3 => "es", 4 => "de", _ => "en"
        };
    }

    private void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep < 3)
        {
            _currentStep++;
            UpdateStepUI();
        }
        else
        {
            Finish();
        }
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep > 1)
        {
            _currentStep--;
            UpdateStepUI();
        }
    }

    private void SkipBtn_Click(object sender, RoutedEventArgs e)
    {
        _settingsService.Settings.Language = _selectedLanguage;
        _settingsService.Save();
        Close();
    }

    private void UpdateStepUI()
    {
        Step1Content.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
        Step2Content.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
        Step3Content.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

        Step1Circle.Background = _currentStep >= 1 ? ActiveBrush : InactiveBrush;
        Step2Circle.Background = _currentStep >= 2 ? ActiveBrush : InactiveBrush;
        Step3Circle.Background = _currentStep >= 3 ? ActiveBrush : InactiveBrush;
        StepLine1.Background = _currentStep >= 2 ? ActiveBrush : InactiveBrush;
        StepLine2.Background = _currentStep >= 3 ? ActiveBrush : InactiveBrush;

        BackBtn.Visibility = _currentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
        SkipBtn.Visibility = _currentStep < 3 ? Visibility.Visible : Visibility.Collapsed;

        if (_currentStep == 3)
        {
            NextBtnText.Text = "Get Started";
        }
        else
        {
            NextBtnText.Text = "Next";
        }

        TitleText.Text = _currentStep switch
        {
            1 => "Welcome to AcademicAI Suite",
            2 => "Choose Your AI Provider",
            3 => "Enter Your API Key",
            _ => "Welcome to AcademicAI Suite"
        };

        SubtitleText.Text = _currentStep switch
        {
            1 => "Set up your language and AI provider to get started.",
            2 => "Select which AI provider and model you'd like to use.",
            3 => "Your key is encrypted locally and never sent to our servers.",
            _ => ""
        };
    }

    private void Finish()
    {
        _settingsService.Settings.Language = _selectedLanguage;

        var apiKey = ApiKeyBox.Password.Trim();
        if (!string.IsNullOrEmpty(apiKey))
            _secretStore.Save($"apikey_{_selectedProvider}", apiKey);

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
