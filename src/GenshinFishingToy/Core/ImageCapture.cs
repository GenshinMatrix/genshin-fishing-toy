using System;
using System.Drawing;
using System.Windows;

namespace GenshinFishingToy.Core;

internal static class ImageCapture
{
    public static int X { get; set; }
    public static int Y { get; set; }
    public static int W { get; set; }
    public static int H { get; set; }

    public static void Setup(int x, int y, int w, int h)
    {
        X = x;
        Y = y;
        W = w;
        H = h;
    }

    public static void Setup((int x, int y, int w, int h) rect)
    {
        Setup(rect.x, rect.y, rect.w, rect.h);
    }

    public static bool IsFullScreen { get; private set; } = false;
    public static bool IsFullScreenMode(IntPtr hwnd)
    {
        int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);

        if ((exStyle & NativeMethods.WS_EX_TOPMOST) != 0)
        {
            return IsFullScreen = true;
        }
        return IsFullScreen = false;
    }

    private static int GetCaptionHeight(IntPtr? hwnd = null)
    {
        int captionHeight = default;

        if (hwnd != null && hwnd != IntPtr.Zero)
        {
            if (!IsFullScreenMode(hwnd.Value))
            {
                captionHeight = (int)(SystemParameters.CaptionHeight * DpiUtils.ScaleY);
            }
        }
        return captionHeight;
    }

    public static Bitmap Capture(IntPtr? hwnd = null)
    {
        return ImageExtension.Capture(X, Y - GetCaptionHeight(hwnd), W, H, hwnd);
    }

    public static Bitmap CaptureLiftingWords(IntPtr? hwnd = null)
    {
        return ImageExtension.Capture(X, Y - GetCaptionHeight(hwnd) + H, W, (int)(H * 2.5d), hwnd);
    }
}
