using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class ParaphraserView : UserControl
{
    public ParaphraserView()
    {
        InitializeComponent();
        DataContext = new ParaphraserViewModel();
    }
}
