using AcademicAI.Core.Models;

namespace AcademicAI.Core.Interfaces;

public interface INotificationService
{
    void Show(string title, string message);
    IReadOnlyList<NotificationMessage> GetRecent(int count);
    event Action<NotificationMessage>? OnNotification;
}
