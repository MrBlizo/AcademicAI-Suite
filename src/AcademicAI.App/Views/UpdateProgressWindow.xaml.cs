using System.Windows;

namespace AcademicAI.App.Views;

public partial class UpdateProgressWindow : Window
{
    private readonly string _version;

    public UpdateProgressWindow(string version)
    {
        _version = version;
        InitializeComponent();
        VersionText.Text = $"Downloading update {_version}...";
    }

    public void UpdateProgress(double percent)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBar.Value = percent;
            PercentText.Text = $"{percent:F0}%";
        });
    }
}
