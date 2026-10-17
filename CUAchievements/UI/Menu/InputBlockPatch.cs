using CUAchievements.UI.Menu.CustomButton;
using CUAchievements.UI.Menu.Main;
using HarmonyLib;

namespace CUAchievements.UI
{
    [HarmonyPatch(typeof(AdaptiveButton))]
    internal static class AchievementModalInputBlockPatch
    {
        [HarmonyPatch(nameof(AdaptiveButton.GetDistanceToCursor))]
        [HarmonyPostfix]
        static void DistanceToCursorPatch(ref float __result)
        {
            if (MenuButtonHandler.IsPointerOverAny() || MainMenuWindow.IsOpen)
                __result = 99999f;
        }

        [HarmonyPatch(nameof(AdaptiveButton.overlayActive), methodType: MethodType.Getter)]
        [HarmonyPostfix]
        static void OverlayActiveGetterPatch(ref bool __result)
        {
            __result |= MainMenuWindow.IsOpen;
        }
    }
}
