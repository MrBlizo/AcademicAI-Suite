using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class GrammarCheckView : UserControl
{
    public GrammarCheckView()
    {
        InitializeComponent();
        DataContext = new GrammarCheckViewModel();
    }
}
