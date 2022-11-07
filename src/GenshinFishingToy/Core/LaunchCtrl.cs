using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GenshinFishingToy.Core;

internal class LaunchCtrl
{
    public static async Task<bool> SearchRunning()
    {
        return await Task.Run(() =>
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("YuanShen");

                if (processes.Length <= 0)
                {
                    processes = Process.GetProcessesByName("Genshin Impact");
                }
                if (processes.Length <= 0)
                {
                    processes = Process.GetProcessesByName("GenshinImpact");
                }
                return processes.Length > 0;
            }
            catch
            {
            }
            return false;
        });
    }

    public static async Task<bool> IsRunning(Func<Process?, Task> callback = null!)
    {
        return await Task.Run(async () =>
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("YuanShen");

                if (processes.Length <= 0)
                {
                    processes = Process.GetProcessesByName("Genshin Impact");
                }
                if (processes.Length <= 0)
                {
                    processes = Process.GetProcessesByName("GenshinImpact");
                }
                if (processes.Length > 0)
                {
                    foreach (Process? process in processes)
                    {
                        await callback?.Invoke(process)!;
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        });
    }

    public static bool TryGetHwnd(out IntPtr hwnd)
    {
        try
        {
            Process[] processes = Process.GetProcessesByName("YuanShen");

            if (processes.Length <= 0)
            {
                processes = Process.GetProcessesByName("Genshin Impact");
            }
            if (processes.Length <= 0)
            {
                processes = Process.GetProcessesByName("GenshinImpact");
            }
            if (processes.Length > 0)
            {
                foreach (Process? process in processes)
                {
                    hwnd = process?.MainWindowHandle ?? IntPtr.Zero;
                    return true;
                }
            }
        }
        catch
        {
        }

        hwnd = IntPtr.Zero;
        return false;
    }

    public static async Task<IntPtr> Launch(LaunchParameter launchParameter = null!)
    {
        if (string.IsNullOrEmpty(GenshinRegedit.InstallPath))
        {
            NoticeService.AddNotice(Mui("Tips"), "Failed", "Genshin Impact not installed.");
        }
        else
        {
            const string GameFolderName = "Genshin Impact Game";

            string fileName = Path.Combine(GenshinRegedit.InstallPath, GameFolderName, "YuanShen.exe");

            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(GenshinRegedit.InstallPath, GameFolderName, "GenshinImpact.exe");
            }

            Process? p = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Path.Combine(GenshinRegedit.InstallPath, GameFolderName, fileName),
                Arguments = (launchParameter ?? new()).ToString(),
                WorkingDirectory = Environment.CurrentDirectory,
                Verb = "runas",
            });
            return p?.MainWindowHandle ?? IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    internal class LaunchParameter
    {
        public bool? IsFullScreen { get; set; } = null;
        public int? ScreenWidth { get; set; } = null;
        public int? ScreenHeight { get; set; } = null;

        public override string ToString()
        {
            StringBuilder sb = new();

            if (IsFullScreen != null)
            {
                sb.Append("-screen-fullscreen").Append(' ').Append(IsFullScreen.Value ? 1 : 0);
            }
            if (ScreenWidth != null)
            {
                sb.Append("-screen-width").Append(' ').Append(ScreenWidth);
            }
            if (ScreenHeight != null)
            {
                sb.Append("-screen-height").Append(' ').Append(ScreenHeight);
            }
            return sb.ToString();
        }
    }
}
