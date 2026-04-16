using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class ParaphraserView : Page
{
    public ParaphraserView()
    {
        InitializeComponent();
        DataContext = new ParaphraserViewModel();
    }
}
