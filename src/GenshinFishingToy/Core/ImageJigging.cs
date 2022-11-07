using GenshinFishingToy.Models;
using GenshinFishingToy.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Rect = OpenCvSharp.Rect;

namespace GenshinFishingToy.Core;

internal class ImageJigging
{
    private const double frameRate = 60d;
    private List<Rect> rects = new();
    private Rect cur, left, right;
    private int prevMouseEvent = 0x0;
    private int noRectsCount = 0;
    private bool findFishBoxTips = false;
    private bool isFishingProcess = false;
    private Rect baseHookTips = Rect.Empty;
    private int hookTipsExitCount = 0; // 钓鱼提示持续时间
    private int notFishingAfterHookCount = 0; // 提竿后没有钓鱼的时间

    internal GenshinWindow window = new();
    internal Timer timerCapture = new();
    internal bool ticking = false;

    public bool IsRunning { get; protected set; } = false;

    public ImageJigging()
    {
        timerCapture.Tick += Tick;
        timerCapture.Interval = Convert.ToInt32(1000d / frameRate);
        timerCapture.Start();
    }

    public async Task StartFishing()
    {
        await window.Start();
        IsRunning = true;
    }

    public async Task StopFishing()
    {
        IsRunning = false;
        findFishBoxTips = false;
        isFishingProcess = false;
    }

