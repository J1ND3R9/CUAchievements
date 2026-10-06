using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUAchievements.UI.Menu;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.SidePanels
{
    internal sealed class DetailsPanel : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        public Transform Panel { get; private set; }
        //public event Action Opened;
        //public event Action Closed;

        private const float PanelWidth = 380f;
        private const float PanelHeight = 800f;
        private const float ScrollWidth = PanelWidth - 10f;
        private const float ScrollHeight = PanelHeight - 100f;

        private Vector2 HiddenPos = new(-600f, 0f);
        private Vector2 ShownPos = new(20f, 0f);

        private Coroutine slideAnim;

        private Image icon;
        private Image frame;

        private TextMeshProUGUI aTitle;
        private TextMeshProUGUI aDescription;
        private TextMeshProUGUI aModSource;
        private TextMeshProUGUI aUnlocked;
        private TextMeshProUGUI aUnlockedAtDate;
        private TextMeshProUGUI aUnlockedAtTime;

        //private string currentShowedId = string.Empty;

        public static DetailsPanel Create(Transform parent, Transform panel)
        {
            GameObject detailsPanel = Instantiate(panel, parent).gameObject;
            detailsPanel.name = "Details";

            DetailsPanel panelComponent = detailsPanel.AddComponent<DetailsPanel>();

            panelComponent.Initialize();
            return panelComponent;
        }

        private void Initialize()
        {
            Root = transform.gameObject;

            var scroll = Root.GetComponentInChildren<ScrollRect>(true);

            Content = scroll?.content;
            if (Content != null)
            {
                Root.transform.ClearPanel();
                var vertical = Content.SetupScrollContent(spacing: 16f);
                vertical.name = "Details";
            }

            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(ScrollWidth, ScrollHeight);

            var description = transform.GetComponentInChildren<TextMeshProUGUI>();
            description.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollWidth, 50f);
            description.text = LocaleRegistry.Get("achievements_details_panel_title", "Details");

            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            rect.anchoredPosition = HiddenPos;

            CreateHeader(parent: Content);
            UIUtility.CreateSeparator(parent: Content);

            aDescription = UIUtility.CreateText(Content, "Description", "Description", GameFonts.Retro, 16f);
            UIUtility.CreateSeparator(parent: Content);

            CreateBottom(parent: Content);
        }

        private void CreateHeader(Transform parent)
        {
            GameObject header = UIUtility.Create("Header", parent, typeof(VerticalLayoutGroup));
            header.GetComponent<VerticalLayoutGroup>().Configure(spacing: 8f, align: TextAnchor.MiddleCenter);

            GameObject icon = UIUtility.Create("Icon", header.transform, typeof(Image), typeof(LayoutElement));

            LayoutElement iconLayout = icon.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = iconLayout.minWidth = 152f;
            iconLayout.preferredHeight = iconLayout.minHeight = 152f;

            this.icon = icon.GetComponent<Image>();

            GameObject frame = UIUtility.Create("Frame", icon.transform, typeof(Image));
            frame.transform.Stretch();

            Image frameImage = frame.GetComponent<Image>();
            frameImage.type = Image.Type.Sliced;

            this.frame = frameImage;

            aTitle = UIUtility.CreateText(header.transform, "Title", "Title", GameFonts.Retro, 20f, TextAlignmentOptions.Center);
            aTitle.gameObject.AddComponent<LayoutElement>().minHeight = 32f;

            aModSource = UIUtility.CreateText(header.transform, "Source", "Source", GameFonts.Retro, 16f, TextAlignmentOptions.Center);
            aModSource.alpha = 0.75f;
        }

        private void CreateBottom(Transform parent)
        {
            GameObject bottom = UIUtility.Create("Bottom", parent, typeof(VerticalLayoutGroup));
            aUnlocked = UIUtility.CreateText(bottom.transform, "Unlocked", "Unlocked", GameFonts.Retro, 18f);
            (aUnlockedAtDate, aUnlockedAtTime) = UIUtility.CreateStatisticRow(bottom.transform, "UnlockedAt", "-", "-");
        }

        private void Refresh(AchievementCardModel card)
        {
            if (card == null) return;

            icon.sprite = card.Icon;
            frame.sprite = card.Frame;

            aTitle.text = card.Title;
            aDescription.text = card.Description;
            aModSource.text = AchievementRegistry.TryGetPluginName(card.Source.PluginGuid, out var pluginName) ? pluginName : card.Source.PluginGuid;
            
            if (card.Source.IsUnlocked)
            {
                string unlockedLocale = LocaleRegistry.Get("achievements_details_earned", "EARNED");
                aUnlocked.text = unlockedLocale;
                aUnlocked.color = Color.green;

                aUnlockedAtDate.text = card.Source.UnlockedAt?.ToShortDateString();
                aUnlockedAtTime.text = card.Source.UnlockedAt?.ToShortTimeString();
            }
            else
            {
                string lockedLocale = LocaleRegistry.Get("achievements_details_locked", "LOCKED");
                aUnlocked.text = lockedLocale;
                aUnlocked.color = Color.red;

                aUnlockedAtDate.text = "";
                aUnlockedAtTime.text = "";
            }
        }

        private void SetVisible(bool show, RegisteredAchievement achievement = null)
        {
            IsOpen = show;
            if (show)
                OnOpen(achievement);
            else
                OnClose();
        }
        public void Open(RegisteredAchievement achievement) => SetVisible(true, achievement);
        public void Close() => SetVisible(false);
        private void OnOpen(RegisteredAchievement achievement)
        {
            //if (currentShowedId == achievement.FullId)
            //{
            //    Close();
            //    currentShowedId = string.Empty;
            //    return;
            //}
            //currentShowedId = achievement.FullId;
            Refresh(AchievementCardModel.From(achievement));
            PlayAnim(ShownPos);
        }

        private void OnClose() => PlayAnim(HiddenPos);

        private void PlayAnim(Vector2 target)
        {
            if (slideAnim != null) StopCoroutine(slideAnim);
            slideAnim = StartCoroutine(Animate(target, (RectTransform)Root.transform));
        }
        private IEnumerator Animate(Vector2 target, RectTransform rect)
        {
            yield return AnimationUtility.SlideTo(target, rect);
            slideAnim = null;
        }
        //private RectTransform detailsPanelRect;
        //private Image detailIcon;
        //private Image detailIconFrame;
        //private TextMeshProUGUI detailTitle;
        //private TextMeshProUGUI detailDescription;
        //private TextMeshProUGUI detailSource;
        //private TextMeshProUGUI detailUnlocked;
        //private TextMeshProUGUI detailUnlockedAtDate;
        //private TextMeshProUGUI detailUnlockedAtTime;
        //private bool isVisible = false;
        //private Vector2 hiddenPos = new(-600f, 0f);
        //private Vector2 shownPos = new(20f, 0f);

        //public static DetailsPanel Create(Transform parent)
        //{
        //    Transform panelTransform = parent.Find("Panel");
        //    if (panelTransform == null) return null;

        //    GameObject detailsPanel = Instantiate(panelTransform, parent).gameObject;
        //    detailsPanel.name = "DetailsPanel";

        //    DetailsPanel panelComponent = detailsPanel.AddComponent<DetailsPanel>();
        //    panelComponent.Initialize();

        //    return panelComponent;
        //}

        //private void Initialize()
        //{
        //    foreach (Transform child in transform)
        //    {
        //        if (child.name == "Scroll")
        //        {
        //            var content = child.Find("Content");
        //            if (content != null)
        //            {
        //                foreach (Transform child2 in content.transform)
        //                    Destroy(child2.gameObject);
        //            }
        //            continue;
        //        }
        //        if (child.name != "Description")
        //            Destroy(child.gameObject);
        //    }

        //    GameObject detailsScroll = transform.Find("Scroll").gameObject;

        //    RectTransform descriptionRect = transform.Find("Description").GetComponent<RectTransform>();
        //    descriptionRect.sizeDelta = new Vector2(370f, 50f);

        //    RectTransform detailsScrollRect = detailsScroll.GetComponent<RectTransform>();
        //    detailsScrollRect.sizeDelta = new Vector2(370f, 700f);

        //    TextMeshProUGUI descriptionTemplateTmp = GetComponentInChildren<TextMeshProUGUI>();
        //    if (descriptionTemplateTmp != null)
        //    {
        //        string detailsLocale = LocaleRegistry.Get("detailspanel_details", "Achievement details");
        //        descriptionTemplateTmp.text = detailsLocale;
        //    }

        //    RectTransform detailsRect = GetComponent<RectTransform>();
        //    detailsRect.anchorMax = new Vector2(0f, 0.5f);
        //    detailsRect.anchorMin = new Vector2(0f, 0.5f);
        //    detailsRect.pivot = new Vector2(0f, 0.5f);
        //    detailsRect.sizeDelta = new Vector2(380f, 800f);
        //    detailsRect.anchoredPosition = hiddenPos;

        //    detailsPanelRect = detailsRect;

        //    Transform contentTransform = detailsScroll.transform.Find("Viewport/Content");

        //    GameObject detailsObject = new GameObject("Details", typeof(RectTransform), typeof(VerticalLayoutGroup));
        //    detailsObject.transform.SetParent(contentTransform, false);

        //    VerticalLayoutGroup detailsVerticalLayout = detailsObject.GetComponent<VerticalLayoutGroup>();
        //    detailsVerticalLayout.spacing = 16f;
        //    detailsVerticalLayout.childAlignment = TextAnchor.UpperCenter;

        //    CreateHeader(detailsObject);

        //    UIUtility.CreateSeparator(detailsObject.transform);

        //    CreateDescription(detailsObject);

        //    UIUtility.CreateSeparator(detailsObject.transform);

        //    GameObject bottomSectionObject = new("BottomSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
        //    bottomSectionObject.transform.SetParent(detailsObject.transform, false);

        //    TextMeshProUGUI unlockedTmp = UIUtility.CreateText(bottomSectionObject.transform, "Unlocked", "Unlocked", GameFonts.Retro, 18f);
        //    detailUnlocked = unlockedTmp;
        //    detailUnlocked.font = GameFonts.Retro;

        //    (TextMeshProUGUI unlockedDate, TextMeshProUGUI unlockedTime) = CreateStatisticRow(bottomSectionObject.transform, "UnlockedAt", "-", "-", GameFonts.Retro);
        //    detailUnlockedAtDate = unlockedDate;
        //    detailUnlockedAtTime = unlockedTime;
        //}

        //private void CreateDescription(GameObject detailsObject)
        //{
        //    GameObject descriptionSectionObject = new GameObject("DescriptionSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
        //    descriptionSectionObject.transform.SetParent(detailsObject.transform, false);

        //    TextMeshProUGUI descriptionTmp = UIUtility.CreateText(descriptionSectionObject.transform, "Description", "Description", GameFonts.Retro, 16f);
        //    detailDescription = descriptionTmp;
        //    detailDescription.font = GameFonts.Retro;
        //}

        //private void CreateHeader(GameObject detailsObject)
        //{
        //    GameObject headerSectionObject = new GameObject("HeaderSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
        //    headerSectionObject.transform.SetParent(detailsObject.transform, false);

        //    VerticalLayoutGroup headerVerticalLayout = headerSectionObject.GetComponent<VerticalLayoutGroup>();
        //    headerVerticalLayout.spacing = 8f;
        //    headerVerticalLayout.childAlignment = TextAnchor.MiddleCenter;
        //    headerVerticalLayout.childForceExpandWidth = false;

        //    CreateIconObject(headerSectionObject.transform);

        //    TextMeshProUGUI labelTmp = UIUtility.CreateText(headerSectionObject.transform, "Label", "Label", GameFonts.Retro, 20f, TextAlignmentOptions.Center);
        //    detailTitle = labelTmp;

        //    var labelLayout = labelTmp.gameObject.AddComponent<LayoutElement>();
        //    labelLayout.minHeight = 32f;

        //    TextMeshProUGUI sourceTmp = UIUtility.CreateText(headerSectionObject.transform, "Source", "Source", GameFonts.Retro, 16f, TextAlignmentOptions.Center);
        //    detailSource = sourceTmp;
        //    detailSource.alpha = 0.75f;
        //}

        //private GameObject CreateIconObject(Transform parent)
        //{
        //    GameObject achievementIconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        //    achievementIconObject.transform.SetParent(parent, false);
        //    LayoutElement iconLayout = achievementIconObject.GetComponent<LayoutElement>();
        //    iconLayout.preferredWidth = 156f;
        //    iconLayout.preferredHeight = 152f;
        //    iconLayout.minWidth = 156f;
        //    iconLayout.minHeight = 152f;

        //    Image iconImage = achievementIconObject.GetComponent<Image>();
        //    iconImage.preserveAspect = true;

        //    detailIcon = iconImage;

        //    GameObject iconFrameObject = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
        //    iconFrameObject.transform.SetParent(achievementIconObject.transform, false);

        //    RectTransform iconFrameRectTransform = iconFrameObject.GetComponent<RectTransform>();
        //    iconFrameRectTransform.anchorMin = Vector2.zero;
        //    iconFrameRectTransform.anchorMax = Vector2.one;
        //    iconFrameRectTransform.offsetMin = Vector2.zero;
        //    iconFrameRectTransform.offsetMax = Vector2.zero;

        //    Image iconFrameImage = iconFrameObject.GetComponent<Image>();
        //    iconFrameImage.type = Image.Type.Sliced;
        //    iconFrameImage.raycastTarget = false;

        //    detailIconFrame = iconFrameImage;

        //    return achievementIconObject;
        //}

        //public void SetVisible()
        //{
        //    if (!isVisible)
        //    {
        //        isVisible = true;
        //        StartCoroutine(Animate(shownPos));
        //    }
        //}

        //public void Refresh(RegisteredAchievement achievement, Sprite frameSprite)
        //{
        //    if (!isVisible)
        //    {
        //        SetVisible();
        //    }
        //    detailIcon.sprite =  achievement.IsUnlocked ? achievement.Definition.Icon : achievement.Definition.LockedIcon;
        //    detailIconFrame.sprite = frameSprite;
        //    detailTitle.text = achievement.Definition.Name;
        //    detailSource.text = AchievementRegistry.TryGetPluginName(achievement.PluginGuid, out var pluginName) ? pluginName : achievement.PluginGuid;
        //    detailDescription.text = achievement.Definition.Description;

        //    if (achievement.IsUnlocked)
        //    {
        //        string unlockedLocale = LocaleRegistry.Get("deatils_panel_earned", "EARNED");
        //        detailUnlocked.text = unlockedLocale;
        //        detailUnlocked.color = Color.green;

        //        detailUnlockedAtDate.text = achievement.UnlockedAt?.ToShortDateString();
        //        detailUnlockedAtTime.text = achievement.UnlockedAt?.ToShortTimeString();
        //    }
        //    else
        //    {
        //        string lockedLocale = LocaleRegistry.Get("deatils_panel_locked", "LOCKED");
        //        detailUnlocked.text = lockedLocale;
        //        detailUnlocked.color = Color.red;

        //        detailUnlockedAtDate.text = "-";
        //        detailUnlockedAtTime.text = "-";
        //    }

        //}

        //private IEnumerator Animate(Vector2 target)
        //{
        //    yield return AnimationUtility.SlideTo(target, detailsPanelRect);
        //}

        //private (TextMeshProUGUI, TextMeshProUGUI) CreateStatisticRow(Transform parent, string name, string label, string value, TMP_FontAsset font)
        //{
        //    GameObject row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        //    row.transform.SetParent(parent, false);

        //    HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        //    layout.childControlHeight = true;
        //    layout.childControlWidth = true;
        //    layout.childForceExpandHeight = true;
        //    layout.childForceExpandWidth = false;

        //    TextMeshProUGUI tmpLabel = UIUtility.CreateText(row.transform, "Label", label, font, 18f);

        //    GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        //    spacer.transform.SetParent(row.transform, false);

        //    LayoutElement spacerLayout = spacer.GetComponent<LayoutElement>();
        //    spacerLayout.flexibleWidth = 1f;

        //    TextMeshProUGUI tmpValue = UIUtility.CreateText(row.transform, "Value", value, font, 18f);

        //    return (tmpLabel, tmpValue);
        //}
    }
}
