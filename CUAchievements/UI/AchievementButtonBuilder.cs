using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal static class AchievementButtonBuilder
    {
        internal const string ButtonName = "AchievementsButton";
        internal const string OverlayName = "Overlay";
        internal const string ClickableName = "Clickable";
        internal const string HoverLabelName = "HoverLabel";

        internal static AchievementMenuManager achievementMenu;
        internal static void EnsureButton(Transform menuBackground, Sprite baseSprite, Sprite overlaySprite, string labelText, PreRunScript preRunScript)
        {
            Transform existing = menuBackground.Find(ButtonName);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(ButtonName, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(menuBackground, false);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            ConfigureFullscreenRect(rect);

            Image image = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            image.sprite = baseSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = Color.white;

            Image overlay = EnsureOverlayImage(rect, overlaySprite);
            RectTransform clickRect = EnsureClickableRect(rect);
            TextMeshProUGUI label = EnsureHoverLabel(rect, labelText);

            AchievementButtonHoverVisual hover = gameObject.GetComponent<AchievementButtonHoverVisual>() ?? gameObject.AddComponent<AchievementButtonHoverVisual>();
            hover.Initialize(clickRect, overlay, label, OnClicked);

            var canvas = preRunScript.mainCanvas;
            GameObject modalMenu = new GameObject("AchievementModal", typeof(RectTransform));
            modalMenu.transform.SetParent(canvas.transform, false);
            achievementMenu = modalMenu.AddComponent<AchievementMenuManager>();
            achievementMenu.Initialize(FindFont());

            AddParallax(gameObject);
        }

        internal static void OnClicked()
        {
            if (achievementMenu == null) return;
            achievementMenu.SetVisible(true);
        }

        internal static void AddParallax(GameObject gameObject)
        {
            MenuParallax.ParallaxLayerInfo layer = new MenuParallax.ParallaxLayerInfo
            {
                reference = gameObject.GetComponent<RectTransform>(),
                mouseMoveScale = 0.20f,
                originalPos = gameObject.GetComponent<RectTransform>().anchoredPosition
            };
            MenuParallax.instance.parallax.layers.Add(layer);
        }

        internal static void ConfigureFullscreenRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        internal static Image EnsureOverlayImage(RectTransform parentRect, Sprite overlaySprite)
        {
            Transform existing = parentRect.Find(OverlayName);
            GameObject gameObject;
            if (existing != null)
            {
                gameObject = existing.gameObject;
            }
            else
            {
                gameObject = new GameObject(OverlayName, typeof(RectTransform), typeof(Image));
                gameObject.transform.SetParent(parentRect, false);
            }

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            ConfigureFullscreenRect(rect);
            rect.SetAsLastSibling();

            Image image = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            image.sprite = overlaySprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0f);

            return image;
        }

        internal static RectTransform EnsureClickableRect(RectTransform parentRect)
        {
            Transform existing = parentRect.Find(ClickableName);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(ClickableName, typeof(RectTransform));
            gameObject.transform.SetParent(parentRect, false);

            RectTransform rect = gameObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.7f, 0.75f);
            rect.anchorMax = new Vector2(0.85f, 0.9f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            return rect;
        }

        internal static TextMeshProUGUI EnsureHoverLabel(RectTransform parentRect, string labelText)
        {
            Transform existing = parentRect.Find(HoverLabelName);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(HoverLabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parentRect, false);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // TODO: Подобрать под свою иконку
            rect.anchoredPosition = new Vector2(0f, 200f);
            rect.sizeDelta = new Vector2(900f, 140f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            TextMeshProUGUI tmp = gameObject.GetComponent<TextMeshProUGUI>() ?? gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.color = new Color(1f, 1f, 1f, 0f);
            tmp.raycastTarget = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = FindFont();

            return tmp;
        }

        internal static TMP_FontAsset FindFont() => Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Retro GamingPix");
    }
}
