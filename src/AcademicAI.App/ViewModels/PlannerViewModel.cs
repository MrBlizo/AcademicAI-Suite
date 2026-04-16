using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class PlannerViewModel : ObservableObject
{
    private readonly DispatcherTimer _pomodoroTimer;
    private TimeSpan _remainingTime;

    [ObservableProperty] private string _newTitle = "";
    [ObservableProperty] private string _newSubject = "";
    [ObservableProperty] private string _newCategory = "Homework";
    [ObservableProperty] private DateTime _newDueDate = DateTime.Today;
    [ObservableProperty] private string _newPriority = "Medium";
    [ObservableProperty] private string _newNotes = "";
    [ObservableProperty] private string _filterCategory = "All";
    [ObservableProperty] private string _sortBy = "DueDate";
    [ObservableProperty] private bool _isTimerRunning;
    [ObservableProperty] private string _timerDisplay = "25:00";
    [ObservableProperty] private int _pomodoroWorkMinutes = 25;
    [ObservableProperty] private int _pomodoroBreakMinutes = 5;
    [ObservableProperty] private int _completedSessions;
    [ObservableProperty] private bool _isBreak;

    public ObservableCollection<Assignment> Assignments { get; } = [];

    public bool HasAssignments => Assignments.Count > 0;

    // TODO (P2-32): Duplicate Pomodoro timers exist in DashboardViewModel and PlannerViewModel.
    // Should be refactored into a shared PomodoroService singleton to avoid state duplication.

    public IEnumerable<Assignment> FilteredAssignments
    {
        get
        {
            var filtered = FilterCategory == "All"
                ? Assignments
                : Assignments.Where(a => a.Category == FilterCategory);

            return SortBy switch
            {
                "Priority" => filtered.OrderBy(a => a.Priority).ThenBy(a => a.DueDate),
                "Subject" => filtered.OrderBy(a => a.Subject).ThenBy(a => a.DueDate),
                _ => filtered.OrderBy(a => a.DueDate)
            };
        }
    }

    public PlannerViewModel()
    {
        _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _pomodoroTimer.Tick += OnTimerTick;
        _remainingTime = TimeSpan.FromMinutes(PomodoroWorkMinutes);
        TimerDisplay = _remainingTime.ToString(@"mm\:ss");

        Assignments.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(FilteredAssignments));
            OnPropertyChanged(nameof(HasAssignments));
        };
    }

    partial void OnFilterCategoryChanged(string value) => OnPropertyChanged(nameof(FilteredAssignments));
    partial void OnSortByChanged(string value) => OnPropertyChanged(nameof(FilteredAssignments));

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_remainingTime.TotalSeconds <= 0)
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

            TimerDisplay = _remainingTime.ToString(@"mm\:ss");
            return;
        }

        _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
        TimerDisplay = _remainingTime.ToString(@"mm\:ss");
    }

    [RelayCommand]
    private void AddAssignment()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        Assignments.Add(new Assignment
        {
            Title = NewTitle.Trim(),
            Subject = NewSubject.Trim(),
            Category = NewCategory,
            DueDate = NewDueDate,
            Priority = NewPriority,
            Notes = NewNotes.Trim()
        });

        NewTitle = "";
        NewSubject = "";
        NewCategory = "Homework";
        NewDueDate = DateTime.Today;
        NewPriority = "Medium";
        NewNotes = "";
        OnPropertyChanged(nameof(FilteredAssignments));
    }

    [RelayCommand]
    private void CompleteAssignment(Assignment assignment)
    {
        if (assignment == null) return;
        assignment.IsCompleted = !assignment.IsCompleted;
        OnPropertyChanged(nameof(FilteredAssignments));
    }

    [RelayCommand]
    private void ClearCompleted()
    {
        var completed = Assignments.Where(a => a.IsCompleted).ToList();
        foreach (var a in completed)
            Assignments.Remove(a);
        OnPropertyChanged(nameof(FilteredAssignments));
        OnPropertyChanged(nameof(HasAssignments));
    }

    [RelayCommand]
    private void DeleteAssignment(Assignment assignment)
    {
        if (assignment != null)
        {
            Assignments.Remove(assignment);
            OnPropertyChanged(nameof(FilteredAssignments));
        }
    }

    [RelayCommand]
    private void StartPlannerTimer()
    {
        if (IsTimerRunning) return;
        _remainingTime = IsBreak
            ? TimeSpan.FromMinutes(PomodoroBreakMinutes)
            : TimeSpan.FromMinutes(PomodoroWorkMinutes);
        TimerDisplay = _remainingTime.ToString(@"mm\:ss");
        IsTimerRunning = true;
        _pomodoroTimer.Start();
    }

    [RelayCommand]
    private void PausePlannerTimer()
    {
        IsTimerRunning = false;
        _pomodoroTimer.Stop();
    }

    [RelayCommand]
    private void SkipPlannerTimer()
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

        TimerDisplay = _remainingTime.ToString(@"mm\:ss");
    }

    public string GetUrgency(Assignment a)
    {
        if (a.IsCompleted) return "Completed";
        var daysLeft = (a.DueDate - DateTime.Today).Days;
        if (daysLeft < 0) return "Overdue";
        if (daysLeft <= 1) return "Due Today";
        if (daysLeft <= 3) return "Due Soon";
        return "On Track";
    }
}
