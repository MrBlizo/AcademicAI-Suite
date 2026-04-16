using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using UglyToad.PdfPig;

namespace AcademicAI.App.Controls;

public partial class PdfDropZone : UserControl
{
    public static readonly DependencyProperty ExtractedTextProperty =
        DependencyProperty.Register(
            nameof(ExtractedText),
            typeof(string),
            typeof(PdfDropZone),
            new PropertyMetadata(string.Empty));

    public string ExtractedText
    {
        get => (string)GetValue(ExtractedTextProperty);
        set => SetValue(ExtractedTextProperty, value);
    }

    public static readonly RoutedEvent PdfTextExtractedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(PdfTextExtracted),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PdfDropZone));

    public event RoutedEventHandler PdfTextExtracted
    {
        add => AddHandler(PdfTextExtractedEvent, value);
        remove => RemoveHandler(PdfTextExtractedEvent, value);
    }

    private readonly Brush _defaultBorderBrush;

    public PdfDropZone()
    {
        InitializeComponent();
        _defaultBorderBrush = DropBorder.BorderBrush;
        MouseLeftButtonDown += PdfDropZone_MouseLeftButtonDown;
    }

    private void PdfDropZone_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files is { Length: > 0 } && files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
                var accentBrush = Application.Current.FindResource("TextFillColorSecondaryBrush") as Brush ?? Brushes.DodgerBlue;
                DropBorder.BorderBrush = accentBrush;
                DropBorder.BorderThickness = new Thickness(3);
                e.Handled = true;
                return;
            }
        }

        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void PdfDropZone_Drop(object sender, DragEventArgs e)
    {
        ResetBorder();

        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files is { Length: > 0 })
        {
            var filePath = files[0];
            if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ExtractPdfText(filePath);
            }
        }
    }

    private void PdfDropZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            DefaultExt = ".pdf"
        };

        if (dialog.ShowDialog() == true)
        {
            ExtractPdfText(dialog.FileName);
        }
    }

    private async void ExtractPdfText(string filePath)
    {
        FileNameText.Text = Path.GetFileName(filePath);
        try
        {
            var text = await Task.Run(() =>
            {
                var sb = new System.Text.StringBuilder();
                using var document = PdfDocument.Open(filePath);
                foreach (var page in document.GetPages())
                    sb.AppendLine(page.Text);
                return sb.ToString();
            });
            ExtractedText = text;
            RaiseEvent(new RoutedEventArgs(PdfTextExtractedEvent, this));
        }
        catch (Exception ex)
        {
            FileNameText.Text = $"Error: {ex.Message}";
        }
    }

    private void ResetBorder()
    {
        DropBorder.BorderBrush = _defaultBorderBrush;
        DropBorder.BorderThickness = new Thickness(2);
    }
}
