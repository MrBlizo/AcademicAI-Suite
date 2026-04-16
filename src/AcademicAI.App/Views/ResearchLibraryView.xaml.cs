using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AcademicAI.App.ViewModels;
using AcademicAI.Core.Models;

namespace AcademicAI.App.Views;

public partial class ResearchLibraryView : Page
{
    public ResearchLibraryView()
    {
        InitializeComponent();
        DataContext = new ResearchLibraryViewModel();
    }

    private void OpenUrl(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is SearchResult result && !string.IsNullOrEmpty(result.Url))
            OpenInBrowser(result.Url);
    }

    // ── Quick Access Source Buttons ──────────────────────────────

    private void OpenSciHub(object sender, RoutedEventArgs e)
        => OpenInBrowser("https://sci-hub.usualwant.com");

    private void OpenZLib(object sender, RoutedEventArgs e)
        => OpenInBrowser("https://z-lib.sk");

    private void OpenAnnasArchive(object sender, RoutedEventArgs e)
        => OpenInBrowser("https://annas-archive.gl");

    private void OpenLibGen(object sender, RoutedEventArgs e)
        => OpenInBrowser("https://libgen.li");

    // ── Per-Result Source Buttons (search by title) ─────────────

    private void SearchOnSciHub(object sender, RoutedEventArgs e)
    {
        var title = GetTitleFromTag(sender);
        if (!string.IsNullOrEmpty(title))
            OpenInBrowser($"https://sci-hub.usualwant.com/{Uri.EscapeDataString(title)}");
    }

    private void SearchOnZLib(object sender, RoutedEventArgs e)
    {
        var title = GetTitleFromTag(sender);
        if (!string.IsNullOrEmpty(title))
            OpenInBrowser($"https://z-lib.sk/s/{Uri.EscapeDataString(title)}");
    }

    private void SearchOnLibGen(object sender, RoutedEventArgs e)
    {
        var title = GetTitleFromTag(sender);
        if (!string.IsNullOrEmpty(title))
            OpenInBrowser($"https://libgen.li/index.php?req={Uri.EscapeDataString(title)}&lg_topic=libgen&open=0&view=simple&res=25&phrase=1&column=def");
    }

    // ── Helpers ─────────────────────────────────────────────────

    private static string? GetTitleFromTag(object sender)
    {
        if (sender is FrameworkElement fe && fe.Tag is string tag)
            return tag;
        return null;
    }

    private static void OpenInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch { }
    }
}
