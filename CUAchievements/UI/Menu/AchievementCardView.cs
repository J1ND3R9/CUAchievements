using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu
{
    internal static class AchievementCardView
    {
        public static GameObject Create(Transform parent, AchievementCardModel model, TMP_FontAsset font, Action<RegisteredAchievement> onClick = null)
        {
            var root = UIUtility.Create("AchievementContainer", parent, typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(CanvasGroup));

            root.GetComponent<LayoutElement>().preferredHeight = 140f;
            root.GetComponent<CanvasGroup>().alpha = model.Alpha;
            root.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 10f,
                controlH: false,
                align: TextAnchor.MiddleLeft,
                padding: new RectOffset(25, 25, 5, 5)
                );

            if (model.Source != null)
            {
                root.AddComponent<Image>().color = Color.clear;
                root.AddComponent<AchievementItemUI>().Initialize(model.Source, onClick);
            }

            BuildBackground(root.transform, model.Frame);
            BuildIcon(root.transform, model.Icon);
            BuildTexts(root.transform, model, font);

            return root;
        }

        private static void BuildBackground(Transform parent, Sprite frame)
        {
            GameObject bgObject = UIUtility.Create("Background", parent, typeof(Image), typeof(LayoutIgnorer));
            UIUtility.Stretch(bgObject.transform);

            Image bgImage = bgObject.GetComponent<Image>();
            bgImage.color = Color.black;
            bgImage.raycastTarget = false;

            GameObject frameObject = UIUtility.Create("Frame", parent, typeof(Image), typeof(LayoutIgnorer));
            UIUtility.Stretch(frameObject.transform);

            Image frameImage = frameObject.GetComponent<Image>();
            frameImage.sprite = frame;
            frameImage.type = Image.Type.Sliced;
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;
        }
        private static void BuildIcon(Transform parent, Sprite icon)
        {
            GameObject iconObject = UIUtility.Create("Icon", parent, typeof(Image), typeof(LayoutElement));
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(96f, 96f);

            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 96f;
            iconLayout.preferredHeight = 96f;
            iconLayout.minWidth = 96f;
            iconLayout.minHeight = 96f;

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;

            GameObject iconFrameObject = UIUtility.Create("IconFrame", iconObject.transform, typeof(Image));
            UIUtility.Stretch(iconFrameObject.transform);

            Image iconFrameImage = iconFrameObject.GetComponent<Image>();
            iconFrameImage.sprite = AssetLoader.GetCachedSprite("achievement_frame_image");
            iconFrameImage.type = Image.Type.Simple;
            iconFrameImage.raycastTarget = false;
        }
        private static void BuildTexts(Transform root, AchievementCardModel model, TMP_FontAsset font)
        {
            var text = UIUtility.Create("Text", root, typeof(VerticalLayoutGroup), typeof(LayoutElement));
            text.GetComponent<LayoutElement>().flexibleWidth = 1f;
            text.GetComponent<VerticalLayoutGroup>().Configure(spacing: 4f,
                expandW: true,
                expandH: true);

            var header = UIUtility.Create("Header", text.transform, typeof(HorizontalLayoutGroup));
            header.GetComponent<HorizontalLayoutGroup>().Configure(expandH: true,
                align: TextAnchor.MiddleLeft);

            TextMeshProUGUI title = UIUtility.CreateText(header.transform, "Title", model.Title, font, 24f);
            title.color = Color.white;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.overflowMode = TextOverflowModes.Ellipsis;
            title.maxVisibleLines = 1;

            UIUtility.CreateSpacer(header.transform);

            if (model.Counter != null)
                BuildCounterRow(header.transform, model.Counter, font);

            TextMeshProUGUI desc = UIUtility.CreateText(text.transform, "Description", model.Description, font, 18f);
            desc.color = new Color(1f, 1f, 1f, 0.7f);
            desc.overflowMode = TextOverflowModes.Ellipsis;
            desc.maxVisibleLines = 2;
            desc.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;
        }

        private static void BuildCounterRow(Transform parent, AchievementCardModel.CounterInfo counter, TMP_FontAsset font, float squareSize = 24f)
        {
            GameObject progressObject = UIUtility.Create("ProgressRow", parent, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            progressObject.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 10f, align: TextAnchor.MiddleLeft);
            progressObject.GetComponent<LayoutElement>().preferredHeight = squareSize;

            UIUtility.CreateSquareProgress(progressObject.transform, squareSize, counter.FilledSquares);

            TextMeshProUGUI progressText = UIUtility.CreateText(progressObject.transform, "ProgressNumber", counter.Text, font, 18f);
            progressText.gameObject.AddComponent<LayoutElement>();
            progressText.color = Color.white;
        }
    }
}