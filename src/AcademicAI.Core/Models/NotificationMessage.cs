namespace AcademicAI.Core.Models;

public class NotificationMessage
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
