using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
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
        private Vector2 hiddenPos = new Vector2(-600f, 0f);
        private Vector2 shownPos = new Vector2(20f, 0f);
        private TMP_FontAsset fontAsset;

        public static AchievementDetailsPanel Create(Transform parent, TMP_FontAsset font)
        {
            Transform panelTransform = parent.Find("Panel");
            if (panelTransform == null) return null;

            GameObject detailsPanel = Instantiate(panelTransform, parent).gameObject;
            detailsPanel.name = "DetailsPanel";

            AchievementDetailsPanel panelComponent = detailsPanel.AddComponent<AchievementDetailsPanel>();
            panelComponent.fontAsset = font;
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

            TextMeshProUGUI descriptionTmp = GetComponentInChildren<TextMeshProUGUI>();
            if (descriptionTmp != null) descriptionTmp.text = "Детали достижения"; // TODO: Localization

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

            GameObject headerSectionObject = new GameObject("HeaderSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            headerSectionObject.transform.SetParent(detailsObject.transform, false);
            
            VerticalLayoutGroup headerVerticalLayout = headerSectionObject.GetComponent<VerticalLayoutGroup>();
            headerVerticalLayout.spacing = 8f;
            headerVerticalLayout.childAlignment = TextAnchor.MiddleCenter;

            //headerVerticalLayout.childControlWidth = false;
            //headerVerticalLayout.childControlHeight = false;

            headerVerticalLayout.childForceExpandWidth = false;
            //headerVerticalLayout.childForceExpandHeight = false;

            CreateIconObject(headerSectionObject.transform);

            detailTitle = UIUtility.CreateText(headerSectionObject.transform, "Label", "Label", fontAsset, 20f, TextAlignmentOptions.Center);
            detailTitle.font = fontAsset;

            detailSource = UIUtility.CreateText(headerSectionObject.transform, "Source", "Source", fontAsset, 16f, TextAlignmentOptions.Center);
            detailSource.font = fontAsset;
            detailSource.alpha = 0.75f;

            UIUtility.CreateSeparator(detailsObject.transform);

            GameObject descriptionSectionObject = new GameObject("DescriptionSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            descriptionSectionObject.transform.SetParent(detailsObject.transform, false);

            detailDescription = UIUtility.CreateText(descriptionSectionObject.transform, "Description", "Description", fontAsset, 16f); // TODO: Localization
            detailDescription.font = fontAsset;

            UIUtility.CreateSeparator(detailsObject.transform);

            GameObject bottomSectionObject = new GameObject("BottomSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            bottomSectionObject.transform.SetParent(detailsObject.transform, false);

            detailUnlocked = UIUtility.CreateText(bottomSectionObject.transform, "Unlocked", "Unlocked", fontAsset, 18f);
            detailUnlocked.font = fontAsset;

            (Transform _, TextMeshProUGUI unlockedDate, TextMeshProUGUI unlockedTime) = CreateStatisticRow(bottomSectionObject.transform, "UnlockedAt", "-", "-", fontAsset);
            detailUnlockedAtDate = unlockedDate;
            detailUnlockedAtTime = unlockedTime;
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
            detailTitle.text = achievement.Definition.Title;
            detailSource.text = AchievementRegistry.TryGetPluginName(achievement.PluginGuid, out var pluginName) ? pluginName : achievement.PluginGuid;
            detailDescription.text = achievement.Definition.Description;

            if (achievement.IsUnlocked)
            {
                detailUnlocked.text = "ПОЛУЧЕНО"; // TODO: Localization
                detailUnlocked.color = Color.green;

                detailUnlockedAtDate.text = achievement.UnlockedAt?.ToShortDateString();
                detailUnlockedAtTime.text = achievement.UnlockedAt?.ToShortTimeString();
            }
            else
            {
                detailUnlocked.text = "ЗАБЛОКИРОВАНО"; // TODO: Localization
                detailUnlocked.color = Color.red;

                detailUnlockedAtDate.text = "-";
                detailUnlockedAtTime.text = "-";
            }

        }

        private IEnumerator Animate(Vector2 target)
        {
            yield return AnimationUtility.SlideTo(target, detailsPanelRect);
        }

        private (Transform, TextMeshProUGUI, TextMeshProUGUI) CreateStatisticRow(Transform parent, string name, string label, string value, TMP_FontAsset font)
        {
            GameObject row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            var tmpLabel = UIUtility.CreateText(row.transform, "Label", label, font, 18f);

            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(row.transform, false);

            LayoutElement spacerLayout = spacer.GetComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1f;

            var tmpValue = UIUtility.CreateText(row.transform, "Value", value, font, 18f);

            return (row.transform, tmpLabel, tmpValue);
        }
    }
}
