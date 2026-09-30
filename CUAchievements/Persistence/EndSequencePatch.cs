using CUAchievements.Services;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUAchievements.Persistence
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
