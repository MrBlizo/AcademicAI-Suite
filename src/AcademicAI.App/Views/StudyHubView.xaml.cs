using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace AcademicAI.App.Views;

public partial class StudyHubView : Page
{
    private readonly Wpf.Ui.Controls.Button[] _tabButtons;
    private readonly Page[] _tabPages;

    public StudyHubView()
    {
        InitializeComponent();
        _tabButtons = [Tab0Btn, Tab1Btn, Tab2Btn, Tab3Btn];
        _tabPages = [new FlashcardsView(), new QuizView(), new NoteOrganizerView(), new MathSolverView()];
        TabContent.Navigate(_tabPages[0]);
    }

    private void SelectTab0(object sender, RoutedEventArgs e) => SwitchTab(0);
    private void SelectTab1(object sender, RoutedEventArgs e) => SwitchTab(1);
    private void SelectTab2(object sender, RoutedEventArgs e) => SwitchTab(2);
    private void SelectTab3(object sender, RoutedEventArgs e) => SwitchTab(3);

    private void SwitchTab(int index)
    {
        for (int i = 0; i < _tabButtons.Length; i++)
            _tabButtons[i].Appearance = i == index ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;
        TabContent.Navigate(_tabPages[index]);
    }
}
