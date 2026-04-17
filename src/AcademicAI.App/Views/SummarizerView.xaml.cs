using System.Windows;
using System.Windows.Controls;
using AcademicAI.App.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class SummarizerView : UserControl
{
    public SummarizerView()
    {
        InitializeComponent();
        var vm = new SummarizerViewModel();
        DataContext = vm;
        PdfDrop.PdfTextExtracted += OnPdfTextExtracted;
    }

    private void OnPdfTextExtracted(object sender, RoutedEventArgs e)
    {
        if (DataContext is SummarizerViewModel vm)
        {
            vm.IsPdfMode = true;
            vm.PdfText = PdfDrop.ExtractedText;
        }
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
