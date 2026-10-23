using CUAchievements.Plugin;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.CustomButton
{
    internal static class MenuButtonBuilder
    {
        public const string ButtonName = "Plush";
        public static void CreateButton(Transform parent, TMP_FontAsset font, Action onClick)
        {
            Sprite baseSprite = AssetLoader.GetCachedSprite("achievement_button");
            Sprite glowSprite = AssetLoader.GetCachedSprite("achievement_button_glow");
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
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(-435f, 60f);
            rect.sizeDelta = baseSprite.rect.size;

            Image overlay = AddOverlay(rect, overlaySprite);
            TextMeshProUGUI label = AddLabel(rect, font);
            AddGlow(rect, glowSprite);

            buttonObject.AddComponent<MenuButtonHandler>().Initialize(rect, overlay, label, onClick);
            AddToParallax(rect);
        }

        private static void AddToParallax(RectTransform rect)
        {
            MenuParallax.ParallaxImage parallax = MenuParallax.instance?.parallax;

            if (parallax == null) return;

            float scale = 0.22f;

            MenuParallax.ParallaxLayerInfo experimentLayer = parallax.layers.FirstOrDefault(l => l.reference.transform.name == "Experiment");
            if (experimentLayer != null)
            {
                scale = experimentLayer.mouseMoveScale;
            }
            MenuParallax.ParallaxLayerInfo layer = new()
            {
                reference = rect,
                mouseMoveScale = scale,
                originalPos = rect.anchoredPosition
            };
            parallax.layers.Add(layer);
        }

        private static TextMeshProUGUI AddLabel(Transform parent, TMP_FontAsset font)
        {
            GameObject labelObject = UIUtility.Create("HoverLabel", parent, typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, -20f);
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

        private static Image AddGlow(Transform parent, Sprite sprite)
        {
            Image glow = UIUtility.Create("Glow", parent, typeof(Image)).GetComponent<Image>();
            glow.transform.Stretch();
            glow.sprite = sprite;
            glow.raycastTarget = false;
            return glow;
        }
    }
}