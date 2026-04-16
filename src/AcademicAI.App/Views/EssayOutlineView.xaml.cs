using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class EssayOutlineView : Page
{
    public EssayOutlineView()
    {
        InitializeComponent();
        DataContext = new EssayOutlineViewModel();
    }
}
