using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AcademicAI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace AcademicAI.App.Views;

public partial class MainWindow : FluentWindow
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
        SetupNavigation();
        ApplyFlowDirection();

        NavView.Loaded += (_, _) =>
        {
            NavView.Navigate(typeof(DashboardView));
        };

        SourceInitialized += OnSourceInitialized;
    }

    private void SetupNavigation()
    {
        NavView.TitleBar = TitleBar;

        var loc = Core.Services.LocalizationManager.Instance;

        var menuItems = new List<NavigationViewItem>
        {
            new() { Content = loc?["Nav_Dashboard"] ?? "Dashboard", Tag = "Nav_Dashboard", Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 }, TargetPageType = typeof(DashboardView) },
            new() { Content = loc?["Nav_StudyHub"] ?? "Study Hub", Tag = "Nav_StudyHub", Icon = new SymbolIcon { Symbol = SymbolRegular.HatGraduation24 }, TargetPageType = typeof(StudyHubView) },
            new() { Content = loc?["Nav_Planner"] ?? "Planner", Tag = "Nav_Planner", Icon = new SymbolIcon { Symbol = SymbolRegular.CalendarMonth24 }, TargetPageType = typeof(PlannerView) },
            new() { Content = loc?["Nav_Research"] ?? "Research", Tag = "Nav_Research", Icon = new SymbolIcon { Symbol = SymbolRegular.Search24 }, TargetPageType = typeof(ResearchView) },
            new() { Content = loc?["Nav_Writer"] ?? "Writer", Tag = "Nav_Writer", Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentEdit24 }, TargetPageType = typeof(WriterView) },
            new() { Content = loc?["Nav_TextTools"] ?? "Text Tools", Tag = "Nav_TextTools", Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentText24 }, TargetPageType = typeof(TextToolsView) },
        };

        var footerItems = new List<NavigationViewItem>
        {
            new() { Content = loc?["Nav_Settings"] ?? "Settings", Tag = "Nav_Settings", Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 }, TargetPageType = typeof(SettingsView) },
        };

        NavView.MenuItemsSource = menuItems;
        NavView.FooterMenuItemsSource = footerItems;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        try
        {
            var exeDir = AppContext.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar);

            // Find the PNG logo for WPF Window.Icon + TitleBar.Icon
            var logoCandidates = new[]
            {
                System.IO.Path.Combine(exeDir, "assets", "logo.png"),
                System.IO.Path.Combine(exeDir, "logo.png"),
            };
            var logoPath = logoCandidates.FirstOrDefault(System.IO.File.Exists);
            if (logoPath != null)
            {
                var bmp = new BitmapImage(new Uri(logoPath, UriKind.Absolute));
                Icon = bmp;                      // Window icon (taskbar + alt-tab)
                TitleBar.Icon = new Wpf.Ui.Controls.ImageIcon
                {
                    Source = bmp,
                    Width = 24,
                    Height = 24,
                };
            }

            // Also set the native icon via Win32 for taskbar/alt-tab
            var icoCandidates = new[]
            {
                System.IO.Path.Combine(exeDir, "assets", "app.ico"),
                System.IO.Path.Combine(exeDir, "app.ico"),
            };
            var icoPath = icoCandidates.FirstOrDefault(System.IO.File.Exists);
            if (icoPath != null)
            {
                var icon = new System.Drawing.Icon(icoPath);
                SendMessage(hwnd, WM_SETICON, IntPtr.Zero, icon.Handle);  // Small (titlebar)
                SendMessage(hwnd, WM_SETICON, (IntPtr)1, icon.Handle);    // Big (taskbar/alt-tab)
            }
        }
        catch { }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    private const int WM_SETICON = 0x0080;

    public void ApplyFlowDirection()
    {
        var loc = Core.Services.LocalizationManager.Instance;
        if (loc != null && loc.IsRightToLeft)
            FlowDirection = FlowDirection.RightToLeft;
        else
            FlowDirection = FlowDirection.LeftToRight;
    }

    public void ReloadLocalization()
    {
        var loc = Core.Services.LocalizationManager.Instance;
        if (loc == null) return;

        ApplyFlowDirection();

        if (NavView.MenuItemsSource is System.Collections.IList menuList)
            foreach (var item in menuList)
            {
                if (item is NavigationViewItem navItem && navItem.Tag is string key)
                    navItem.Content = loc[key];
            }

        if (NavView.FooterMenuItemsSource is System.Collections.IList footerList)
            foreach (var item in footerList)
            {
                if (item is NavigationViewItem navItem && navItem.Tag is string key)
                    navItem.Content = loc[key];
            }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        var settingsService = _serviceProvider.GetRequiredService<IAppSettingsService>();
        if (settingsService.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            base.OnClosing(e);
        }
    }
}
