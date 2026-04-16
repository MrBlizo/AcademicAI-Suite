using System.Collections.Concurrent;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Core.Services;

public class NotificationService : INotificationService
{
    private readonly ConcurrentQueue<NotificationMessage> _notifications = new();
    private const int MaxNotifications = 50;

    public event Action<NotificationMessage>? OnNotification;

    public void Show(string title, string message)
    {
        var notification = new NotificationMessage { Title = title, Message = message };
        _notifications.Enqueue(notification);

        while (_notifications.Count > MaxNotifications)
            _notifications.TryDequeue(out _);

        OnNotification?.Invoke(notification);
    }

    public IReadOnlyList<NotificationMessage> GetRecent(int count)
    {
        return _notifications.ToArray().TakeLast(count).ToList().AsReadOnly();
    }
}
