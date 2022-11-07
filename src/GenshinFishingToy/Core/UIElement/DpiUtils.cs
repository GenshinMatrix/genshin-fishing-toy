using System;
using System.Windows;

namespace GenshinFishingToy.Core;

public class DpiUtils
{
    public static DpiScale GetScale()
    {
        IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
        float scaleX = NativeMethods.GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSX) / 96f;
        float scaleY = NativeMethods.GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSY) / 96f;
        NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
        return new(scaleX, scaleY);
    }

    public static double ScaleX => GetScale().DpiScaleX;
    public static double ScaleY => GetScale().DpiScaleY;

    public static double ScaleXReci => 1d / ScaleX;
    public static double ScaleYReci => 1d / ScaleY;
}
