namespace AcademicAI.Core.Models;

public class ClipboardEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = "";
    public string? Summary { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.Now;
}
