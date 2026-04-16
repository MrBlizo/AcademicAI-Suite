using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AcademicAI.Agents;
using AcademicAI.App.Services;
using AcademicAI.App.Views;
using AcademicAI.Core.Helpers;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;
using AcademicAI.Core.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private TaskbarIcon? _trayIcon;
    private Views.SplashScreen? _splash;

    public static IServiceProvider Services => ((App)Current)._serviceProvider!;

    public static void ApplyTheme(string theme)
    {
        var appTheme = theme.Equals("Light", StringComparison.OrdinalIgnoreCase)
            ? Wpf.Ui.Appearance.ApplicationTheme.Light
            : Wpf.Ui.Appearance.ApplicationTheme.Dark;
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(appTheme);
        // Apply 2026 indigo accent colour
        Wpf.Ui.Appearance.ApplicationAccentColorManager.Apply(
            System.Windows.Media.Color.FromRgb(0x63, 0x66, 0xF1), appTheme, false);
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        LogStartup("Application starting...");

        _splash = new Views.SplashScreen();
        _splash.Show();
        _splash.UpdateStatus("Initializing...", 5);

        try
        {
            LogStartup("Building DI container...");
            var services = new ServiceCollection();
            ConfigureServices(services);
            LogStartup("Building service provider...");
            _serviceProvider = services.BuildServiceProvider();
            LogStartup("DI container ready.");

            _splash.UpdateStatus("Generating license...", 15);
            LogStartup("Generating license...");
            var licenseService = _serviceProvider.GetRequiredService<ILicenseService>();
            licenseService.GetLicenseInfo();
            LogStartup("License generated.");

            _splash.UpdateStatus("Checking remote control...", 25);
            LogStartup("Checking remote control...");
            RemoteCheckResult remoteResult = new();
            try
            {
                var remoteService = _serviceProvider.GetRequiredService<IRemoteControlService>();
                remoteResult = await Task.Run(() => remoteService.CheckAsync());
                LogStartup("Remote check completed.");
            }
            catch (Exception ex)
            {
                LogStartup($"Remote check skipped: {ex.Message}");
                remoteResult = new RemoteCheckResult();
            }

            if (!remoteResult.IsAlive || remoteResult.IsRevoked)
            {
                MessageBox.Show(remoteResult.KillMessage, "AcademicAI Suite", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            if (remoteResult.HasUpdate && remoteResult.MandatoryUpdate)
            {
                MessageBox.Show($"A mandatory update ({remoteResult.LatestVersion}) is available. The application will now close. Please download the latest version.", "Mandatory Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
                return;
            }

            if (remoteResult.HasUpdate)
            {
                var result = MessageBox.Show($"Update {remoteResult.LatestVersion} is available. Download now?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(remoteResult.DownloadUrl))
                {
                    try
                    {
                        var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
                        var tempPath = Path.Combine(Path.GetTempPath(), $"AcademicAI_Update_{remoteResult.LatestVersion}.exe");
                        var progressWindow = new UpdateProgressWindow(remoteResult.LatestVersion);
                        progressWindow.Show();
                        var progress = new Progress<double>(p => progressWindow.UpdateProgress(p));
                        await updateService.DownloadUpdateAsync(remoteResult.DownloadUrl, tempPath, progress);
                        progressWindow.Close();
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPath) { UseShellExecute = true });
                        Shutdown();
                        return;
                    }
                    catch (Exception ex)
                    {
                        var toastService = _serviceProvider.GetRequiredService<INotificationService>();
                        toastService.Show("Update Error", $"Failed to download update: {ex.Message}");
                    }
                }
            }

            _splash.UpdateStatus("Loading settings...", 40);
            var settingsService = _serviceProvider.GetRequiredService<IAppSettingsService>();
            settingsService.Load();

            _splash.UpdateStatus("Loading localization...", 50);
            var locManager = (LocalizationManager)LocalizationManager.Instance!;
            locManager.Reload();
            locManager.SetLanguage(settingsService.Settings.Language);

            _splash.UpdateStatus("Applying theme...", 55);
            ApplyTheme(settingsService.Settings.Theme);

            _splash.UpdateStatus("Creating main window...", 65);
            var mainWindow = new MainWindow(_serviceProvider);

            if (settingsService.Settings.WindowLeft >= 0) mainWindow.Left = settingsService.Settings.WindowLeft;
            if (settingsService.Settings.WindowTop >= 0) mainWindow.Top = settingsService.Settings.WindowTop;
            mainWindow.Width = settingsService.Settings.WindowWidth > 0 ? settingsService.Settings.WindowWidth : 1200;
            mainWindow.Height = settingsService.Settings.WindowHeight > 0 ? settingsService.Settings.WindowHeight : 800;
            if (settingsService.Settings.IsMaximized) mainWindow.WindowState = WindowState.Maximized;

            _splash.UpdateStatus("Initializing tray icon...", 75);
            InitializeTrayIcon(mainWindow);

            _splash.UpdateStatus("Starting notification service...", 80);
            var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            if (notificationService is ToastNotificationService toastSvc)
                toastSvc.Initialize(mainWindow.ToastContainer);

            _splash.UpdateStatus("Ready", 100);
            _splash.Close();
            _splash = null;

            if (settingsService.Settings.FirstRun)
            {
                var wizard = new OnboardingWizard(settingsService, _serviceProvider.GetRequiredService<ISecretStore>(), _serviceProvider.GetRequiredService<IAgentFactory>());
                wizard.ShowDialog();
                settingsService.Settings.FirstRun = false;
                settingsService.Save();
            }

            mainWindow.Show();
            Current.MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            LogStartup("Application started successfully.");
        }
        catch (Exception ex)
        {
            LogStartup($"FATAL: {ex}");
            MessageBox.Show($"Failed to start: {ex.Message}", "AcademicAI Suite", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISecretStore, AesSecretStore>();
        services.AddSingleton<ILicenseService, LicenseService>();
        services.AddSingleton<IRemoteControlService, RemoteControlService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IAgentFactory, AgentFactory>();
        services.AddSingleton<ITokenTrackerService, TokenTrackerService>();
        services.AddSingleton<INotificationService, ToastNotificationService>();
        services.AddSingleton<AcademicAI.Academic.AcademicTextProcessor>();
        services.AddSingleton<AcademicAI.Academic.FlashcardGenerator>();
        services.AddSingleton<AcademicAI.Academic.QuizGenerator>();
        services.AddSingleton<AcademicAI.Academic.EssayOutliner>();
        services.AddSingleton<AcademicAI.Academic.Paraphraser>();
        services.AddSingleton<AcademicAI.Academic.GrammarChecker>();
        services.AddSingleton<AcademicAI.Academic.MathSolver>();
        services.AddSingleton<AcademicAI.Academic.NoteOrganizer>();
        services.AddSingleton<AcademicAI.Humanizer.TextHumanizer>();
        services.AddSingleton<AcademicAI.Detection.AiDetector>();
    }

    private void InitializeTrayIcon(MainWindow mainWindow)
    {
        _trayIcon = new TaskbarIcon();
        _trayIcon.ToolTipText = "AcademicAI Suite";
        _trayIcon.IconSource = new System.Windows.Media.Imaging.BitmapImage(
            new Uri("pack://application:,,,/assets/app.ico"));
        _trayIcon.TrayLeftMouseDown += (_, _) => ShowMainWindow(mainWindow);

        var contextMenu = new ContextMenu();
        var showItem = new MenuItem { Header = "Show" };
        showItem.Click += (_, _) => ShowMainWindow(mainWindow);
        var timerItem = new MenuItem { Header = "Floating Timer" };
        timerItem.Click += (_, _) =>
        {
            var floating = new FloatingPomodoroWindow();
            floating.Show();
        };
        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => Shutdown();
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(timerItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);
        _trayIcon.ContextMenu = contextMenu;
    }

    private void ShowMainWindow(MainWindow mainWindow)
    {
        if (mainWindow.WindowState == WindowState.Minimized)
            mainWindow.WindowState = WindowState.Normal;
        mainWindow.Show();
        mainWindow.Activate();
    }

    public static void LogStartup(string message)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcademicAI");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "startup.log"), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
        }
        catch { }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            var settingsService = _serviceProvider?.GetService<IAppSettingsService>();
            if (settingsService != null && Current.MainWindow is Window win)
            {
                var s = settingsService.Settings;
                s.WindowLeft = win.WindowState == WindowState.Minimized ? -1 : (win.Left >= 0 ? win.Left : -1);
                s.WindowTop = win.WindowState == WindowState.Minimized ? -1 : (win.Top >= 0 ? win.Top : -1);
                s.WindowWidth = win.RestoreBounds.Width > 0 ? win.RestoreBounds.Width : win.Width;
                s.WindowHeight = win.RestoreBounds.Height > 0 ? win.RestoreBounds.Height : win.Height;
                s.IsMaximized = win.WindowState == WindowState.Maximized;
                settingsService.Save();
            }
        }
        catch { }

        _trayIcon?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public App()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            LogStartup($"DispatcherUnhandledException: {e.Exception}");
            try
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged.",
                    "AcademicAI Suite — Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch { }
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            LogStartup($"UnhandledException: {e.ExceptionObject}");
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogStartup($"UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };
    }
}
