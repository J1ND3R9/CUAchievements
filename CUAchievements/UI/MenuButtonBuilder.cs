using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal static class MenuButtonBuilder
    {
        internal static TMP_FontAsset FindFont() => Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Retro GamingPix");
        internal static AchievementMenuManager achievementMenu;

        public static void CreateButton(Transform parent, PreRunScript preRun)
        {
            if (parent == null || preRun == null) return;

            Transform existing = parent.Find("AchievementsButton");
            GameObject achievementsButtonObject = existing != null ? existing.gameObject : new GameObject("AchievementsButton", typeof(RectTransform), typeof(Image));

            Sprite baseSprite = AssetLoader.GetCachedSprite("achievement_button");
            Sprite overlaySprite = AssetLoader.GetCachedSprite("achievement_button_overlay");
            string achievementsLocale = LocaleRegistry.Get("achievements_button", "Achievements");

            achievementsButtonObject.transform.SetParent(parent, false);

            Image achievementsButtonImage = achievementsButtonObject.GetComponent<Image>();
            achievementsButtonImage.sprite = baseSprite;
            achievementsButtonImage.type = Image.Type.Simple;
            achievementsButtonImage.preserveAspect = false;
            achievementsButtonImage.raycastTarget = false;
            achievementsButtonImage.color = Color.white;

            RectTransform achievementsButtonRect = achievementsButtonObject.GetComponent<RectTransform>();
            achievementsButtonRect.anchorMin = achievementsButtonRect.anchorMax = new Vector2(1f, 0f);
            achievementsButtonRect.pivot = new Vector2(1f, 0f);
            achievementsButtonRect.anchoredPosition = new Vector2(-300f, 25f);
            achievementsButtonRect.sizeDelta = baseSprite.rect.size;

            Image overlayImage = AddOverlay(achievementsButtonRect, overlaySprite);
            TextMeshProUGUI label = AddLabel(achievementsLocale, achievementsButtonRect);

            MenuButtonHoverVisual hover = achievementsButtonObject.GetComponent<MenuButtonHoverVisual>() ?? achievementsButtonObject.AddComponent<MenuButtonHoverVisual>();

            hover.Initialize(achievementsButtonRect, achievementsButtonImage, overlayImage, label, OnClicked);

            var canvas = preRun.mainCanvas;
            GameObject modalMenu = new GameObject("AchievementModal", typeof(RectTransform));
            modalMenu.transform.SetParent(canvas.transform, false);
            achievementMenu = modalMenu.AddComponent<AchievementMenuManager>();
            achievementMenu.Initialize(FindFont());
            AddParallax(achievementsButtonRect);
        }

        private static Image AddOverlay(RectTransform achievementsButtonRect, Sprite overlaySprite)
        {
            Transform existingOverlay = achievementsButtonRect.Find("Overlay");
            GameObject overlayObject = existingOverlay != null ? existingOverlay.gameObject : new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(achievementsButtonRect, false);

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            overlayRect.anchorMax = Vector2.zero;
            overlayRect.anchorMin = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.localScale = Vector3.one;
            overlayRect.SetAsLastSibling();

            Image overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.sprite = overlaySprite;
            overlayImage.type = Image.Type.Simple;
            overlayImage.preserveAspect = false;
            overlayImage.raycastTarget = false;
            overlayImage.color = new Color(1f, 1f, 1f, 0f);
            return overlayImage;
        }

        private static TextMeshProUGUI AddLabel(string achievementsLocale, RectTransform achievementsButtonRect)
        {
            Transform existingLabel = achievementsButtonRect.Find("HoverLabel");
            GameObject labelObject = existingLabel != null ? existingLabel.gameObject : new GameObject("HoverLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(achievementsButtonRect, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 20f);
            labelRect.localScale = Vector3.one;
            labelRect.SetAsLastSibling();

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = achievementsLocale;
            label.font = FindFont();
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(1f, 1f, 1f, 0f);
            label.raycastTarget = false;
            label.enableWordWrapping = false;

            label.ForceMeshUpdate();
            labelRect.sizeDelta = new Vector2(label.preferredWidth + 40f, label.preferredHeight + 10f);
            return label;
        }

        private static void AddParallax(RectTransform achievementsButtonRect)
        {
            var existingLayer = MenuParallax.instance.parallax.layers.FirstOrDefault(l => l.reference == achievementsButtonRect);
            if (existingLayer == null)
            {
                MenuParallax.ParallaxLayerInfo layer = new MenuParallax.ParallaxLayerInfo
                {
                    reference = achievementsButtonRect,
                    mouseMoveScale = 0.20f,
                    originalPos = achievementsButtonRect.anchoredPosition
                };
                MenuParallax.instance.parallax.layers.Add(layer);
            }
        }

        internal static void OnClicked()
        {
            if (achievementMenu == null) return;
            achievementMenu.SetVisible(true);
        }
    }
}