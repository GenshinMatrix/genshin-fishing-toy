using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenshinFishingToy.Core;

internal static class ImageExtension
{
    public static ImageSource ToImageSource(this Bitmap bitmap)
    {
        IntPtr hBitmap = bitmap.GetHbitmap();
        ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        _ = NativeMethods.DeleteObject(hBitmap);
        return imageSource;
    }

    public static BitmapSource ToBitmapSource(this Bitmap bitmap)
    {
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        catch
        {
        }
        return null!;
    }

    public static Bitmap Capture(int x, int y, int w, int h, IntPtr? hwnd = null)
    {
        try
        {
            Bitmap copied = new(w, h);
            using Graphics g = Graphics.FromImage(copied);
            IntPtr hdcDest = g.GetHdc();
            IntPtr hdcSrc = NativeMethods.GetDC(hwnd ?? NativeMethods.GetDesktopWindow());
            _ = NativeMethods.StretchBlt(hdcDest, 0, 0, w, h, hdcSrc, x, y, w, h, CopyPixelOperation.SourceCopy);
            g.ReleaseHdc();
            _ = NativeMethods.DeleteDC(hdcDest);
            _ = NativeMethods.DeleteDC(hdcSrc);
            return copied;
        }
        catch
        {
        }
        return null!;
    }

    public static Bitmap CaptureRodWordsArea(int x, int y, int w, int h, IntPtr? hwnd = null)
    {
        return Capture(x, y + h, w, h * 2, hwnd);
    }
}
