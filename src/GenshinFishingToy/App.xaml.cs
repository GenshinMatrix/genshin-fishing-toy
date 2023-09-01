using Composition.WindowsRuntimeHelpers;
using GenshinFishingToy.Core;
using GenshinFishingToy.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.System;

namespace GenshinFishingToy;

public partial class App : Application
{
    private DispatcherQueueController _controller;
    public static new App? Current { get; protected set; } = null!;
    public TaskbarIcon Taskbar { get; protected set; } = null!;
    public static bool IsElevated { get; } = GetElevated();

    public App()
    {
        Logger.Info("Startup");
        Current = this;
        Current.DispatcherUnhandledException += (_, e) => e.Handled = true;
        _controller = CoreMessagingHelper.CreateDispatcherQueueControllerForCurrentThread();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => Logger.Error(e);
        ToastNotificationManagerCompat.OnActivated += NotifyIconViewModel.OnNotificationActivated;
        NoticeService.ClearNotice();
        if (!EnsureElevated()) return;
        CheckSingleInstance();
        InitializeComponent();
        SetupLanguage();
        SettingsManager.Setup();
        CheckOSVersion();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Taskbar = (TaskbarIcon)FindResource("PART_Taskbar");
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        NoticeService.ClearNotice();
        base.OnExit(e);
    }

    protected void CheckOSVersion()
    {
        if (!Pack.IsSupported)
        {
            NoticeService.AddNotice(Mui("Tips"), Mui("SupportedOSVersionTips", Pack.SupportedOSVersion, Pack.CurrentOSVersion), string.Empty, ToastDuration.Short);
        }
    }

    public bool EnsureElevated()
    {
        if (!IsElevated)
        {
            RestartAsElevated('r' + 'u' + 'n' + 'a' + 's');
            return false;
        }
        return true;
    }

    public static void RestartAsElevated(int? exitCode = null)
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo()
            {
                Verb = "runas",
                UseShellExecute = true,
                FileName = Process.GetCurrentProcess().MainModule!.FileName,
                WorkingDirectory = Environment.CurrentDirectory,
            });
        }
        catch (Win32Exception)
        {
            return;
        }
        Current!.Shutdown();
        if (exitCode != null) Environment.Exit(exitCode.Value);
    }

    private static bool GetElevated()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void CheckSingleInstance()
    {
        EventWaitHandle? handle;

        try
        {
            handle = EventWaitHandle.OpenExisting(Pack.Alias);
            handle.Set();
            Shutdown();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            handle = new EventWaitHandle(false, EventResetMode.AutoReset, Pack.Alias);
        }
        GC.KeepAlive(handle);
        _ = Task.Run(() =>
        {
            while (handle.WaitOne())
            {
                Dispatcher.Invoke(() =>
                {
                    MainWindow?.Activate();
                    MainWindow?.Focus();
                    MainWindow?.Show();
                });
            }
        });
    }
}
