using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AcademicAI.Core.Interfaces;
using AcademicAI.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace AcademicAI.App.Views;

public partial class SettingsView : Page
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateThemeButtons();
        SelectCurrentLanguage();
        if (DataContext is SettingsViewModel vm)
            vm.RefreshUsage();
        LoadProviderBadges();
    }

    private void LoadProviderBadges()
    {
        try
        {
            var secretStore = App.Services.GetRequiredService<ISecretStore>();
            UpdateBadge("OpenRouter", secretStore);
            UpdateBadge("Fireworks", secretStore);

            var orKey = secretStore.Load("apikey_OpenRouter");
            if (orKey != null) OpenRouterKeyBox.Password = orKey;

            var fwKey = secretStore.Load("apikey_Fireworks");
            if (fwKey != null) FireworksKeyBox.Password = fwKey;
        }
        catch { }
    }

    private void UpdateBadge(string provider, ISecretStore secretStore)
    {
        var hasKey = !string.IsNullOrEmpty(secretStore.Load($"apikey_{provider}"));
        if (provider == "OpenRouter")
        {
            OpenRouterBadge.Background = hasKey
                ? Application.Current.FindResource("SystemFillColorSuccessBrush") as Brush ?? Brushes.Green
                : Application.Current.FindResource("SystemFillColorCriticalBrush") as Brush ?? Brushes.Red;
            OpenRouterBadgeText.Text = hasKey
                ? Application.Current.FindResource("Lbl_Configured") as string ?? "Configured"
                : Application.Current.FindResource("Lbl_NotConfigured") as string ?? "Not Configured";
        }
        else
        {
            FireworksBadge.Background = hasKey
                ? Application.Current.FindResource("SystemFillColorSuccessBrush") as Brush ?? Brushes.Green
                : Application.Current.FindResource("SystemFillColorCriticalBrush") as Brush ?? Brushes.Red;
            FireworksBadgeText.Text = hasKey
                ? Application.Current.FindResource("Lbl_Configured") as string ?? "Configured"
                : Application.Current.FindResource("Lbl_NotConfigured") as string ?? "Not Configured";
        }
    }

    private void SaveOpenRouterKey_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = OpenRouterKeyBox.Password.Trim();
            if (string.IsNullOrWhiteSpace(key)) return;
            var secretStore = App.Services.GetRequiredService<ISecretStore>();
            secretStore.Save("apikey_OpenRouter", key);
            UpdateBadge("OpenRouter", secretStore);
            OpenRouterTestResult.Text = "Key saved successfully";
            OpenRouterTestResult.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            OpenRouterTestResult.Text = $"Failed: {ex.Message}";
            OpenRouterTestResult.Visibility = Visibility.Visible;
        }
    }

    private async void TestOpenRouter_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var apiKey = OpenRouterKeyBox.Password.Trim();
            if (string.IsNullOrWhiteSpace(apiKey)) return;
            OpenRouterTestResult.Text = "Testing connection...";
            OpenRouterTestResult.Visibility = Visibility.Visible;
            var agentFactory = App.Services.GetRequiredService<IAgentFactory>();
            var agent = agentFactory.GetAgent("OpenRouter");
            agent.SetApiKey(apiKey);
            var (success, error) = await agent.TestConnectionWithDetailsAsync();
            OpenRouterTestResult.Text = success ? "Connection successful!" : $"Failed: {error}";
        }
        catch (Exception ex)
        {
            OpenRouterTestResult.Text = $"Test failed: {ex.Message}";
        }
    }

    private void SaveFireworksKey_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = FireworksKeyBox.Password.Trim();
            if (string.IsNullOrWhiteSpace(key)) return;
            var secretStore = App.Services.GetRequiredService<ISecretStore>();
            secretStore.Save("apikey_Fireworks", key);
            UpdateBadge("Fireworks", secretStore);
            FireworksTestResult.Text = "Key saved successfully";
            FireworksTestResult.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            FireworksTestResult.Text = $"Failed: {ex.Message}";
            FireworksTestResult.Visibility = Visibility.Visible;
        }
    }

    private async void TestFireworks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var apiKey = FireworksKeyBox.Password.Trim();
            if (string.IsNullOrWhiteSpace(apiKey)) return;
            FireworksTestResult.Text = "Testing connection...";
            FireworksTestResult.Visibility = Visibility.Visible;
            var agentFactory = App.Services.GetRequiredService<IAgentFactory>();
            var agent = agentFactory.GetAgent("Fireworks");
            agent.SetApiKey(apiKey);
            var (success, error) = await agent.TestConnectionWithDetailsAsync();
            FireworksTestResult.Text = success ? "Connection successful!" : $"Failed: {error}";
        }
        catch (Exception ex)
        {
            FireworksTestResult.Text = $"Test failed: {ex.Message}";
        }
    }

    private void DarkThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && vm.SelectedTheme != "Dark")
        {
            vm.ToggleThemeCommand.Execute(null);
            UpdateThemeButtons();
        }
    }

    private void LightThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && vm.SelectedTheme != "Light")
        {
            vm.ToggleThemeCommand.Execute(null);
            UpdateThemeButtons();
        }
    }

    private void UpdateThemeButtons()
    {
        if (DataContext is SettingsViewModel vm)
        {
            DarkThemeButton.Appearance = vm.SelectedTheme == "Dark"
                ? ControlAppearance.Primary
                : ControlAppearance.Secondary;
            LightThemeButton.Appearance = vm.SelectedTheme == "Light"
                ? ControlAppearance.Primary
                : ControlAppearance.Secondary;
        }
    }

    private void SelectCurrentLanguage()
    {
        if (DataContext is not SettingsViewModel vm) return;
        foreach (ComboBoxItem item in LanguageComboBox.Items)
        {
            if (item.Tag is string tag && tag == vm.SelectedLanguage)
            {
                LanguageComboBox.SelectedItem = item;
                return;
            }
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            vm.SelectedLanguage = tag;
            vm.SaveLanguageCommand.Execute(null);
        }
    }
}
