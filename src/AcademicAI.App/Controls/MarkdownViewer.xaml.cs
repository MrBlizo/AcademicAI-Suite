using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Markdig;

namespace AcademicAI.App.Controls;

public partial class MarkdownViewer : UserControl
{
    public static readonly DependencyProperty MarkdownTextProperty =
        DependencyProperty.Register(
            nameof(MarkdownText),
            typeof(string),
            typeof(MarkdownViewer),
            new PropertyMetadata(string.Empty, OnMarkdownTextChanged));

    public string MarkdownText
    {
        get => (string)GetValue(MarkdownTextProperty);
        set => SetValue(MarkdownTextProperty, value);
    }

    public MarkdownViewer()
    {
        InitializeComponent();
        Browser.LoadCompleted += Browser_LoadCompleted;

        // Navigate to a dark blank page immediately to avoid white flash
        try
        {
            var isDark = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark;
            var bg = isDark ? "#2a2a2e" : "#ffffff";
            Browser.NavigateToString($"<html><body style='background:{bg};margin:0;'></body></html>");
        }
        catch { }
    }

    private static void OnMarkdownTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewer = (MarkdownViewer)d;
        viewer.RenderMarkdown();
    }

    /// <summary>
    /// After the WebBrowser finishes loading, read the document body's scrollHeight
    /// and set the control's Height to match — making it expand to fit the content.
    /// Also fade away the loading overlay.
    /// </summary>
    private void Browser_LoadCompleted(object? sender, System.Windows.Navigation.NavigationEventArgs e)
    {
        try
        {
            // Hide the dark overlay now that content is rendered
            LoadingOverlay.Visibility = Visibility.Collapsed;

            dynamic doc = Browser.Document;
            if (doc?.body != null)
            {
                int scrollH = (int)doc.body.scrollHeight;
                // Clamp between 80 and 4000 to avoid extreme sizes
                int h = Math.Max(80, Math.Min(scrollH + 24, 4000));
                Browser.Height = h;
            }
        }
        catch
        {
            // Fallback if MSHTML COM is not available
            Browser.Height = 300;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private static string SanitizeHtml(string html)
    {
        html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"on\w+\s*=", "", RegexOptions.IgnoreCase);
        return html;
    }

    private void RenderMarkdown()
    {
        // Show overlay while loading new content
        LoadingOverlay.Visibility = Visibility.Visible;

        var markdown = MarkdownText ?? string.Empty;

        if (string.IsNullOrWhiteSpace(markdown))
        {
            Browser.Height = 0;
            LoadingOverlay.Visibility = Visibility.Collapsed;
            return;
        }

        var html = Markdown.ToHtml(markdown);
        html = SanitizeHtml(html);

        var isDark = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark;
        var bgColor = isDark ? "#2a2a2e" : "#ffffff";
        var textColor = isDark ? "#e0e0e0" : "#1a1a1a";
        var codeBg = isDark ? "rgba(128,128,128,0.2)" : "rgba(128,128,128,0.1)";
        var borderColor = isDark ? "#555" : "#ddd";
        var linkColor = isDark ? "#818CF8" : "#4F46E5";
        var quoteColor = isDark ? "#aaa" : "#666";
        var quoteBorder = isDark ? "#6366F1" : "#6366F1";

        var template = $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<style>
* {{ margin: 0; padding: 0; box-sizing: border-box; }}
body {{
    font-family: 'Segoe UI Variable Display', 'Segoe UI', -apple-system, sans-serif;
    margin: 0;
    padding: 20px;
    line-height: 1.7;
    color: {textColor};
    background: {bgColor};
    font-size: 14px;
    overflow: hidden;
}}
h1 {{ font-size: 1.6em; margin: 0.8em 0 0.4em; font-weight: 700; }}
h2 {{ font-size: 1.35em; margin: 0.7em 0 0.3em; font-weight: 600; }}
h3 {{ font-size: 1.15em; margin: 0.6em 0 0.3em; font-weight: 600; }}
h4, h5, h6 {{ margin: 0.5em 0 0.2em; font-weight: 600; }}
p {{ margin: 0.5em 0; }}
ul, ol {{ margin: 0.5em 0 0.5em 1.5em; }}
li {{ margin: 0.2em 0; }}
code {{
    background: {codeBg};
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 0.9em;
    font-family: 'Cascadia Code', 'Consolas', monospace;
}}
pre {{
    background: {codeBg};
    padding: 14px;
    border-radius: 8px;
    overflow-x: auto;
    margin: 0.8em 0;
}}
pre code {{ background: none; padding: 0; }}
blockquote {{
    border-left: 3px solid {quoteBorder};
    margin: 0.6em 0;
    padding: 4px 0 4px 14px;
    color: {quoteColor};
}}
table {{ border-collapse: collapse; margin: 0.8em 0; width: 100%; }}
th, td {{ border: 1px solid {borderColor}; padding: 8px 12px; text-align: left; }}
th {{ font-weight: 600; }}
img {{ max-width: 100%; border-radius: 6px; }}
a {{ color: {linkColor}; text-decoration: none; }}
a:hover {{ text-decoration: underline; }}
hr {{ border: none; border-top: 1px solid {borderColor}; margin: 1em 0; }}
</style>
</head>
<body>
{html}
</body>
</html>";

        Browser.NavigateToString(template);
    }
}
