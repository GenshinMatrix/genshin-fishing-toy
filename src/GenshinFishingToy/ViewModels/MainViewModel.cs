using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenshinFishingToy.Core;
using GenshinFishingToy.Models;
using GenshinFishingToy.Views;
using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GenshinFishingToy.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public MainWindow Source { get; set; } = null!;
    internal ImageJigging jigging = new();

    public double Left => SystemParameters.WorkArea.Right;
    public double Top => SystemParameters.WorkArea.Bottom;

    public bool OptionLock
    {
        get => Settings.Lock;
        set
        {
            Settings.Lock.Set(value);
            SettingsManager.Save();
            BorderWindowBehavior.SetLocked(Source, value);
            jigging.window.MotionArea.SetLayeredWindow(value);
        }
    }

    public bool OptionAutoLifting
    {
        get => Settings.AutoLifting;
        set
        {
            Settings.AutoLifting.Set(value);
            SettingsManager.Save();
        }
    }

    public bool OptionShowRecognitionJigging
    {
        get => Settings.ShowRecognitionJigging;
        set
        {
            Settings.ShowRecognitionJigging.Set(value);
            SettingsManager.Save();
        }
    }

    public bool OptionShowRecognitionCapture
    {
        get => Settings.ShowRecognitionCapture;
        set
        {
            Settings.ShowRecognitionCapture.Set(value);
            SettingsManager.Save();
        }
    }

    public bool OptionShowRecognitionLifting
    {
        get => Settings.ShowRecognitionLifting;
        set
        {
            Settings.ShowRecognitionLifting.Set(value);
            SettingsManager.Save();
        }
    }

    public ICommand ResetJiggingRegionCommand { get; }

    public ICommand StartCommand { get; }

    public ICommand ConfigOpenCommand => new RelayCommand(async () =>
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{SettingsManager.Path}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        catch
        {
        }
    });

    public ICommand ConfigOpenWithNotepadCommand => new RelayCommand(async () =>
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo()
            {
                FileName = "notepad.exe",
                Arguments = $"\"{SettingsManager.Path}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        catch
        {
        }
    });

    public ICommand ConfigOpenWithCommand => new RelayCommand(async () =>
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo()
            {
                FileName = "openwith.exe",
                Arguments = $"\"{SettingsManager.Path}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        catch
        {
        }
    });

    public ICommand ConfigReloadCommand => new RelayCommand(async () =>
    {
        SettingsManager.Reinit();
        SetupLanguage();
    });

    public ICommand TopMostCommand => new RelayCommand<Window>(async app =>
    {
        app!.Topmost = !app.Topmost;
        if (app.FindName("TextBlockTopMost") is TextBlock topMostIcon)
        {
            topMostIcon.Text = app.Topmost ? FluentSymbol.Unpin : FluentSymbol.Pin;
        }
    });
    public ICommand RestorePosCommand => new RelayCommand<Window>(async app =>
    {
        app!.Left = Left - app.Width;
        app!.Top = Top - app.Height;
    });
    public ICommand RestartCommand => NotifyIconViewModel.RestartCommand;
    public ICommand ExitCommand => NotifyIconViewModel.ExitCommand;
    public ICommand UsageCommand => NotifyIconViewModel.UsageCommand;
    public ICommand GitHubCommand => NotifyIconViewModel.GitHubCommand;

    public MainViewModel(MainWindow source)
    {
        Source = source;

        ResetJiggingRegionCommand = new RelayCommand(() =>
        {
            jigging.window.MotionArea.ResetClient(jigging.window.Hwnd);
        });

        StartCommand = new RelayCommand<Button>(async button =>
        {
            TextBlock startIcon = (Window.GetWindow(button).FindName("TextBlockStartIcon") as TextBlock)!;
            TextBlock start = (Window.GetWindow(button).FindName("TextBlockStart") as TextBlock)!;
            SvgViewbox mainIcon = (Window.GetWindow(button).FindName("SvgViewBoxMainIcon") as SvgViewbox)!;

            Brush brush = null!;
            button!.IsEnabled = false;
            if (startIcon.Text.Equals(FluentSymbol.Start))
            {
                brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEDBE8F6"));
                start.Text = Mui("ButtonStop");
                startIcon.Text = FluentSymbol.Stop;
                mainIcon.SetColor("Blue");
                await StartFishing();
            }
            else
            {
                brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEFFFFFF"));
                start.Text = Mui("ButtonStart");
                startIcon.Text = FluentSymbol.Start;
                mainIcon.SetColor("Black");
                await StopFishing();
            }

            Border border = (Window.GetWindow(button).FindName("Border") as Border)!;
            StoryboardUtils.BeginBrushStoryboard(border, new Dictionary<DependencyProperty, Brush>()
            {
                [Border.BackgroundProperty] = brush,
            });
            await Task.Delay(20);
            button!.IsEnabled = true;
        });
        RegisterHotKey();
    }

    private void RegisterHotKey()
    {
        try
        {
            HotkeyHolder.RegisterHotKey(Settings.ShortcutKey, (s, e) =>
            {
                StartCommand?.Execute(Source.FindName("ButtonStart"));
            });
        }
        catch (Exception e)
        {
            Logger.Exception(e);
        }
    }

    public async Task StartFishing()
    {
        await jigging.StartFishing();
    }

    public async Task StopFishing()
    {
        await jigging.StopFishing();
    }

    [ObservableProperty]
    private bool isImageCaptureTypeIsBitBlt = Settings.CaptureType.Get().Equals(nameof(ImageCaptureType.BitBlt));

    [ObservableProperty]
    private bool isImageCaptureTypeIsWindowsGraphicsCapture = Settings.CaptureType.Get().Equals(nameof(ImageCaptureType.WindowsGraphicsCapture));

    [RelayCommand]
    public void SetImageCaptureType(string captureType)
    {
        if (jigging.IsRunning)
        {
            IsImageCaptureTypeIsBitBlt = Settings.CaptureType.Get().Equals(nameof(ImageCaptureType.BitBlt));
            IsImageCaptureTypeIsWindowsGraphicsCapture = Settings.CaptureType.Get().Equals(nameof(ImageCaptureType.WindowsGraphicsCapture));
            return;
        }
        ImageCapture.CaptureType = (ImageCaptureType)Enum.Parse(typeof(ImageCaptureType), captureType);
        Settings.CaptureType.Set(ImageCapture.CaptureType.ToString());
        SettingsManager.Save();
    }
}
