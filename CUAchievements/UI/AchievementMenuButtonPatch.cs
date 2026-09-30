using HarmonyLib;
using System.Xml.Xsl;
using UnityEngine;

namespace CUAchievements.UI
{
    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class AchievementMenuButtonPatch // Thanks Custom Structures code.
    {
        public static void Postfix(PreRunScript __instance)
        {
            Transform menuBackground = __instance.mainCanvas.transform.Find("MenuBackground");
            if (menuBackground == null) return;

            MenuButtonBuilder.CreateButton(menuBackground, __instance);
        }
    }
}