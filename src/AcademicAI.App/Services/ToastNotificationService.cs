using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.App.Services;

public class ToastNotificationService : INotificationService
{
    private readonly DispatcherTimer _cleanupTimer;
    private readonly ConcurrentQueue<NotificationMessage> _notifications = new();
    private readonly List<NotificationMessage> _pendingNotifications = new();
    private const int MaxNotifications = 50;
    private Panel? _toastContainer;

    public event Action<NotificationMessage>? OnNotification;

    public ToastNotificationService()
    {
        _cleanupTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
    }

    public void Initialize(Panel container)
    {
        _toastContainer = container;
        foreach (var notification in _pendingNotifications)
            ShowToast(notification);
        _pendingNotifications.Clear();
    }

    public void Show(string title, string message)
    {
        var notification = new NotificationMessage { Title = title, Message = message };
        _notifications.Enqueue(notification);
        while (_notifications.Count > MaxNotifications) _notifications.TryDequeue(out _);
        OnNotification?.Invoke(notification);
        ShowToast(notification);
    }

    public IReadOnlyList<NotificationMessage> GetRecent(int count)
    {
        return _notifications.ToArray().TakeLast(count).ToList().AsReadOnly();
    }

    private void ShowToast(NotificationMessage notification)
    {
        if (_toastContainer == null)
        {
            _pendingNotifications.Add(notification);
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = CreateToastElement(notification);
            _toastContainer.Children.Add(toast);

            var dismissTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            dismissTimer.Tick += (_, _) =>
            {
                dismissTimer.Stop();
                _toastContainer.Children.Remove(toast);
            };
            dismissTimer.Start();
        });
    }

    private Border CreateToastElement(NotificationMessage notification)
    {
        return new Border
        {
            Background = Application.Current.FindResource("ControlFillColorDefaultBrush") as System.Windows.Media.Brush,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 8),
            BorderBrush = Application.Current.FindResource("ControlStrokeColorDefaultBrush") as System.Windows.Media.Brush,
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = notification.Title,
                        FontSize = 13,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Application.Current.FindResource("TextFillColorPrimaryBrush") as System.Windows.Media.Brush
                    },
                    new TextBlock
                    {
                        Text = notification.Message,
                        FontSize = 12,
                        Foreground = Application.Current.FindResource("TextFillColorSecondaryBrush") as System.Windows.Media.Brush,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 4, 0, 0)
                    }
                }
            }
        };
    }
}
