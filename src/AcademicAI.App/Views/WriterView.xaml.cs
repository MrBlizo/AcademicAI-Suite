using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace AcademicAI.App.Views;

public partial class WriterView : Page
{
    private readonly Wpf.Ui.Controls.Button[] _tabButtons;
    private readonly Page[] _tabPages;

    public WriterView()
    {
        InitializeComponent();
        _tabButtons = [Tab0Btn, Tab1Btn, Tab2Btn];
        _tabPages = [new AcademicWriterView(), new EssayOutlineView(), new GrammarCheckView()];
        TabContent.Navigate(_tabPages[0]);
    }

    private void SelectTab0(object sender, RoutedEventArgs e) => SwitchTab(0);
    private void SelectTab1(object sender, RoutedEventArgs e) => SwitchTab(1);
    private void SelectTab2(object sender, RoutedEventArgs e) => SwitchTab(2);

    private void SwitchTab(int index)
    {
        for (int i = 0; i < _tabButtons.Length; i++)
            _tabButtons[i].Appearance = i == index ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;
        TabContent.Navigate(_tabPages[index]);
    }
}
