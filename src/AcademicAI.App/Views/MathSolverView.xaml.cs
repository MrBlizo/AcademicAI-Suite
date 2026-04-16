using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class MathSolverView : Page
{
    public MathSolverView()
    {
        InitializeComponent();
        DataContext = new MathSolverViewModel();
    }
}
