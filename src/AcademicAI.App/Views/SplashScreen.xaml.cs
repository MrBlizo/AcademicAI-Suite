using System.Windows;

namespace AcademicAI.App.Views;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();
        VersionText.Text = $"v{typeof(App).Assembly.GetName().Version?.ToString() ?? "3.0.0"}";
    }

    public void UpdateStatus(string status, double progress)
    {
        StatusText.Text = status;
        ProgressBar.Value = progress;
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, () => { });
    }
}
