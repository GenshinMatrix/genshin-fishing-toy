using GenshinFishingToy.Core;
using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Cursor2 = System.Windows.Forms.Cursor;
using Cursors2 = System.Windows.Forms.Cursors;

namespace GenshinFishingToy.Views;

public class BorderWindowBehavior : Behavior<Window>
{
    internal const int resizeMargin = 10;

    public static bool GetLocked(DependencyObject obj) => (bool)obj.GetValue(LockedProperty);
    public static void SetLocked(DependencyObject obj, bool value) => obj.SetValue(LockedProperty, value);
    public static readonly DependencyProperty LockedProperty =DependencyProperty.RegisterAttached("Locked", typeof(bool), typeof(BorderWindowBehavior), new PropertyMetadata(false, OnLockedChanged));

    private double Width
    {
        get => AssociatedObject.Width;
        set => AssociatedObject.Width = value;
    }
    private double Height
    {
        get => AssociatedObject.Height;
        set => AssociatedObject.Height = value;
    }

    private double ActualWidth => AssociatedObject.ActualWidth;
    private double ActualHeight => AssociatedObject.ActualHeight;

    private double Top => AssociatedObject.Top;
    private double Left => AssociatedObject.Left;

    private Rect TopRect => new(0, 0, Width, resizeMargin);
    private Rect LeftRect => new(0, 0, resizeMargin, Height);
    private Rect BottomRect => new(0, Height - resizeMargin, Width, resizeMargin);
    private Rect RightRect => new(Width - resizeMargin, 0, resizeMargin, Height);
    private Rect TopLeftRect => new(0, 0, resizeMargin, resizeMargin);
    private Rect TopRightRect => new(Width - resizeMargin, 0, resizeMargin, resizeMargin);
    private Rect BottomLeftRect => new(0, Height - resizeMargin, resizeMargin, resizeMargin);
    private Rect BottomRightRect => new(Width - resizeMargin, Height - resizeMargin, resizeMargin, resizeMargin);

    private Cursor Cursor
    {
        get => AssociatedObject.Cursor;
        set => AssociatedObject.Cursor = value;
    }

    internal static Cursor2 Cursor2
    {
        get => Cursor2.Current!;
        set => Cursor2.Current = value;
    }

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += Loaded;
        AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
        AssociatedObject.MouseMove += MouseMove;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= Loaded;
        AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
        AssociatedObject.MouseMove -= MouseMove;
        base.OnDetaching();
    }

    private void Loaded(object sender, RoutedEventArgs e)
    {
        if (PresentationSource.FromVisual(AssociatedObject) as HwndSource is HwndSource hwndSource)
        {
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }
    }

    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (GetLocked(AssociatedObject))
        {
            Cursor = Cursors.No;
            return;
        }

        Point point = e.GetPosition(AssociatedObject);

        if (TopLeftRect.Contains(point)) Cursor = Cursors.SizeNWSE;
        else if (TopRightRect.Contains(point)) Cursor = Cursors.SizeNESW;
        else if (BottomLeftRect.Contains(point)) Cursor = Cursors.SizeNESW;
        else if (BottomRightRect.Contains(point)) Cursor = Cursors.SizeNWSE;
        else if (TopRect.Contains(point)) Cursor = Cursors.SizeNS;
        else if (LeftRect.Contains(point)) Cursor = Cursors.SizeWE;
        else if (RightRect.Contains(point)) Cursor = Cursors.SizeWE;
        else if (BottomRect.Contains(point)) Cursor = Cursors.SizeNS;
        else Cursor = Cursors.SizeAll;
    }

    protected void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (GetLocked(AssociatedObject)) return;
        if (Cursor == Cursors.SizeAll)
        {
            AssociatedObject.DragMove();
        }
    }

    private static void OnLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (GetLocked(AssociatedObject)) return IntPtr.Zero;

        double r = resizeMargin;
        (double top, double left, double w, double h) = (Top * DpiUtils.ScaleX, Left * DpiUtils.ScaleX, ActualWidth * DpiUtils.ScaleX, ActualHeight * DpiUtils.ScaleX);

        switch (msg)
        {
            case NativeMethods.WM_STYLECHANGING:
                // 想要让窗口透明穿透鼠标和触摸等，需要同时设置 WS_EX_LAYERED 和 WS_EX_TRANSPARENT 样式，
                // 确保窗口始终有 WS_EX_LAYERED 这个样式，并在开启穿透时设置 WS_EX_TRANSPARENT 样式
                // 但是WPF窗口在未设置 AllowsTransparency = true 时，会自动去掉 WS_EX_LAYERED 样式（在 HwndTarget 类中)，
                // 如果设置了 AllowsTransparency = true 将使用WPF内置的低性能的透明实现，
                // 所以这里通过 Hook 的方式，在不使用WPF内置的透明实现的情况下，强行保证这个样式存在。
                if ((int)wParam == NativeMethods.GWL_EXSTYLE)
                {
                    if (Marshal.PtrToStructure(lParam, typeof(STYLESTRUCT)) is STYLESTRUCT styleStruct)
                    {
                        styleStruct.styleNew |= NativeMethods.WS_EX_LAYERED;
                        Marshal.StructureToPtr(styleStruct, lParam, false);
                        handled = true;
                    }
                }
                break;
            case NativeMethods.WM_SETCURSOR:
                if ((lParam.ToInt32() & 0xffff) == (int)HitTest.HTCAPTION)
                {
                    Cursor = Cursors.SizeAll;
                    Cursor2 = Cursors2.SizeAll;
                    return IntPtr.Zero;
                }
                break;
            case NativeMethods.WM_RBUTTONDBLCLK:
            case NativeMethods.WM_MBUTTONDOWN:
            case NativeMethods.WM_NCLBUTTONDBLCLK:
                handled = true;
                break;
            case NativeMethods.WM_NCHITTEST:
                Point point = new()
                {
                    X = lParam.ToInt32() & 0xFFFF,
                    Y = lParam.ToInt32() >> 16
                };
                // 窗口左上角
                if (point.Y - top <= r && point.X - left <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTTOPLEFT);
                }
                // 窗口左下角
                else if (h + top - point.Y <= r && point.X - left <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTBOTTOMLEFT);
                }
                // 窗口右上角
                else if (point.Y - top <= r && w + left - point.X <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTTOPRIGHT);
                }
                // 窗口右下角
                else if (w + left - point.X <= r && h + top - point.Y <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                }
                // 窗口左侧
                else if (point.X - left <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTLEFT);
                }
                // 窗口右侧
                else if (w + left - point.X <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTRIGHT);
                }
                // 窗口上方
                else if (point.Y - top <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTTOP);
                }
                // 窗口下方
                else if (h + top - point.Y <= r)
                {
                    handled = true;
                    return new IntPtr((int)HitTest.HTBOTTOM);
                }
                // 窗口移动
                else
                {
#if false
                    Cursor = Cursors.SizeAll;
                    Cursor2 = Cursors2.SizeAll;
                    return new IntPtr((int)HitTest.HTCAPTION);
#endif
                }
                break;
        }
        return IntPtr.Zero;
    }
}
