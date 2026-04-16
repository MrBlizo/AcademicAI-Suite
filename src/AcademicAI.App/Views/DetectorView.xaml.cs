using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AcademicAI.App.Controls;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class DetectorView : Page
{
    public DetectorView()
    {
        InitializeComponent();
        var vm = new DetectorViewModel();
        DataContext = vm;
        PdfDrop.PdfTextExtracted += OnPdfTextExtracted;
    }

    private void OnPdfTextExtracted(object sender, RoutedEventArgs e)
    {
        if (DataContext is DetectorViewModel vm)
        {
            vm.IsPdfMode = true;
            vm.PdfText = PdfDrop.ExtractedText;
        }
    }
}

public class ProbabilityBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double probability)
        {
            if (probability > 70)
                return Application.Current.FindResource("SystemFillColorCriticalBrush") as Brush ?? Brushes.Red;
            if (probability > 40)
                return Application.Current.FindResource("TextFillColorSecondaryBrush") as Brush ?? Brushes.Orange;
            return Application.Current.FindResource("SystemFillColorSuccessBrush") as Brush ?? Brushes.Green;
        }
        return Application.Current.FindResource("TextFillColorPrimaryBrush") as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class StringVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
