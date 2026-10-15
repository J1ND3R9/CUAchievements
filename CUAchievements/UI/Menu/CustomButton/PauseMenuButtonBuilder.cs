using CUAchievements.Anticheat;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.CustomButton
{
    internal class PauseMenuButtonBuilder : MonoBehaviour
    {
        public const string ButtonName = "Plush";

        private static UITooltip buttonTooltip;
        public static RectTransform CreateButton(Transform parent, TMP_FontAsset font, RectTransform buttonTemplate)
        {
            Sprite sprite = AssetLoader.GetCachedSprite("achievement_pause_button");

            if (sprite == null)
            {
                CUAchievementsPlugin.Logger.LogError("achievement_pause_button sprite not found.");
                return null;
            }

            GameObject buttonObject = Instantiate(buttonTemplate, parent).gameObject;

            buttonObject.name = "Plush";

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = sprite;

            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchoredPosition = new Vector2(380f, -180f);
            rect.offsetMax = new Vector2(440, -120f);
            rect.offsetMin = new Vector2(320, -240f);

            buttonTooltip = buttonObject.GetComponent<UITooltip>();

            SetTooltip();

            return (RectTransform)buttonObject.transform;
        }

        public static void SetTooltip()
        {
            string tipName = LocaleRegistry.Get("achievements_pause_button_tip_name", "Achievements");
            string tipDesc = LocaleRegistry.Get("achievements_pause_button_tip_desc", "Show my achievements!");

            buttonTooltip.skipLocale = true;
            buttonTooltip.tipName = tipName;
            buttonTooltip.tipDesc = tipDesc;

            if (WorldGeneration.world.biomeOverride == WorldGeneration.OverrideSceneType.Tutorial)
            {
                string tipDescAdditional = LocaleRegistry.Get("achievements_pause_button_tip_desc_tutorial", "<color=\"orange\">ATTENTION!</color> Achievements will not be unlocked in the tutorial or sandbox!");
                buttonTooltip.tipDesc += $"\n\n{tipDescAdditional}";
            }
            else if (RunIntegrity.Cheated)
            {
                string tipDescAdditional = LocaleRegistry.Get("achievements_pause_button_tip_desc_cheated", "<color=\"orange\">ATTENTION!</color>Achievements will not be unlocked because this run is marked as \"cheated\"!");
                buttonTooltip.tipDesc += $"\n\n{tipDescAdditional}";
            }
        }
    }
}