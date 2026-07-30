using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    public static class AchievementButtonBuilder
    {
        private const string ButtonName = "AchievementsButton";
        private const string OverlayName = "Overlay";
        private const string ClickableName = "Clickable";
        private const string HoverLabelName = "HoverLabel";
        public static void EnsureButton(Transform menuBackground, Sprite baseSprite, Sprite overlaySprite, string labelText)
        {
            Transform existing = menuBackground.Find(ButtonName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(ButtonName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(menuBackground, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            ConfigureFullscreenRect(rect);

            Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.sprite = baseSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = Color.white;

            Image overlay = EnsureOverlayImage(rect, overlaySprite);
            RectTransform clickRect = EnsureClickableRect(rect);
            TextMeshProUGUI label = EnsureHoverLabel(rect, labelText);

            AchievementButtonHoverVisual hover = go.GetComponent<AchievementButtonHoverVisual>() ?? go.AddComponent<AchievementButtonHoverVisual>();
            hover.Initialize(clickRect, overlay, label, OnClicked);

            AddParallax(go);
        }

        private static void OnClicked()
        {
            // сюда логика открытия окна достижений
            // пока лэээнь....
        }

        private static void AddParallax(GameObject gameObject)
        {
            MenuParallax.ParallaxLayerInfo layer = new MenuParallax.ParallaxLayerInfo
            {
                reference = gameObject.GetComponent<RectTransform>(),
                mouseMoveScale = 0.25f,
                originalPos = gameObject.GetComponent<RectTransform>().anchoredPosition
            };
            MenuParallax.instance.parallax.layers.Add(layer);
        }

        private static void ConfigureFullscreenRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static Image EnsureOverlayImage(RectTransform parentRect, Sprite overlaySprite)
        {
            Transform existing = parentRect.Find(OverlayName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(OverlayName, typeof(RectTransform), typeof(Image));

            RectTransform rect = go.GetComponent<RectTransform>();
            ConfigureFullscreenRect(rect);
            rect.SetAsLastSibling();

            Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.sprite = overlaySprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0f);

            return image;
        }

        private static RectTransform EnsureClickableRect(RectTransform parentRect)
        {
            Transform existing = parentRect.Find(ClickableName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(ClickableName, typeof(RectTransform));
            go.transform.SetParent(parentRect, false);

            RectTransform rect = go.GetComponent<RectTransform>();

            // TODO: Подсчитать нужные anchor
            rect.anchorMin = new Vector2(0.7f, 0.75f);
            rect.anchorMax = new Vector2(0.85f, 0.9f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            return rect;
        }

        private static TextMeshProUGUI EnsureHoverLabel(RectTransform parentRect, string labelText)
        {
            Transform existing = parentRect.Find(HoverLabelName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(HoverLabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parentRect, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // TODO: Подобрать под свою иконку
            rect.anchoredPosition = new Vector2(0f, 200f);
            rect.sizeDelta = new Vector2(900f, 140f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.color = new Color(1f, 1f, 1f, 0f);
            tmp.raycastTarget = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = FindFont();

            return tmp;
        }

        private static TMP_FontAsset FindFont() => Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Retro GamingPix");
    }
}
