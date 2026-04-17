using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class HumanizerView : UserControl
{
    public HumanizerView()
    {
        InitializeComponent();
        DataContext = new HumanizerViewModel();
    }

    private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = InputBox.Text ?? "";
        var words = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        InputCountLabel.Text = $"{words} words · {text.Length} chars";
    }
}
