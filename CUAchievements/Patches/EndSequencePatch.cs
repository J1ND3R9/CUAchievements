using CUAchievements.Services;
using HarmonyLib;

namespace CUAchievements.Patches
{
    [HarmonyPatch(typeof(PlayerCamera), nameof(PlayerCamera.EndSequence))]
    internal static class EndSequencePatch
    {
        [HarmonyPrefix]
        static void Prefix(int type)
        {
            if (type == 0)
                AchievementService.EndRun();
        }
    }
}
