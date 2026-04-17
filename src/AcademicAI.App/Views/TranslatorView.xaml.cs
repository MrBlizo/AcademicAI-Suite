using System.Windows.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class TranslatorView : UserControl
{
    public TranslatorView()
    {
        InitializeComponent();
        DataContext = new TranslatorViewModel();
    }

    private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = InputBox.Text ?? "";
        CharCountLabel.Text = $"{text.Length} characters";
    }
}
