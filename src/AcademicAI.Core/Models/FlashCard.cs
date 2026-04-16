namespace AcademicAI.Core.Models;

public class FlashCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public string Hint { get; set; } = "";
    public string Rating { get; set; } = "New";
    public int ReviewCount { get; set; } = 0;
}
