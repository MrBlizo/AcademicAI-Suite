using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class MathSolverView : UserControl
{
    public MathSolverView()
    {
        InitializeComponent();
        DataContext = new MathSolverViewModel();
    }
}
