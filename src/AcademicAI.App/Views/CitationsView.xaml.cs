using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class CitationsView : UserControl
{
    public CitationsView()
    {
        InitializeComponent();
        DataContext = new CitationsViewModel();
    }
}
