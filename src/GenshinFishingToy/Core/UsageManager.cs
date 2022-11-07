using GenshinFishingToy.Views;
using System.Threading.Tasks;

namespace GenshinFishingToy.Core;

internal static class UsageManager
{
    public static async Task ShowUsage()
    {
        await DialogWindow.ShowMessageContent(Mui("Usage"), new UsageContent());
    }
}
