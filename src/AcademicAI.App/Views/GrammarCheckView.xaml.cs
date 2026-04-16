using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class GrammarCheckView : Page
{
    public GrammarCheckView()
    {
        InitializeComponent();
        DataContext = new GrammarCheckViewModel();
    }
}
