using BepInEx;
using BepInEx.Bootstrap;
using CUAchievements.Definitions;
using CUAchievements.Persistence;
using CUAchievements.Registry;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal sealed class AchievementMenuManager : MonoBehaviour
    {
        //internal GameObject modalBackdrop;
        internal GameObject vanillaPanel;
        internal RectTransform vanillaPanelRect;
        internal ScrollRect scrollRect;
        internal RectTransform content;
        internal Button closeButton;
        internal Canvas modalCanvas;
        internal CanvasGroup modalCanvasGroup;
        internal GraphicRaycaster modalRaycaster;
        internal TMP_FontAsset font;
        internal bool visible;
        internal static bool IsModalOpen { get; private set; }
        internal Button achievementListTab;
        internal Button settingsTab;
        internal GameObject buttonTemplate;

        internal Dictionary<string, List<RegisteredAchievement>> registeredAchievement;

        internal void Initialize(TMP_FontAsset font)
        {
            this.font = font;
            BuildModal();
            SetVisible(false);
        }

        internal void SetVisible(bool show)
        {
            visible = show;
            IsModalOpen = show;

            if (modalCanvasGroup != null)
            {
                modalCanvasGroup.alpha = show ? 1 : 0;
                modalCanvasGroup.interactable = show;
                modalCanvasGroup.blocksRaycasts = show;
            }
            //if (modalBackdrop != null) modalBackdrop.SetActive(show);
            if (vanillaPanel != null) vanillaPanel.SetActive(show);
            if (show)
                RefreshAchievementList();
        }
        internal void BuildTabs() // Thanks Makeshift Prosthetics code
        {
            Transform panelTransform = vanillaPanel.transform.Find("Panel");
            if (panelTransform == null) return;

            GameObject headerGameObject = new GameObject("AchievementHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            headerGameObject.transform.SetParent(panelTransform, false);

            RectTransform headerRect = headerGameObject.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 48f);
            headerRect.anchoredPosition = Vector2.zero;

            GameObject achievementTabObject = new GameObject("AchievementTabs", typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));

            achievementTabObject.transform.SetParent(headerGameObject.transform, false);

            achievementTabObject.GetComponent<LayoutElement>().preferredHeight = 48f;

            HorizontalLayoutGroup horizontalLayoutGroup = achievementTabObject.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = 8f;
            horizontalLayoutGroup.padding = new RectOffset(6, 6, 3, 3);
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childForceExpandHeight = true;
            horizontalLayoutGroup.childForceExpandWidth = true;

            achievementListTab = BuildButton(horizontalLayoutGroup.transform, "AchievementListTab", "Achievements"); // TODO: Localization
            settingsTab = BuildButton(horizontalLayoutGroup.transform, "AchievementSettingsTab", "Settings"); // TODO: Localization
        }

        internal Button BuildButton(Transform parent, string name, string label)
        {
            GameObject buttonTemplate = this.buttonTemplate;
            if (buttonTemplate == null) return null;

            GameObject buttonObject = Instantiate(buttonTemplate, parent, false);
            buttonObject.name = name;
            buttonObject.SetActive(false);

            Button button = buttonObject.GetComponent<Button>();

            if (button == null) button = buttonObject.GetComponentInChildren<Button>(true);
            if (button == null)
            {
                Destroy(buttonObject);
                return null;
            }
            button.onClick = new Button.ButtonClickedEvent();
            button.interactable = true;
            TMP_Text buttomTmpText = button.GetComponentInChildren<TMP_Text>();
            if (buttomTmpText != null) buttomTmpText.text = label;

            buttonObject.SetActive(true);
            return button;
        }

        internal void BuildModal()
        {
            RectTransform selfRect = GetComponent<RectTransform>();
            selfRect.anchorMin = Vector2.zero;
            selfRect.anchorMax = Vector2.one;
            selfRect.offsetMin = Vector2.zero;
            selfRect.offsetMax = Vector2.zero;

            modalCanvas = gameObject.AddComponent<Canvas>();
            modalCanvas.overrideSorting = true;
            modalCanvas.sortingOrder = 500;

            modalRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            modalCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            //modalBackdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            //modalBackdrop.transform.SetParent(transform, false);
            //RectTransform backdropRect = modalBackdrop.GetComponent<RectTransform>();
            //backdropRect.anchorMin = Vector2.zero;
            //backdropRect.anchorMax = Vector2.one;
            //backdropRect.offsetMin = Vector2.zero;
            //backdropRect.offsetMax = Vector2.zero;
            //Image backdropImage = modalBackdrop.GetComponent<Image>();
            //backdropImage.color = new Color(0f, 0f, 0f, 0.72f);
            //backdropImage.raycastTarget = true;

            GameObject template = Resources.Load<GameObject>("Special/SettingsMenu");
            if (template == null)
            {
                CUAchievementsPlugin.Logger.LogError("SettingsMenu template not found.");
                return;
            }

            vanillaPanel = Instantiate(template, transform, false);
            vanillaPanel.name = "Modal";
            LogHierarchy(vanillaPanel.transform);

            foreach (var mb in vanillaPanel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                string ns = mb.GetType().Namespace ?? "";
                if (!ns.StartsWith("UnityEngine.UI") && !ns.StartsWith("TMPro"))
                    Destroy(mb);
            }

            vanillaPanelRect = vanillaPanel.GetComponent<RectTransform>();
            vanillaPanelRect.anchorMin = Vector2.zero;
            vanillaPanelRect.anchorMax = Vector2.one;
            vanillaPanelRect.offsetMin = Vector2.zero;
            vanillaPanelRect.offsetMax = Vector2.zero;

            var settingsDescriptionTextComponent = vanillaPanelRect.GetComponentInChildren<TextMeshProUGUI>();
            settingsDescriptionTextComponent.text = LocaleRegistry.Get("achievementsbutton", "Achievements");

            scrollRect = vanillaPanel.GetComponentInChildren<ScrollRect>(true);
            content = scrollRect != null ? scrollRect.content : null;

            if (content != null)
            {
                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    Destroy(content.GetChild(i).gameObject);
                }

                VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.spacing = 6f;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = true;

                ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            Button[] buttons = vanillaPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                b.gameObject.SetActive(b == buttons[0]);
            }
            if (buttons.Length > 0)
            {
                closeButton = buttons[0];
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => SetVisible(false));
                var closeButtonTextComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                closeButtonTextComponent.text = Locale.GetOther("back");

                buttonTemplate = closeButton.gameObject;
            }
            //Transform panelTransform = vanillaPanel.transform.Find("Panel");
            //if (panelTransform != null)
            //{
            //    panelTransform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1500f);

            //    Transform scrollTransform = panelTransform.Find("Scroll");
            //    scrollTransform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1475f);

            //    Transform buttonTransform = panelTransform.Find("Button");
            //    RectTransform buttonRectTransform = buttonTransform.GetComponent<RectTransform>();

            //    buttonRectTransform.anchoredPosition = new Vector2(-656f, buttonRectTransform.anchoredPosition.y);
            //}

            BuildTabs();
        }

        internal void RefreshAchievementList()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                if (content.GetChild(i).name == "AchievementPluginContainer")
                    Destroy(content.GetChild(i).gameObject);
            }

            if (registeredAchievement == null) registeredAchievement = AchievementRegistry.GetAchievementsPerPlugin(true);

            foreach (var pluginAchievement in registeredAchievement)
            {
                var sortedAchievements = pluginAchievement.Value.OrderBy(a => GetSortRank(a)).ToList();

                BuildAchievementRow(pluginAchievement.Key, sortedAchievements);
            }
        }

        private static int GetSortRank(RegisteredAchievement achievement)
        {
            if (achievement.Progress.Unlocked)
                return 0;

            if (achievement.Definition.IsHidden)
                return 2;

            return 1; 
        }

        internal void BuildAchievementRow(string pluginName, List<RegisteredAchievement> pluginRegisteredAchievements)
        {
            if (content == null) return;
            if (pluginRegisteredAchievements.Count() == 0) return;

            GameObject achievementPluginContainer = new GameObject("AchievementPluginContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter));
            achievementPluginContainer.transform.SetParent(content, false);

            VerticalLayoutGroup achievementsVerticalLayout = achievementPluginContainer.GetComponent<VerticalLayoutGroup>();
            achievementsVerticalLayout.spacing = 12f;
            achievementsVerticalLayout.childControlHeight = true;
            achievementsVerticalLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = achievementPluginContainer.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateAchievementRowHeader(achievementPluginContainer.transform, pluginName);
            foreach (var achievement in pluginRegisteredAchievements)
            {
                AddAchievementObject(achievementPluginContainer.transform, achievement);
            }
        }

        internal void CreateAchievementRowHeader(Transform parent, string pluginName)
        {
            GameObject buttonTemplate = this.buttonTemplate;
            if (buttonTemplate == null) return;

            GameObject headerObject = Instantiate(buttonTemplate, parent, false);
            headerObject.SetActive(false);
            headerObject.name = "PluginName";

            Button headerButton = headerObject.GetComponent<Button>();
            if (headerButton == null) headerButton = headerObject.GetComponentInChildren<Button>(true);

            headerButton.onClick = new Button.ButtonClickedEvent();
            headerButton.interactable = false;
            TMP_Text headerTmpText = headerButton.GetComponentInChildren<TMP_Text>();
            if (headerTmpText != null) headerTmpText.text = pluginName;

            Destroy(headerButton);

            headerObject.SetActive(true);
        }
        internal void AddAchievementObject(Transform parent, RegisteredAchievement achievement)
        {
            if (achievement == null) return;

            GameObject achievementObject = new GameObject("AchievementContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(CanvasGroup)); 
            achievementObject.transform.SetParent(parent, false);

            LayoutElement layoutElement = achievementObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 140f;

            HorizontalLayoutGroup achievementHorizontalLayout = achievementObject.GetComponent<HorizontalLayoutGroup>();
            achievementHorizontalLayout.padding = new RectOffset(25, 25, 5, 5);
            achievementHorizontalLayout.spacing = 10f;
            achievementHorizontalLayout.childControlWidth = true;
            achievementHorizontalLayout.childControlHeight = false;
            achievementHorizontalLayout.childForceExpandWidth = false;
            achievementHorizontalLayout.childForceExpandHeight = false;
            achievementHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;

            GameObject bgObject = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(LayoutIgnorer));
            bgObject.transform.SetParent(achievementObject.transform, false);

            RectTransform bgRect = bgObject.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgObject.GetComponent<Image>();
            bgImage.color = Color.black;
            bgImage.raycastTarget = false;

            GameObject frameObject = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(LayoutIgnorer));
            frameObject.transform.SetParent(achievementObject.transform, false);

            RectTransform frameRectTransform = frameObject.GetComponent<RectTransform>();
            frameRectTransform.anchorMin = Vector2.zero;
            frameRectTransform.anchorMax = Vector2.one;
            frameRectTransform.offsetMin = Vector2.zero;
            frameRectTransform.offsetMax = Vector2.zero;

            Image frameImage = frameObject.GetComponent<Image>();
            frameImage.sprite = AssetLoader.GetCachedSprite("achievement_frame_common");
            frameImage.type = Image.Type.Sliced;
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;

            GameObject achievementIconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            achievementIconObject.transform.SetParent(achievementObject.transform, false);
            LayoutElement iconLayout = achievementIconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 96f;
            iconLayout.preferredHeight = 96f;
            iconLayout.minWidth = 96f;
            iconLayout.minHeight = 96f;

            Image iconImage = achievementIconObject.GetComponent<Image>();
            iconImage.sprite = achievement.Progress.Unlocked ? achievement.Definition.Sprite : AssetLoader.GetCachedSprite("achievement_default_locked");
            iconImage.preserveAspect = true;

            GameObject iconFrameObject = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            iconFrameObject.transform.SetParent(achievementIconObject.transform, false);
            RectTransform iconFrameRectTransform = iconFrameObject.GetComponent<RectTransform>();
            iconFrameRectTransform.anchorMin = Vector2.zero;
            iconFrameRectTransform.anchorMax = Vector2.one;
            iconFrameRectTransform.offsetMin = Vector2.zero;
            iconFrameRectTransform.offsetMax = Vector2.zero;

            Image iconFrameImage = iconFrameObject.GetComponent<Image>();
            iconFrameImage.sprite = AssetLoader.GetCachedSprite("achievement_frame_image");
            iconFrameImage.type = Image.Type.Simple;
            iconFrameImage.raycastTarget = false;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textObject.transform.SetParent(achievementObject.transform, false);
            LayoutElement textLayout = textObject.GetComponent<LayoutElement>();
            textLayout.flexibleWidth = 1f;

            VerticalLayoutGroup textVerticalLayout = textObject.GetComponent<VerticalLayoutGroup>();
            textVerticalLayout.childControlWidth = true;
            textVerticalLayout.childControlHeight = true;
            textVerticalLayout.childForceExpandWidth = true;
            textVerticalLayout.spacing = 4f;

            var titleTmp = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            titleTmp.transform.SetParent(textObject.transform, false);
            titleTmp.text = achievement.Definition.Title;
            titleTmp.fontSize = 24;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.font = this.font;
            titleTmp.enableWordWrapping = true;
            titleTmp.overflowMode = TextOverflowModes.Ellipsis;
            titleTmp.maxVisibleLines = 1;

            var descObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            descObj.transform.SetParent(textObject.transform, false);

            var descTmp = descObj.GetComponent<TextMeshProUGUI>();
            descTmp.transform.SetParent(textObject.transform, false);
            descTmp.text = achievement.Definition.Description;
            descTmp.fontSize = 18;
            descTmp.color = new Color(1f, 1f, 1f, 0.7f);
            descTmp.font = this.font;
            descTmp.enableWordWrapping = true;
            descTmp.overflowMode = TextOverflowModes.Ellipsis;
            descTmp.maxVisibleLines = 2;

            LayoutElement descLayout = descObj.GetComponent<LayoutElement>();
            descLayout.preferredHeight = 45f;
            descLayout.flexibleHeight = 0f;

            CanvasGroup canvasGroup = achievementObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = achievement.Progress.Unlocked ? 1f : 0.35f;
        }

        private static void LogHierarchy(Transform root, int depth = 0)
        {
            CUAchievementsPlugin.Logger.LogInfo(new string(' ', depth * 2) + root.name);
            foreach (Transform child in root)
            {
                LogHierarchy(child, depth + 1);
            }
        }
    }
}
