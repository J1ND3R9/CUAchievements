using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using System.Linq;
using System.Xml.Xsl;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class AchievementButtonPatch // Thanks Custom Structures code.
    {
        public static void Postfix(PreRunScript __instance)
        {
            Transform menuBackground = __instance.mainCanvas.transform.Find("MenuBackground");
            if (menuBackground == null) return;

            Sprite baseSprite = AssetLoader.GetCachedSprite("achievement_button");
            Sprite overlaySprite = AssetLoader.GetCachedSprite("achievement_button_overlay");

            string achievementsLocale = LocaleRegistry.Get("achievements_button_title", "Achievements");
            AchievementButtonBuilder.EnsureButton(menuBackground, baseSprite, overlaySprite, LocaleRegistry.Get("achievementsbutton", achievementsLocale), __instance);
        }
    }
}