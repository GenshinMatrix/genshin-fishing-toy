using System;
using System.Runtime.InteropServices;

namespace GenshinFishingToy.Core;

internal class NativeMethods
{
    public const int WM_NCHITTEST = 0x0084;
    public const int WM_SYSCOMMAND = 0x0112;
    public const int WM_HOTKEY = 0x0312;
    public const int WM_LBUTTONDOWN = 0x201;
    public const int WM_LBUTTONUP = 0x202;
    public const int WM_RBUTTONDBLCLK = 0x0206;
    public const int WM_MBUTTONDOWN = 0x0207;
    public const int WM_STYLECHANGING = 0x007C;
    public const int WM_SETCURSOR = 0x0020;
    public const int WM_NCLBUTTONDBLCLK = 0x00A3;

    public const int SC_RESTORE = 0xF120;
    public const int LWA_ALPHA = 2;

    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;

    public const int WS_EX_TOPMOST = 0x0008;
    public const int WS_EX_TRANSPARENT = 0x20;
    public const int WS_EX_LAYERED = 0x80000;
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    public const int WS_MINIMIZEBOX = 0x00020000;
    public const int WS_MAXIMIZEBOX = 0x00010000;

    public const int HWND_TOPMOST = -1;
    public const int HWND_NOTOPMOST = -2;

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.DLL", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
    public static extern bool StretchBlt(IntPtr hdcDest, int nXDest, int nYDest, int nDestWidth, int nDestHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int nSrcWidth, int nSrcHeight, CopyPixelOperation dwRop);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32")]
    public static extern int mouse_event(MouseEventFlag dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll ")]
    public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childe, string strclass, string FrmText);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("User32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [DllImport("user32.dll")]
    public static extern void BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

    [DllImport("kernel32.dll")]
    public static extern int GetLastError();

    public static void Focus(IntPtr hwnd)
    {
        _ = SendMessage(hwnd, WM_SYSCOMMAND, SC_RESTORE, 0);
        _ = SetForegroundWindow(hwnd);
        while (IsIconic(hwnd))
        {
            continue;
        }
#if false // Left jigging window to top
        BringWindowToTop(hwnd);
#endif
    }

    public static RECT GetWindowRECT(IntPtr hwnd)
    {
        RECT rect = new();
        _ = GetWindowRect(hwnd, ref rect);
        return rect;
    }

    public static void SetWindowRECT(IntPtr hwnd, RECT rect, bool topMost = false)
    {
        _ = SetWindowPos(hwnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 0);
    }

    public static void SetToolWindow(IntPtr hwnd)
    {
        int style = GetWindowLong(hwnd, GWL_EXSTYLE);

        style |= WS_EX_TOOLWINDOW;
        _ = SetWindowLong(hwnd, GWL_EXSTYLE, style);
    }

    public static void SetLayeredWindow(IntPtr hwnd, bool isLayered = true)
    {
        int style = GetWindowLong(hwnd, GWL_EXSTYLE);

        if (isLayered)
        {
            style |= WS_EX_TRANSPARENT;
            style |= WS_EX_LAYERED;
        }
        else
        {
            style &= ~WS_EX_TRANSPARENT;
            style &= ~WS_EX_LAYERED;
        }
        _ = SetWindowLong(hwnd, GWL_EXSTYLE, style);
    }
}

[Flags]
internal enum CopyPixelOperation
{
    NoMirrorBitmap = -2147483648,

    /// <summary>dest = BLACK, 0x00000042</summary>
    Blackness = 66,

    ///<summary>dest = (NOT src) AND (NOT dest), 0x001100A6</summary>
    NotSourceErase = 1114278,

    ///<summary>dest = (NOT source), 0x00330008</summary>
    NotSourceCopy = 3342344,

    ///<summary>dest = source AND (NOT dest), 0x00440328</summary>
    SourceErase = 4457256,

    /// <summary>dest = (NOT dest), 0x00550009</summary>
    DestinationInvert = 5570569,

    /// <summary>dest = pattern XOR dest, 0x005A0049</summary>
    PatInvert = 5898313,

    ///<summary>dest = source XOR dest, 0x00660046</summary>
    SourceInvert = 6684742,

    ///<summary>dest = source AND dest, 0x008800C6</summary>
    SourceAnd = 8913094,

    /// <summary>dest = (NOT source) OR dest, 0x00BB0226</summary>
    MergePaint = 12255782,

    ///<summary>dest = (source AND pattern), 0x00C000CA</summary>
    MergeCopy = 12583114,

    ///<summary>dest = source, 0x00CC0020</summary>
    SourceCopy = 13369376,

    /// <summary>dest = source OR dest, 0x00EE0086</summary>
    SourcePaint = 15597702,

    /// <summary>dest = pattern, 0x00F00021</summary>
    PatCopy = 15728673,

    /// <summary>dest = DPSnoo, 0x00FB0A09</summary>
    PatPaint = 16452105,

    /// <summary>dest = WHITE, 0x00FF0062</summary>
    Whiteness = 16711778,

    /// <summary>
    /// Capture window as seen on screen.  This includes layered windows 
    /// such as WPF windows with AllowsTransparency="true", 0x40000000
    /// </summary>
    CaptureBlt = 1073741824,
}

[Flags]
public enum MouseEventFlag : uint
{
    Move = 0x0001,
    LeftDown = 0x0002,
    LeftUp = 0x0004,
    RightDown = 0x0008,
    RightUp = 0x0010,
    MiddleDown = 0x0020,
    MiddleUp = 0x0040,
    XDown = 0x0080,
    XUp = 0x0100,
    Wheel = 0x0800,
    VirtualDesk = 0x4000,
    Absolute = 0x8000
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

[Flags]
public enum ModifierKeys : uint
{
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

public enum HitTest : int
{
    HTERROR = -2,
    HTTRANSPARENT = -1,
    HTNOWHERE = 0,
    HTCLIENT = 1,
    HTCAPTION = 2,
    HTSYSMENU = 3,
    HTGROWBOX = 4,
    HTSIZE = HTGROWBOX,
    HTMENU = 5,
    HTHSCROLL = 6,
    HTVSCROLL = 7,
    HTMINBUTTON = 8,
    HTMAXBUTTON = 9,
    HTLEFT = 10,
    HTRIGHT = 11,
    HTTOP = 12,
    HTTOPLEFT = 13,
    HTTOPRIGHT = 14,
    HTBOTTOM = 15,
    HTBOTTOMLEFT = 16,
    HTBOTTOMRIGHT = 17,
    HTBORDER = 18,
    HTREDUCE = HTMINBUTTON,
    HTZOOM = HTMAXBUTTON,
    HTSIZEFIRST = HTLEFT,
    HTSIZELAST = HTBOTTOMRIGHT,
    HTOBJECT = 19,
    HTCLOSE = 20,
    HTHELP = 21,
}

public enum DeviceCaps
{
    HORZRES = 8,
    VERTRES = 10,
    LOGPIXELSX = 88,
    LOGPIXELSY = 90,
    DESKTOPVERTRES = 117,
    DESKTOPHORZRES = 118,
}

public struct STYLESTRUCT
{
    public int styleOld;
    public int styleNew;
}
