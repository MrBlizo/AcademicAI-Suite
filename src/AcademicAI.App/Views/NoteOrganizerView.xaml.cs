using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class NoteOrganizerView : Page
{
    public NoteOrganizerView()
    {
        InitializeComponent();
        DataContext = new NoteOrganizerViewModel();
    }
}
