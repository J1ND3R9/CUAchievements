using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.Linq;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal class PauseMenuButtonHandler : MonoBehaviour
    {

    }
    internal static class PauseMenuButtonBuilder
    {
        public const string ButtonName = "AchievementsPauseButton";

        public static void CreateButton(Transform parent, Action onClick)
        {
            Sprite baseSprite = AssetLoader.GetCachedSprite("achievement_pause_button");

            if (baseSprite == null)
            {
                CUAchievementsPlugin.Logger.LogError("achievement_pause_button sprite not found.");
                return;
            }

            GameObject buttonObject = UIUtility.Create(ButtonName, parent, typeof(Image), typeof(UITooltip), typeof(Button));
            UITooltip buttonTooltip = buttonObject.GetComponent<UITooltip>();

            string tipPauseName = LocaleRegistry.Get("achievement_pause_button_tip_name", "Achievements");
            string tipPauseDesc = LocaleRegistry.Get("achievement_pause_button_tip_desc", "Check your achievement progress!");
            buttonTooltip.tipName = tipPauseName;
            buttonTooltip.tipDesc = tipPauseDesc;

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = baseSprite;
            image.raycastTarget = false;

            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(120f, 120f);

            
        }
    }
    internal static class MenuButtonBuilder
    {
        public const string ButtonName = "AchievementsButton";
        public static void CreateButton(Transform parent, TMP_FontAsset font, Action onClick)
        {
            Sprite baseSprite = AssetLoader.GetCachedSprite("achievement_button");
            Sprite overlaySprite = AssetLoader.GetCachedSprite("achievement_button_overlay");

            if (baseSprite == null)
            {
                CUAchievementsPlugin.Logger.LogError("achievement_button sprite not found.");
                return;
            }

            GameObject buttonObject = UIUtility.Create(ButtonName, parent, typeof(Image));

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = baseSprite;
            image.raycastTarget = false;

            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-300f, 25f);
            rect.sizeDelta = baseSprite.rect.size;

            Image overlay = AddOverlay(rect, overlaySprite);
            TextMeshProUGUI label = AddLabel(rect, font);

            buttonObject.AddComponent<MenuButtonHandler>().Initialize(rect, overlay, label, onClick);
            AddParallax(rect);
        }

        private static void AddParallax(RectTransform rect)
        {
            if (MenuParallax.instance == null) return;

            var existingLayer = MenuParallax.instance.parallax.layers.FirstOrDefault(l => l.reference == rect);
            if (existingLayer == null)
            {
                MenuParallax.ParallaxLayerInfo layer = new MenuParallax.ParallaxLayerInfo
                {
                    reference = rect,
                    mouseMoveScale = 0.20f,
                    originalPos = rect.anchoredPosition
                };
                MenuParallax.instance.parallax.layers.Add(layer);
            }
        }

        private static TextMeshProUGUI AddLabel(Transform parent, TMP_FontAsset font)
        {
            GameObject labelObject = UIUtility.Create("HoverLabel", parent, typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 20f);
            labelRect.localScale = Vector3.one;
            labelRect.SetAsLastSibling();

            string achievementsLocale = LocaleRegistry.Get("achievements_button", "Achievements");

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = achievementsLocale;
            label.font = font;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(1f, 1f, 1f, 0f);
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.ForceMeshUpdate();

            labelRect.sizeDelta = new Vector2(label.preferredWidth + 40f, label.preferredHeight + 10f);

            return label;
        }

        private static Image AddOverlay(Transform parent, Sprite sprite)
        {
            Image overlay = UIUtility.Create("Overlay", parent, typeof(Image)).GetComponent<Image>();
            overlay.transform.Stretch();
            overlay.sprite = sprite;
            overlay.raycastTarget = false;
            overlay.color = new Color(1f, 1f, 1f, 0f);
            return overlay;
        }
    }
}