using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademicAI.Core.Models;

public partial class Assignment : ObservableObject
{
    [ObservableProperty] private Guid _id = Guid.NewGuid();
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _subject = "";
    [ObservableProperty] private string _category = "Homework";
    [ObservableProperty] private DateTime _dueDate = DateTime.Today;
    [ObservableProperty] private string _priority = "Medium";
    [ObservableProperty] private string _notes = "";
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
}
