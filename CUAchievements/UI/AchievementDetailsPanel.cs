using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal sealed class AchievementDetailsPanel : MonoBehaviour
    {
        private RectTransform detailsPanelRect;
        private Image detailIcon;
        private Image detailIconFrame;
        private TextMeshProUGUI detailTitle;
        private TextMeshProUGUI detailDescription;
        private TextMeshProUGUI detailSource;
        private TextMeshProUGUI detailUnlocked;
        private TextMeshProUGUI detailUnlockedAtDate;
        private TextMeshProUGUI detailUnlockedAtTime;
        private bool isVisible = false;
        private Vector2 hiddenPos = new(-600f, 0f);
        private Vector2 shownPos = new(20f, 0f);

        public static AchievementDetailsPanel Create(Transform parent)
        {
            Transform panelTransform = parent.Find("Panel");
            if (panelTransform == null) return null;

            GameObject detailsPanel = Instantiate(panelTransform, parent).gameObject;
            detailsPanel.name = "DetailsPanel";

            AchievementDetailsPanel panelComponent = detailsPanel.AddComponent<AchievementDetailsPanel>();
            panelComponent.Initialize();

            return panelComponent;
        }

        private void Initialize()
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Scroll")
                {
                    var content = child.Find("Content");
                    if (content != null)
                    {
                        foreach (Transform child2 in content.transform)
                            Destroy(child2.gameObject);
                    }
                    continue;
                }
                if (child.name != "Description")
                    Destroy(child.gameObject);
            }

            GameObject detailsScroll = transform.Find("Scroll").gameObject;

            RectTransform descriptionRect = transform.Find("Description").GetComponent<RectTransform>();
            descriptionRect.sizeDelta = new Vector2(370f, 50f);

            RectTransform detailsScrollRect = detailsScroll.GetComponent<RectTransform>();
            detailsScrollRect.sizeDelta = new Vector2(370f, 700f);

            TextMeshProUGUI descriptionTemplateTmp = GetComponentInChildren<TextMeshProUGUI>();
            if (descriptionTemplateTmp != null)
            {
                string detailsLocale = LocaleRegistry.Get("detailspanel_details", "Achievement details");
                descriptionTemplateTmp.text = detailsLocale;
            }

            RectTransform detailsRect = GetComponent<RectTransform>();
            detailsRect.anchorMax = new Vector2(0f, 0.5f);
            detailsRect.anchorMin = new Vector2(0f, 0.5f);
            detailsRect.pivot = new Vector2(0f, 0.5f);
            detailsRect.sizeDelta = new Vector2(380f, 800f);
            detailsRect.anchoredPosition = hiddenPos;

            detailsPanelRect = detailsRect;

            Transform contentTransform = detailsScroll.transform.Find("Viewport/Content");

            GameObject detailsObject = new GameObject("Details", typeof(RectTransform), typeof(VerticalLayoutGroup));
            detailsObject.transform.SetParent(contentTransform, false);

            VerticalLayoutGroup detailsVerticalLayout = detailsObject.GetComponent<VerticalLayoutGroup>();
            detailsVerticalLayout.spacing = 16f;
            detailsVerticalLayout.childAlignment = TextAnchor.UpperCenter;

            CreateHeader(detailsObject);

            UIUtility.CreateSeparator(detailsObject.transform);

            CreateDescription(detailsObject);

            UIUtility.CreateSeparator(detailsObject.transform);

            GameObject bottomSectionObject = new("BottomSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            bottomSectionObject.transform.SetParent(detailsObject.transform, false);

            TextMeshProUGUI unlockedTmp = UIUtility.CreateText(bottomSectionObject.transform, "Unlocked", "Unlocked", GameFonts.Retro, 18f);
            detailUnlocked = unlockedTmp;
            detailUnlocked.font = GameFonts.Retro;

            (TextMeshProUGUI unlockedDate, TextMeshProUGUI unlockedTime) = CreateStatisticRow(bottomSectionObject.transform, "UnlockedAt", "-", "-", GameFonts.Retro);
            detailUnlockedAtDate = unlockedDate;
            detailUnlockedAtTime = unlockedTime;
        }

        private void CreateDescription(GameObject detailsObject)
        {
            GameObject descriptionSectionObject = new GameObject("DescriptionSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            descriptionSectionObject.transform.SetParent(detailsObject.transform, false);

            TextMeshProUGUI descriptionTmp = UIUtility.CreateText(descriptionSectionObject.transform, "Description", "Description", GameFonts.Retro, 16f);
            detailDescription = descriptionTmp;
            detailDescription.font = GameFonts.Retro;
        }

        private void CreateHeader(GameObject detailsObject)
        {
            GameObject headerSectionObject = new GameObject("HeaderSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            headerSectionObject.transform.SetParent(detailsObject.transform, false);

            VerticalLayoutGroup headerVerticalLayout = headerSectionObject.GetComponent<VerticalLayoutGroup>();
            headerVerticalLayout.spacing = 8f;
            headerVerticalLayout.childAlignment = TextAnchor.MiddleCenter;
            headerVerticalLayout.childForceExpandWidth = false;

            CreateIconObject(headerSectionObject.transform);

            TextMeshProUGUI labelTmp = UIUtility.CreateText(headerSectionObject.transform, "Label", "Label", GameFonts.Retro, 20f, TextAlignmentOptions.Center);
            detailTitle = labelTmp;

            var labelLayout = labelTmp.gameObject.AddComponent<LayoutElement>();
            labelLayout.minHeight = 32f;

            TextMeshProUGUI sourceTmp = UIUtility.CreateText(headerSectionObject.transform, "Source", "Source", GameFonts.Retro, 16f, TextAlignmentOptions.Center);
            detailSource = sourceTmp;
            detailSource.alpha = 0.75f;
        }

        private GameObject CreateIconObject(Transform parent)
        {
            GameObject achievementIconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            achievementIconObject.transform.SetParent(parent, false);
            LayoutElement iconLayout = achievementIconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 156f;
            iconLayout.preferredHeight = 152f;
            iconLayout.minWidth = 156f;
            iconLayout.minHeight = 152f;

            Image iconImage = achievementIconObject.GetComponent<Image>();
            iconImage.preserveAspect = true;

            detailIcon = iconImage;

            GameObject iconFrameObject = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            iconFrameObject.transform.SetParent(achievementIconObject.transform, false);

            RectTransform iconFrameRectTransform = iconFrameObject.GetComponent<RectTransform>();
            iconFrameRectTransform.anchorMin = Vector2.zero;
            iconFrameRectTransform.anchorMax = Vector2.one;
            iconFrameRectTransform.offsetMin = Vector2.zero;
            iconFrameRectTransform.offsetMax = Vector2.zero;

            Image iconFrameImage = iconFrameObject.GetComponent<Image>();
            iconFrameImage.type = Image.Type.Sliced;
            iconFrameImage.raycastTarget = false;

            detailIconFrame = iconFrameImage;

            return achievementIconObject;
        }

        public void SetVisible()
        {
            if (!isVisible)
            {
                isVisible = true;
                StartCoroutine(Animate(shownPos));
            }
        }

        public void Refresh(RegisteredAchievement achievement, Sprite frameSprite)
        {
            if (!isVisible)
            {
                SetVisible();
            }
            detailIcon.sprite =  achievement.IsUnlocked ? achievement.Definition.Icon : achievement.Definition.LockedIcon;
            detailIconFrame.sprite = frameSprite;
            detailTitle.text = achievement.Definition.Name;
            detailSource.text = AchievementRegistry.TryGetPluginName(achievement.PluginGuid, out var pluginName) ? pluginName : achievement.PluginGuid;
            detailDescription.text = achievement.Definition.Description;

            if (achievement.IsUnlocked)
            {
                string unlockedLocale = LocaleRegistry.Get("deatils_panel_earned", "EARNED");
                detailUnlocked.text = unlockedLocale;
                detailUnlocked.color = Color.green;

                detailUnlockedAtDate.text = achievement.UnlockedAt?.ToShortDateString();
                detailUnlockedAtTime.text = achievement.UnlockedAt?.ToShortTimeString();
            }
            else
            {
                string lockedLocale = LocaleRegistry.Get("deatils_panel_locked", "LOCKED");
                detailUnlocked.text = lockedLocale;
                detailUnlocked.color = Color.red;

                detailUnlockedAtDate.text = "-";
                detailUnlockedAtTime.text = "-";
            }

        }

        private IEnumerator Animate(Vector2 target)
        {
            yield return AnimationUtility.SlideTo(target, detailsPanelRect);
        }

        private (TextMeshProUGUI, TextMeshProUGUI) CreateStatisticRow(Transform parent, string name, string label, string value, TMP_FontAsset font)
        {
            GameObject row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            TextMeshProUGUI tmpLabel = UIUtility.CreateText(row.transform, "Label", label, font, 18f);

            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(row.transform, false);

            LayoutElement spacerLayout = spacer.GetComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1f;

            TextMeshProUGUI tmpValue = UIUtility.CreateText(row.transform, "Value", value, font, 18f);

            return (tmpLabel, tmpValue);
        }
    }
}
