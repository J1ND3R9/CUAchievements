using HarmonyLib;
using System.Xml.Xsl;
using TMPro;
using UnityEngine;

namespace CUAchievements.UI
{
    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class AchievementMenuButtonPatch // Thanks Custom Structures code.
    {
        private static void Postfix(PreRunScript __instance)
        {
            Transform canvas = __instance.mainCanvas.transform;
            Transform menuBackground = canvas.Find("MenuBackground");
            if (menuBackground == null) return;
            if (menuBackground.Find(MenuButtonBuilder.ButtonName) != null) return;

            TMP_FontAsset font = GameFonts.Retro;
            MenuManager menu = MenuManager.Create(canvas, font);
            MenuButtonBuilder.CreateButton(menuBackground, font, menu.Open);
        }
    }

    [HarmonyPatch(typeof(PauseHandler), "Start")]
    internal static class AchievementPauseMenuButtonPatcn
    {
        private static void Postfix()
        {

        }
    }
}