    public async void Tick(object? sender, EventArgs e)
    {
        if (ticking) return;
        ticking = true;
        bool hasHwnd = await window.HasHwnd();

        if (hasHwnd)
        {
            if (window.MotionArea.Visibility != Visibility.Visible)
            {
                window.MotionArea.Show();
                window.MotionArea.SetLayeredWindow(Settings.Lock);
            }
        }
        else
        {
            window.MotionArea.Hide();
        }
#if false // Not important
        if (Application.Current.MainWindow != null)
        {
            NativeMethods.BringWindowToTop(new WindowInteropHelper(Application.Current.MainWindow).Handle);
        }
#endif
        if (!hasHwnd)
        {
            ticking = false;
            return;
        }
        (int, int, int, int) rect = default;
        if (window.MotionArea.Visibility == Visibility.Visible && window.MotionArea.IsContentRendered)
        {
            rect = await window.MotionArea.ClientToClient(window.Hwnd);
            if (rect != Settings.JigRect.Get())
            {
                Settings.JigRect.Set(rect);
                Settings.FullScreenWhenSaved.Set(ImageCapture.IsFullScreen);
                SettingsManager.Save();
            }
        }
        BorderWindowBehavior.SetLocked(window.MotionArea, Settings.Lock);
        if (!IsRunning)
        {
            window.MotionArea.Rects = Array.Empty<Rect>();
            window.MotionArea.Stop();
            ticking = false;
            return;
        }
        window.MotionArea.Start();

        ImageCapture.Setup(rect);

        using Bitmap captured = ImageCapture.Capture(window.Hwnd);
        rects = ImageRecognition.GetRect(captured, Settings.ShowRecognitionJigging);

        window.MotionArea.Rects = rects?.ToArray() ?? Array.Empty<Rect>();

        // 提竿判断
        if (!isFishingProcess)
        {
            using Bitmap liftingWordsAreaBitmap = ImageCapture.CaptureLiftingWords(window.Hwnd);
            if (liftingWordsAreaBitmap == null)
            {
                ticking = false;
                return;
            }
            Rect currHookTips = ImageRecognition.MatchWords(liftingWordsAreaBitmap, Settings.ShowRecognitionLifting);
            if (currHookTips != Rect.Empty)
            {
                if (baseHookTips == Rect.Empty)
                {
                    baseHookTips = currHookTips;
                }
                else
                {
                    if (Math.Abs(baseHookTips.X - currHookTips.X) < 10
                        && Math.Abs(baseHookTips.Y - currHookTips.Y) < 10
                        && Math.Abs(baseHookTips.Width - currHookTips.Width) < 10
                        && Math.Abs(baseHookTips.Height - currHookTips.Height) < 10)
                    {
                        hookTipsExitCount++;
                    }
                    else
                    {
                        hookTipsExitCount = 0;
                        baseHookTips = currHookTips;
                    }
                    if (hookTipsExitCount >= frameRate / 2d)
                    {
                        await window.MouseClick(0, 0);
                        Logger.Info(@"┌------------------------┐");
                        Logger.Info("  自动提竿");
                        isFishingProcess = true;
                        hookTipsExitCount = 0;
                        baseHookTips = Rect.Empty;
                    }
                }
            }
        }

        if (rects != null && rects.Count > 0)
        {
            if (rects.Count >= 2 && prevMouseEvent == 0x0 && !findFishBoxTips)
            {
                findFishBoxTips = true;
                Logger.Info("  识别到钓鱼框，自动拉扯中...");
            }
            // 超过3个矩形是异常情况，取高度最高的三个矩形进行识别
            if (rects.Count > 3)
            {
                rects.Sort((a, b) => b.Height.CompareTo(a.Height));
                rects.RemoveRange(3, rects.Count - 3);
            }

            Logger.Info($"识别到{rects.Count}个矩形");
            if (rects.Count == 2)
            {
                if (rects[0].Width < rects[1].Width)
                {
                    cur = rects[0];
                    left = rects[1];
                }
                else
                {
                    cur = rects[1];
                    left = rects[0];
                }

                if (cur.X < left.X)
                {
                    if (prevMouseEvent != NativeMethods.WM_LBUTTONDOWN)
                    {
                        window.MouseLeftButtonDown();
                        prevMouseEvent = NativeMethods.WM_LBUTTONDOWN;
                        Logger.Info("进度不到 左键按下");
                    }
                }
                else
                {
                    if (prevMouseEvent == NativeMethods.WM_LBUTTONDOWN)
                    {
                        window.MouseLeftButtonUp();
                        prevMouseEvent = NativeMethods.WM_LBUTTONUP;
                        Logger.Info("进度超出 左键松开");
                    }
                }
            }
            else if (rects.Count == 3)
            {
                rects.Sort((a, b) => a.X.CompareTo(b.X));
                left = rects[0];
                cur = rects[1];
                right = rects[2];

                if (right.X + right.Width - (cur.X + cur.Width) <= cur.X - left.X)
                {
                    if (prevMouseEvent == NativeMethods.WM_LBUTTONDOWN)
                    {
                        window.MouseLeftButtonUp();
                        prevMouseEvent = NativeMethods.WM_LBUTTONUP;
                        Logger.Info("进入框内中间 左键松开");
                    }
                }
                else
                {
                    if (prevMouseEvent != NativeMethods.WM_LBUTTONDOWN)
                    {
                        window.MouseLeftButtonDown();
                        prevMouseEvent = NativeMethods.WM_LBUTTONDOWN;
                        Logger.Info("未到框内中间 左键按下");
                    }
                }
            }
        }
        else
        {
            noRectsCount++;
            // 2s 没有矩形视为已经完成钓鱼
            if (noRectsCount >= frameRate * 2d && prevMouseEvent != 0x0)
            {
                findFishBoxTips = false;
                isFishingProcess = false;
                prevMouseEvent = 0x0;
                Logger.Info("  钓鱼结束");
                Logger.Info(@"└------------------------┘");
            }
        }

        // 提竿后没有钓鱼的情况
        if (isFishingProcess && !findFishBoxTips)
        {
            notFishingAfterHookCount++;
            if (notFishingAfterHookCount >= frameRate * 2d)
            {
                isFishingProcess = false;
                notFishingAfterHookCount = 0;
                Logger.Info("  X 提竿后没有钓鱼，重置!");
                Logger.Info(@"└------------------------┘");
            }
        }
        else
        {
            notFishingAfterHookCount = 0;
        }
        ticking = false;
    }
}
