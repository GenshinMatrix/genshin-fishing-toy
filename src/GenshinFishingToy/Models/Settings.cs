using GenshinFishingToy.Core;
using System.Reflection;

namespace GenshinFishingToy.Models;

[Obfuscation]
public class Settings
{
    public static SettingsDefinition<string> ShortcutKey { get; } = new(nameof(ShortcutKey), "F11");
    public static SettingsDefinition<string> Language { get; } = new(nameof(Language), string.Empty);
    public static SettingsDefinition<(int, int, int, int)> JigRect { get; } = new(nameof(JigRect), (100, 100, 450, 100));
    public static SettingsDefinition<bool> FullScreenWhenSaved { get; } = new(nameof(FullScreenWhenSaved), false);
    public static SettingsDefinition<bool> Lock { get; } = new(nameof(Lock), false);
    public static SettingsDefinition<bool> AutoLifting { get; } = new(nameof(AutoLifting), true);
    public static SettingsDefinition<bool> ShowRecognitionJigging { get; } = new(nameof(ShowRecognitionJigging), false);
    public static SettingsDefinition<bool> ShowRecognitionLifting { get; } = new(nameof(ShowRecognitionLifting), false);
}
