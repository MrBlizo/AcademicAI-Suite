using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class DashboardView : Page
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
    }

    private void NavigateToWriter(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(AcademicWriterView));
    private void NavigateToTranslator(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(TranslatorView));
    private void NavigateToSummarizer(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(SummarizerView));
    private void NavigateToDetector(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(DetectorView));

    private void NavigateTo(Type pageType)
    {
        if (Application.Current.MainWindow is MainWindow main && main.NavView != null)
            main.NavView.Navigate(pageType);
    }
}
