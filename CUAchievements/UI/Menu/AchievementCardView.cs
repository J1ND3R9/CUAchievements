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
            GameObject root = UIUtility.Create("AchievementContainer", parent, typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(CanvasGroup));

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

        public static GameObject CreateRevealableHidden(Transform parent, AchievementCardModel model, TMP_FontAsset font, Action<RegisteredAchievement> onClick = null)
        {
            RevealCover cover = null;

            GameObject root = Create(parent, model, font, a =>
            {
                if (cover != null)
                {
                    cover.Reveal();
                    return;
                }
                onClick?.Invoke(a);
            });

            //root.GetComponent<CanvasGroup>().alpha = 1f;

            if (!RevealCover.IsRevealed(model.Source.FullId))
                cover = RevealCoverBuilder.Build(root.transform, model.Source.FullId, font);

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
        private static void BuildTexts(Transform parent, AchievementCardModel model, TMP_FontAsset font)
        {
            GameObject text = UIUtility.Create("Text", parent, typeof(VerticalLayoutGroup), typeof(LayoutElement));
            text.GetComponent<LayoutElement>().flexibleWidth = 1f;
            text.GetComponent<VerticalLayoutGroup>().Configure(spacing: 4f,
                expandW: true,
                expandH: true);

            GameObject header = UIUtility.Create("Header", text.transform, typeof(HorizontalLayoutGroup));
            header.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 12f,
                expandH: true,
                align: TextAnchor.MiddleLeft);

            TextMeshProUGUI title = UIUtility.CreateText(header.transform, "Title", model.Title, font, 24f);
            title.color = Color.white;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Left;
            title.overflowMode = TextOverflowModes.Ellipsis;
            title.maxVisibleLines = 1;
            title.enableWordWrapping = false;

            LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1f;
            titleLayout.preferredWidth = 0f;

            if (model.Counter != null)
                BuildCounterRow(header.transform, model.Counter, font);

            TextMeshProUGUI desc = UIUtility.CreateText(text.transform, "Description", model.ShortDescription, font, 18f);
            desc.color = new Color(1f, 1f, 1f, 0.7f);
            desc.overflowMode = TextOverflowModes.Ellipsis;
            desc.maxVisibleLines = 2;
            desc.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;
        }

        private static void BuildCounterRow(Transform parent, AchievementCardModel.CounterInfo counter, TMP_FontAsset font, float squareSize = 24f)
        {
            GameObject progressObject = UIUtility.Create("ProgressRow", parent, typeof(HorizontalLayoutGroup));
            progressObject.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 10f, align: TextAnchor.MiddleLeft);

            TextMeshProUGUI progressText = UIUtility.CreateText(progressObject.transform, "ProgressNumber", counter.Text, font, 18f);
            progressText.gameObject.AddComponent<LayoutElement>();
            progressText.color = Color.white;
            progressText.enableWordWrapping = false;
            progressText.overflowMode = TextOverflowModes.Ellipsis;

            UIUtility.CreateSquareProgress(progressObject.transform, squareSize, counter.FilledSquares);
        }
    }
}