using System.Windows;
using System.Windows.Controls;

namespace AcademicAI.App.Views;

public partial class TextToolsView : Page
{
    private readonly Wpf.Ui.Controls.Button[] _tabButtons;
    private readonly UIElement[] _tabContents;

    public TextToolsView()
    {
        InitializeComponent();
        _tabButtons = [Tab0Btn, Tab1Btn, Tab2Btn, Tab3Btn, Tab4Btn];
        _tabContents = [Tab0Content, Tab1Content, Tab2Content, Tab3Content, Tab4Content];
    }

    private void SelectTab0(object sender, RoutedEventArgs e) => SwitchTab(0);
    private void SelectTab1(object sender, RoutedEventArgs e) => SwitchTab(1);
    private void SelectTab2(object sender, RoutedEventArgs e) => SwitchTab(2);
    private void SelectTab3(object sender, RoutedEventArgs e) => SwitchTab(3);
    private void SelectTab4(object sender, RoutedEventArgs e) => SwitchTab(4);

    private void SwitchTab(int index)
    {
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            _tabButtons[i].Appearance = i == index ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;
            _tabContents[i].Visibility = i == index ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
