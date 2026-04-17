using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class AcademicWriterView : UserControl
{
    public AcademicWriterView()
    {
        InitializeComponent();
        DataContext = new AcademicWriterViewModel();
    }

    private void PromptBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = PromptBox.Text ?? "";
        var words = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        PromptCountLabel.Text = $"{words} words · {text.Length} chars";
    }
}
