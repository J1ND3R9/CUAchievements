using HarmonyLib;
using System;
using System.Reflection;

namespace CUAchievements.UI
{
    [HarmonyPatch]
    internal static class AchievementModalInputBlockPatch
    {
        private static readonly Type TargetType = AccessTools.TypeByName("AdaptiveButton");
        private static bool Prepare()
        {
            return TargetType != null;
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(TargetType, "GetDistanceToCursor");
        }

        private static void Postfix(ref float __result)
        {
            if (AchievementMenuManager.IsModalOpen)
            {
                __result = 10000f;
            }
        }
    }
}
