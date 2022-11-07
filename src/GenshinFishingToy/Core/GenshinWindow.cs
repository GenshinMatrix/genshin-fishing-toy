using GenshinFishingToy.Models;
using GenshinFishingToy.Views;
using System;
using System.Threading.Tasks;

namespace GenshinFishingToy.Core;

public class GenshinWindow
{
    protected IntPtr hwnd;
    public IntPtr Hwnd => hwnd;

    public bool IsSetupClient { get; protected set; } = false;

    internal MotionAreaWindow MotionArea = new();

    public GenshinWindow()
    {
        if (LaunchCtrl.TryGetHwnd(out hwnd))
        {
            MotionArea.rectGameLatest = NativeMethods.GetWindowRECT(hwnd);
        }
    }

    public async Task Start()
    {
        await HasHwnd();
        MotionArea.rectGameLatest = NativeMethods.GetWindowRECT(hwnd);
    }

    public async Task<bool> HasHwnd()
    {
        return await LaunchCtrl.IsRunning(async p =>
        {
            hwnd = p?.MainWindowHandle ?? IntPtr.Zero;
        });
    }

    public void MouseLeftButtonDown()
    {
        NativeMethods.Focus(hwnd);
        NativeMethods.PostMessage(hwnd, NativeMethods.WM_LBUTTONDOWN, 0, (0 << 16) | 0);
    }

    public void MouseLeftButtonUp()
    {
        NativeMethods.Focus(hwnd);
        NativeMethods.PostMessage(hwnd, NativeMethods.WM_LBUTTONUP, 0, (0 << 16) | 0);
    }

    public async Task MouseClick(int x, int y)
    {
        NativeMethods.Focus(hwnd);
        NativeMethods.PostMessage(hwnd, NativeMethods.WM_LBUTTONDOWN, 0, (y << 16) | x);
        await Task.Delay(80);
        NativeMethods.PostMessage(hwnd, NativeMethods.WM_LBUTTONUP, 0, (y << 16) | x);
    }
}
