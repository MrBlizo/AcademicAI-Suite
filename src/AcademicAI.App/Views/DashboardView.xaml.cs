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

    private void NavigateToStudyHub(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(StudyHubView));
    private void NavigateToWriter(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(WriterView));
    private void NavigateToTextTools(object sender, MouseButtonEventArgs e) => NavigateTo(typeof(TextToolsView));

    private void NavigateTo(Type pageType)
    {
        if (Application.Current.MainWindow is MainWindow main && main.NavView != null)
            main.NavView.Navigate(pageType);
    }
}
