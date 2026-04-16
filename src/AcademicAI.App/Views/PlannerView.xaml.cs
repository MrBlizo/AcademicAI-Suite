using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AcademicAI.App.ViewModels;
using AcademicAI.Core.Models;

namespace AcademicAI.App.Views;

public partial class PlannerView : Page
{
    public PlannerView()
    {
        InitializeComponent();
        DataContext = new PlannerViewModel();
    }
}

public class UrgencyTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Assignment a)
        {
            if (a.IsCompleted) return "Completed";
            var daysLeft = (a.DueDate - DateTime.Today).Days;
            if (daysLeft < 0) return "Overdue";
            if (daysLeft <= 1) return "Due Today";
            if (daysLeft <= 3) return "Due Soon";
            return "On Track";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class UrgencyBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Assignment a)
        {
            if (a.IsCompleted) return Application.Current.FindResource("TextFillColorTertiaryBrush") ?? Brushes.Gray;
            var daysLeft = (a.DueDate - DateTime.Today).Days;
            if (daysLeft < 0) return Application.Current.FindResource("SystemFillColorCriticalBrush") ?? Brushes.Red;
            if (daysLeft <= 3) return Application.Current.FindResource("TextFillColorSecondaryBrush") ?? Brushes.Orange;
            return Application.Current.FindResource("SystemFillColorSuccessBrush") ?? Brushes.Green;
        }
        return Application.Current.FindResource("TextFillColorTertiaryBrush") ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
