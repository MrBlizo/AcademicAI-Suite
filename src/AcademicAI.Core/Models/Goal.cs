using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademicAI.Core.Models;

public partial class Goal : ObservableObject
{
    [ObservableProperty] private Guid _id = Guid.NewGuid();
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private double _targetValue = 1;
    [ObservableProperty] private double _currentValue = 0;
    [ObservableProperty] private string _unit = "";
    [ObservableProperty] private string _category = "Study";
    [ObservableProperty] private DateTime? _deadline;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
}
