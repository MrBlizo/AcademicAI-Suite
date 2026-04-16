namespace AcademicAI.Core.Models;

public class PomodoroState
{
    public bool IsRunning { get; set; }
    public bool IsBreak { get; set; }
    public int WorkMinutes { get; set; } = 25;
    public int BreakMinutes { get; set; } = 5;
    public int RemainingSeconds { get; set; }
    public int CompletedSessions { get; set; }
}
