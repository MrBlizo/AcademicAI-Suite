using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AcademicAI.App.ViewModels;
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

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.PasswordBox pb && pb.DataContext is ProviderConfigItem item)
            item.ApiKey = pb.Password;
    }
}

public class ConfiguredBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true)
            return Application.Current.FindResource("SystemFillColorSuccessBrush") as Brush ?? Brushes.Green;
        return Application.Current.FindResource("SystemFillColorCriticalBrush") as Brush ?? Brushes.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class ConfiguredTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Application.Current.FindResource("Lbl_Configured") is string configured)
            return value is true ? configured : Application.Current.FindResource("Lbl_NotConfigured") as string ?? "Not Configured";
        return value is true ? "Configured" : "Not Configured";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

