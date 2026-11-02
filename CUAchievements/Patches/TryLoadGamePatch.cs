using CUAchievements.Services;
using HarmonyLib;

namespace CUAchievements.Patches
{
    [HarmonyPatch(typeof(SaveSystem), nameof(SaveSystem.TryLoadGame))]
    internal static class TryLoadGamePatch
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            AchievementService.ResetRunState();
        }
    }
}
