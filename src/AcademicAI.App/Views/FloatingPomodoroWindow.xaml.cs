using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AcademicAI.App.Views;

public partial class FloatingPomodoroWindow : Window
{
    private readonly DispatcherTimer _timer;
    private int _remainingSeconds;
    private bool _isBreak;

    public FloatingPomodoroWindow()
    {
        InitializeComponent();
        _remainingSeconds = 25 * 60;
        _isBreak = false;
        UpdateDisplay();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        MouseLeftButtonDown += (_, _) => DragMove();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingSeconds <= 0)
        {
            _timer.Stop();
            _isBreak = !_isBreak;
            _remainingSeconds = _isBreak ? 5 * 60 : 25 * 60;
            StatusText.Text = _isBreak ? "Break" : "Work";
            ToggleBtn.Content = "Start";
            UpdateDisplay();
            return;
        }
        _remainingSeconds--;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var mins = _remainingSeconds / 60;
        var secs = _remainingSeconds % 60;
        TimerText.Text = $"{mins:D2}:{secs:D2}";
    }

    private void ToggleBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            ToggleBtn.Content = "Resume";
        }
        else
        {
            _timer.Start();
            ToggleBtn.Content = "Pause";
        }
    }

    private void SkipBtn_Click(object sender, RoutedEventArgs e)
    {
        _isBreak = !_isBreak;
        _remainingSeconds = _isBreak ? 5 * 60 : 25 * 60;
        StatusText.Text = _isBreak ? "Break" : "Work";
        _timer.Stop();
        ToggleBtn.Content = "Start";
        UpdateDisplay();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        Close();
    }
}
