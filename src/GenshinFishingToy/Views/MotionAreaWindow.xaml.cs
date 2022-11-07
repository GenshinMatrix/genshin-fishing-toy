using GenshinFishingToy.Core;
using GenshinFishingToy.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Rect = OpenCvSharp.Rect;

namespace GenshinFishingToy.Views;

public partial class MotionAreaWindow : ObservableWindow
{
    public IntPtr Hwnd => new WindowInteropHelper(this).Handle;

    public Rect[] rects = Array.Empty<Rect>();
    public Rect[] Rects
    {
        get => rects;
        set
        {
            rects = value;
            InvalidateVisual();
        }
    }

    public RECT rectGameLatest = new();

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MotionAreaWindow), new PropertyMetadata(string.Empty));

    public Brush BackBrush
    {
        get => (Brush)GetValue(BackBrushProperty);
        set => SetValue(BackBrushProperty, value);
    }
    public static readonly DependencyProperty BackBrushProperty = DependencyProperty.Register("Backbrush", typeof(Brush), typeof(MotionAreaWindow), new PropertyMetadata("#50FFFFFF".ToBrush()));

    public Brush BorderBrush2
    {
        get => (Brush)GetValue(BorderBrush2Property);
        set => SetValue(BorderBrush2Property, value);
    }
    public static readonly DependencyProperty BorderBrush2Property = DependencyProperty.Register("BorderBrush2", typeof(Brush), typeof(MotionAreaWindow), new PropertyMetadata("#AAFF0000".ToBrush()));

    public bool IsContentRendered = false;

    public MotionAreaWindow()
    {
        DataContext = this;
        InitializeComponent();

        (int x, int y, int w, int h) = Settings.JigRect.Get();
        ContentRendered += (_, _) =>
        {
            if (ImageCapture.IsFullScreen)
            {
                RestoreClientToDesktop(Settings.JigRect.Get());
            }
            else
            {
                if (LaunchCtrl.TryGetHwnd(out IntPtr hwnd))
                {
                    RestoreClient(hwnd, Settings.JigRect.Get());
                }
            }
            IsContentRendered = true;
        };
    }

    public async Task<(int, int, int, int)> ClientToClient(IntPtr hwnd)
    {
        RECT rectJig = NativeMethods.GetWindowRECT(Hwnd);
        RECT rectGame = NativeMethods.GetWindowRECT(hwnd);

        if (!ImageCapture.IsFullScreenMode(hwnd))
        {
            if (rectGame.Left != rectGameLatest.Left || rectGame.Top != rectGameLatest.Top)
            {
                (int offsetX, int offsetY) = (rectGame.Left - rectGameLatest.Left, rectGame.Top - rectGameLatest.Top);
                rectJig = rectJig with
                {
                    Left = rectJig.Left + offsetX,
                    Top = rectJig.Top + offsetY,
                    Right = rectJig.Right + offsetX,
                    Bottom = rectJig.Bottom + offsetY,
                };
                NativeMethods.SetWindowRECT(Hwnd, rectJig, Topmost);
                rectGameLatest = rectGame;
            }

            if (hwnd == IntPtr.Zero)
            {
                return (rectJig.Left, rectJig.Top, rectJig.Right - rectJig.Left, rectJig.Bottom - rectJig.Top);
            }
            else
            {
                return (rectJig.Left - rectGame.Left, rectJig.Top - rectGame.Top, rectJig.Right - rectJig.Left, rectJig.Bottom - rectJig.Top);
            }
        }
        else
        {
            if (rectGame.Left >= 0 && rectGame.Top >= 0)
            {
                NativeMethods.SetWindowRECT(Hwnd, rectJig, true);
                NativeMethods.BringWindowToTop(Hwnd);
            }
            return (rectJig.Left, rectJig.Top, rectJig.Right - rectJig.Left, rectJig.Bottom - rectJig.Top);
        }
    }

    public void ResetClient(IntPtr? hwnd = null)
    {
        Settings.JigRect.Set(Settings.JigRect.DefaultValue);
        RestoreClient(hwnd);
    }

    public void RestoreClientToDesktop((int, int, int, int) rectJig)
    {
        (int x, int y, int w, int h) = rectJig;
        NativeMethods.SetWindowRECT(Hwnd, new()
        {
            Left = x,
            Top = y,
            Right = x + w,
            Bottom = y + h,
        }, Topmost);
    }

    public void RestoreClient(IntPtr? hwnd = null)
    {
        if (hwnd != null && hwnd != IntPtr.Zero)
        {
            if (ImageCapture.IsFullScreen)
            {
                RestoreClientToDesktop(Settings.JigRect.Get());
            }
            else
            {
                RestoreClient(hwnd.Value, Settings.JigRect.Get());
            }
        }
    }

    public void RestoreClient(IntPtr hwnd, (int, int, int, int) rectJig)
    {
        (int x, int y, int w, int h) = rectJig;
        RECT rectGame = NativeMethods.GetWindowRECT(hwnd);
        NativeMethods.SetWindowRECT(Hwnd, rectGame with
        {
            Left = rectGame.Left + x,
            Top = rectGame.Top + y,
            Right = rectGame.Left + x + w,
            Bottom = rectGame.Top + y + h,
        }, Topmost);
    }

    public void SetLayeredWindow(bool isLayered)
    {
        NativeMethods.SetLayeredWindow(Hwnd, isLayered);
#if false // Keep AllowsTransparency true
            _ = NativeMethods.SetLayeredWindowAttributes(jigging.window.MotionArea.Hwnd, 0, 128, NativeMethods.LWA_ALPHA);
#endif
    }

    public void Start()
    {
        BackBrush = "#50FFFFFF".ToBrush();
        BorderBrush2 = "#00FFFFFF".ToBrush();
    }

    public void Stop()
    {
        BackBrush = "#50FFFFFF".ToBrush();
        BorderBrush2 = "#50FF0000".ToBrush();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        foreach (Rect rect in Rects)
        {
            double offsetH = ImageCapture.IsFullScreen ? 0d : 4d * DpiUtils.ScaleXReci;
            dc.DrawRectangle(null, new(Brushes.White, 2d * DpiUtils.ScaleXReci), new(rect.Left * DpiUtils.ScaleXReci, rect.Top * DpiUtils.ScaleYReci + offsetH, rect.Size.Width * DpiUtils.ScaleXReci, rect.Size.Height * DpiUtils.ScaleYReci));
        }
    }
}
