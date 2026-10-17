using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

            MainMenuManager menu = MainMenuManager.GetOrCreate(canvas, GameFonts.Retro);
            MenuButtonBuilder.CreateButton(menuBackground, GameFonts.Retro, menu.Open);
        }
    }

    [HarmonyPatch(typeof(PauseHandler), "Start")]
    internal static class AchievementPauseMenuButtonPatcn
    {
        static void Postfix(PauseHandler __instance, List<Vector2> ___buttonPos)
        {
            Transform pauseMenu = __instance.pauseContainer.transform;
            if (pauseMenu == null) return;
            if (pauseMenu.Find(PauseMenuButtonBuilder.ButtonName) != null) return;

            Transform canvas = PlayerCamera.main.mainCanvas.transform;
            MainMenuManager menu = MainMenuManager.GetOrCreate(canvas, GameFonts.Retro);
            var button = PauseMenuButtonBuilder.CreateButton(pauseMenu, __instance.buttons[0]);

            var uiButton = button.GetComponent<Button>();
            if (uiButton != null)
            {
                uiButton.onClick = new Button.ButtonClickedEvent();
                uiButton.onClick.AddListener(menu.Open);
            }

            __instance.buttons.Insert(0, button);
            ___buttonPos.Insert(0, button.anchoredPosition);
        }
    }
}