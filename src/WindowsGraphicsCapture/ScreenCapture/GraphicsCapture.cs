using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WPFCaptureSample;

public static class GraphicsCapture
{
    public static MainWindow CaptureWindow;
    public static Bitmap CaptureBitmap => CaptureWindow.Sample?.Capture?.Bitmap!;

    public static Bitmap Capture(int x, int y, int w, int h, IntPtr? hwnd = null)
    {
        if (hwnd == null || hwnd == IntPtr.Zero)
        {
            return null!;
        }

        RECT lpRect = default;
        GetWindowRect(hwnd.Value, ref lpRect);
        float controlsWidth = Math.Max(lpRect.Right - lpRect.Left, 2);

        try
        {
            if (CaptureWindow == null)
            {
                CaptureWindow ??= new();
                CaptureWindow.InitComposition(controlsWidth);
                CaptureWindow.StartHwndCapture(hwnd!.Value);
            }
            if (CaptureWindow != null)
            {
                if (CaptureWindow.Sample?.Capture == null)
                {
                    CaptureWindow.InitComposition(controlsWidth);
                    CaptureWindow.StartHwndCapture(hwnd!.Value);
                }
                if (CaptureWindow.ControlsWidth != controlsWidth)
                {
                    CaptureWindow.StopCapture();
                    CaptureWindow.InitComposition(controlsWidth);
                }
            }

            if (CaptureBitmap != null)
            {
                return CropBitmap(CaptureBitmap, x, y, w, h);
            }
        }
        catch
        {
        }
        return null!;
    }

    public static void Uncapture()
    {
        try
        {
            CaptureWindow?.StopCapture();
        }
        catch
        {
        }
    }

    private static unsafe Bitmap CropBitmap(Bitmap source, int x, int y, int width, int height)
    {
        if (x < 0 || x + width > source.Width || y < 0 || y + height > source.Height)
        {
            return source;
        }

        Bitmap target = new(width, height, PixelFormat.Format32bppArgb);

        BitmapData sourceData = null!;
        BitmapData targetData = null!;

        try
        {
            sourceData = source.LockBits(new Rectangle(x, y, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            targetData = target.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte* sourcePtr = (byte*)sourceData.Scan0;
            byte* targetPtr = (byte*)targetData.Scan0;

            int bytesPerRow = width * 4;

            for (int i = 0; i < height; i++)
            {
                byte* sourceRow = sourcePtr + i * sourceData.Stride;
                byte* targetRow = targetPtr + i * targetData.Stride;

                for (int j = 0; j < bytesPerRow; j++)
                {
                    targetRow[j] = sourceRow[j];
                }
            }
        }
        finally
        {
            if (sourceData != null)
                source.UnlockBits(sourceData);
            if (targetData != null)
                target.UnlockBits(targetData);
        }
        return target;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
