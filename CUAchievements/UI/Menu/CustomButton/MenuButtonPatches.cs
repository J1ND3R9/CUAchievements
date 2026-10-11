using HarmonyLib;
using System.Xml.Xsl;
using TMPro;
using UnityEngine;

namespace CUAchievements.UI.Menu.CustomButton
{
    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class MenuButtonPatches // Thanks Custom Structures code.
    {
        static void Postfix(PreRunScript __instance)
        {
            Transform canvas = __instance.mainCanvas.transform;
            Transform menuBackground = canvas.Find("MenuBackground");
            if (menuBackground == null) return;
            if (menuBackground.Find(MenuButtonBuilder.ButtonName) != null) return;

            MainMenuManager menu = MainMenuManager.Create(canvas, GameFonts.Retro);
            MenuButtonBuilder.CreateButton(menuBackground, GameFonts.Retro, menu.Open);
        }
    }

    [HarmonyPatch(typeof(PauseHandler), "Start")]
    internal static class AchievementPauseMenuButtonPatcn
    {
        static void Postfix()
        {

        }
    }
}