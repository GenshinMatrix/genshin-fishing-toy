using GenshinFishingToy.Core;
using System;

namespace GenshinFishingToy;

internal static class Pack
{
    public static string Name => "GenshinFishingToy";
    public static string Alias => "genshin-fishing-toy";
    public static string Url => "https://github.com/genshin-matrix/genshin-fishing-toy/releases";
    public static string Version => AssemblyUtils.GetAssemblyVersion(typeof(App).Assembly, prefix: "v");

    public static string SupportedOSVersion => "Windows 10.0.18362.0";
    public static string CurrentOSVersion => $"Windows {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.MajorRevision}.{Environment.OSVersion.Version.Build}.{Environment.OSVersion.Version.Minor}";
    public static bool IsSupported => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 18362;
}
