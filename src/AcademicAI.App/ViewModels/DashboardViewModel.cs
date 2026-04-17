using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace AcademicAI.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DispatcherTimer _pomodoroTimer;
    private readonly DispatcherTimer _greetingTimer;
    private TimeSpan _remainingTime;
    private DispatcherTimer? _clipboardTimer;
    private string _lastClipboardText = "";

    [ObservableProperty] private int _aiUsageCalls;
    [ObservableProperty] private int _aiUsageTokens;
    [ObservableProperty] private bool _isTimerRunning;
    [ObservableProperty] private string _timerDisplay = "25:00";
    [ObservableProperty] private int _pomodoroWorkMinutes = 25;
    [ObservableProperty] private int _pomodoroBreakMinutes = 5;
    [ObservableProperty] private int _completedSessions;
    [ObservableProperty] private bool _isBreak;
    [ObservableProperty] private string _newGoalTitle = "";
    [ObservableProperty] private string _newGoalTarget = "";
    [ObservableProperty] private string _newGoalUnit = "";
    [ObservableProperty] private string _newGoalCategory = "Study";
    [ObservableProperty] private bool _isClipboardMonitorEnabled;
    [ObservableProperty] private int _studyStreak = 1;

    public string StreakText => StudyStreak == 1 ? "1 day — Keep going!" : $"{StudyStreak} days — Amazing!";

    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            var timeGreeting = hour switch
            {
                < 6 => "Good night",
                < 12 => "Good morning",
                < 18 => "Good afternoon",
                _ => "Good evening"
            };
            return $"{timeGreeting}! Ready to study?";
        }
    }

    public double StudyMasteryPercent
    {
        get
        {
            if (Goals.Count == 0) return 0;
            var completed = Goals.Count(g => g.CurrentValue >= g.TargetValue);
            return (double)completed / Goals.Count * 100;
        }
    }

    public ObservableCollection<Assignment> UpcomingDeadlines { get; } = [];
    public ObservableCollection<Goal> Goals { get; } = [];
    public ObservableCollection<ClipboardEntry> ClipboardEntries { get; } = [];
    public ObservableCollection<ActivityItem> RecentActivity { get; } = [];

    [ObservableProperty] private bool _showQuickActions = true;

    public DashboardViewModel()
    {
        _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _pomodoroTimer.Tick += OnTimerTick;
        _remainingTime = TimeSpan.FromMinutes(PomodoroWorkMinutes);
        TimerDisplay = _remainingTime.ToString(@"mm\:ss");

        _greetingTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _greetingTimer.Tick += (_, _) => OnPropertyChanged(nameof(Greeting));
        _greetingTimer.Start();

        _clipboardTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clipboardTimer.Tick += CheckClipboard;

        Goals.CollectionChanged += (_, _) => OnPropertyChanged(nameof(StudyMasteryPercent));

        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        try
        {
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            IsClipboardMonitorEnabled = settingsService.Settings.ClipboardMonitorEnabled;

            var tokenTracker = App.Services.GetRequiredService<ITokenTrackerService>();
            var usage = tokenTracker.GetTotalUsage();
            AiUsageCalls = usage.TotalCalls;
            AiUsageTokens = usage.TotalPromptTokens + usage.TotalCompletionTokens;
        }
        catch { }

        RecentActivity.Add(new ActivityItem { Icon = SymbolRegular.DocumentAdd24, Title = "Generated flashcards on Machine Learning", Detail = "Study Hub", Timestamp = DateTime.Now.AddMinutes(-10) });
        RecentActivity.Add(new ActivityItem { Icon = SymbolRegular.Translate24, Title = "Translated document to French", Detail = "Text Tools", Timestamp = DateTime.Now.AddMinutes(-45) });
        RecentActivity.Add(new ActivityItem { Icon = SymbolRegular.DocumentText24, Title = "Summarized research paper", Detail = "Text Tools", Timestamp = DateTime.Now.AddHours(-2) });
        RecentActivity.Add(new ActivityItem { Icon = SymbolRegular.DocumentBulletList24, Title = "Created 5 citations in APA format", Detail = "Research", Timestamp = DateTime.Now.AddHours(-5) });

        UpcomingDeadlines.Clear();
        UpcomingDeadlines.Add(new Assignment { Title = "Research Paper Draft", Subject = "CS", DueDate = DateTime.Today.AddDays(2), Priority = "High" });
        UpcomingDeadlines.Add(new Assignment { Title = "Math Problem Set", Subject = "Math", DueDate = DateTime.Today.AddDays(5), Priority = "Medium" });
        UpcomingDeadlines.Add(new Assignment { Title = "Lab Report", Subject = "Physics", DueDate = DateTime.Today.AddDays(7), Priority = "Low" });
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_remainingTime.TotalSeconds <= 0)
        {
            _pomodoroTimer.Stop();
            IsTimerRunning = false;

            var notificationService = App.Services.GetRequiredService<INotificationService>();

            if (!IsBreak)
            {
                CompletedSessions++;
                notificationService.Show("Session Complete", $"Great work! You've completed {CompletedSessions} sessions.");
                IsBreak = true;
                _remainingTime = TimeSpan.FromMinutes(PomodoroBreakMinutes);
            }
            else
            {
                notificationService.Show("Break Over", "Time to get back to work!");
                IsBreak = false;
                _remainingTime = TimeSpan.FromMinutes(PomodoroWorkMinutes);
            }

            UpdateTimerDisplay();
            return;
        }

        _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        var mins = (int)_remainingTime.TotalMinutes;
        var secs = (int)_remainingTime.Seconds;
        TimerDisplay = $"{mins:D2}:{secs:D2}";
    }

    [RelayCommand]
    private void StartTimer()
    {
        if (IsTimerRunning) return;
        _remainingTime = IsBreak
            ? TimeSpan.FromMinutes(PomodoroBreakMinutes)
            : TimeSpan.FromMinutes(PomodoroWorkMinutes);
        UpdateTimerDisplay();
        IsTimerRunning = true;
        _pomodoroTimer.Start();
    }

    [RelayCommand]
    private void PauseTimer()
    {
        IsTimerRunning = false;
        _pomodoroTimer.Stop();
    }

    [RelayCommand]
    private void SkipTimer()
    {
        _pomodoroTimer.Stop();
        IsTimerRunning = false;

        if (!IsBreak)
        {
            CompletedSessions++;
            IsBreak = true;
            _remainingTime = TimeSpan.FromMinutes(PomodoroBreakMinutes);
        }
        else
        {
            IsBreak = false;
            _remainingTime = TimeSpan.FromMinutes(PomodoroWorkMinutes);
        }

        UpdateTimerDisplay();
    }

    [RelayCommand]
    private void AddGoal()
    {
        if (string.IsNullOrWhiteSpace(NewGoalTitle) || !double.TryParse(NewGoalTarget, out var target)) return;

        Goals.Add(new Goal
        {
            Title = NewGoalTitle.Trim(),
            TargetValue = target,
            Unit = NewGoalUnit.Trim(),
            Category = NewGoalCategory
        });

        NewGoalTitle = "";
        NewGoalTarget = "";
        NewGoalUnit = "";
        NewGoalCategory = "Study";
    }

    [RelayCommand]
    private void IncrementGoal(Goal goal)
    {
        if (goal == null) return;
        goal.CurrentValue = Math.Min(goal.CurrentValue + 1, goal.TargetValue);
        OnPropertyChanged(nameof(StudyMasteryPercent));
    }

    [RelayCommand]
    private void DeleteGoal(Goal goal)
    {
        if (goal != null) Goals.Remove(goal);
    }

    partial void OnIsClipboardMonitorEnabledChanged(bool value)
    {
        if (value)
            _clipboardTimer?.Start();
        else
            _clipboardTimer?.Stop();
    }

    private void CheckClipboard(object? sender, EventArgs e)
    {
        try
        {
            if (!Clipboard.ContainsText()) return;
            var text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text) || text == _lastClipboardText) return;
            _lastClipboardText = text;
            var entry = new ClipboardEntry { Text = text.Length > 200 ? text[..200] : text, CapturedAt = DateTime.Now };
            ClipboardEntries.Insert(0, entry);
            if (ClipboardEntries.Count > 50) ClipboardEntries.RemoveAt(ClipboardEntries.Count - 1);
        }
        catch { }
    }

    [RelayCommand]
    private void ToggleClipboard()
    {
        IsClipboardMonitorEnabled = !IsClipboardMonitorEnabled;
        try
        {
            var settingsService = App.Services.GetRequiredService<IAppSettingsService>();
            settingsService.Settings.ClipboardMonitorEnabled = IsClipboardMonitorEnabled;
            settingsService.Save();
        }
        catch { }
    }

    [RelayCommand]
    private void ClearClipboard()
    {
        ClipboardEntries.Clear();
    }

    [RelayCommand]
    private void CopyClipboardEntry(ClipboardEntry entry)
    {
        if (entry != null) Clipboard.SetText(entry.Text);
    }

    public class ActivityItem
    {
        public SymbolRegular Icon { get; set; } = SymbolRegular.DocumentText24;
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
