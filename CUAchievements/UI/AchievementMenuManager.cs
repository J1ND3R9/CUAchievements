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
        private GameObject vanillaPanel;
        private RectTransform vanillaPanelRect;
        private ScrollRect scrollRect;
        private RectTransform content;
        private Button closeButton;
        private Canvas modalCanvas;
        private CanvasGroup modalCanvasGroup;
        private GraphicRaycaster modalRaycaster;
        private TMP_FontAsset font;
        private bool visible;
        public static bool IsModalOpen { get; private set; }
        private TMP_InputField searchField;
        private Button statisticTab;
        private GameObject buttonTemplate;
        private IReadOnlyDictionary<string, List<RegisteredAchievement>> registeredAchievementsPerPlugin;
        private AchievementStatsPanel statsPanel;
        private bool needsRefresh = true;

        internal void Initialize(TMP_FontAsset font)
        {
            this.font = font;
            BuildModal();
            SetVisible(false);

            AchievementEvents.AchievementsChanged += OnAchievementsChanged;
        }

        internal void Update()
        {
            if (visible)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    SetVisible(false);
            }
        }

        private void OnAchievementsChanged()
        {
            needsRefresh = true;
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
            vanillaPanel?.SetActive(show);
            if (show && needsRefresh)
            {
                RefreshAchievementList();
                needsRefresh = false;
            }
        }
        private void BuildTabs() // Thanks Makeshift Prosthetics code
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
            //horizontalLayoutGroup.spacing = 8f;
            horizontalLayoutGroup.padding = new RectOffset(6, 6, 3, 3);
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childForceExpandHeight = true;
            horizontalLayoutGroup.childForceExpandWidth = true;

            searchField = BuildInputField(achievementTabObject.transform, "AchievementSearchField", "Поиск..."); // TODO: Localization
            statisticTab = BuildButton(achievementTabObject.transform, "AchievementStatsButton", "Статистика"); // TODO: Localization

            searchField.onValueChanged.AddListener(OnSearchTextChanged);
            statisticTab.onClick.AddListener(OnStatisticTabClick);
        }

        private void OnSearchTextChanged(string newText)
        {
            RefreshAchievementList(newText);
        }

        private void OnStatisticTabClick()
        {
            statsPanel.SetVisible();
        }

        private GameObject BuildProgressObject(Transform parent, int totalAchievements, int unlockedCount, string additional = "")
        {
            GameObject progressObject = new GameObject("Progress", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Outline));
            progressObject.transform.SetParent(parent, false);

            Outline progressOutline = progressObject.GetComponent<Outline>();
            progressOutline.effectColor = Color.white;
            progressOutline.effectDistance = new Vector2(3f, 3f);

            Image progressImage = progressObject.GetComponent<Image>();
            progressImage.color = Color.black;

            LayoutElement progressLayout = progressObject.GetComponent<LayoutElement>();
            progressLayout.preferredHeight = 24f;

            GameObject divisionObject = new GameObject("DivisionGrid", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            divisionObject.transform.SetParent(progressObject.transform, false);

            RectTransform divisionRect = divisionObject.GetComponent<RectTransform>();
            divisionRect.anchorMin = Vector2.zero;
            divisionRect.anchorMax = Vector2.one;
            divisionRect.offsetMin = Vector2.zero;
            divisionRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup divisionHorizontalLayout = divisionObject.GetComponent<HorizontalLayoutGroup>();
            divisionHorizontalLayout.childControlWidth = true;
            divisionHorizontalLayout.childControlHeight = true;
            divisionHorizontalLayout.childForceExpandWidth = true;
            divisionHorizontalLayout.childForceExpandHeight = true;

            int divisionsCount = 16;

            for (int i = 0; i < divisionsCount; i++)
            {
                GameObject cell = new GameObject($"Cell{i}", typeof(RectTransform));
                cell.transform.SetParent(divisionObject.transform, false);

                if (i < divisionsCount - 1)
                {
                    GameObject line = new GameObject("Line", typeof(RectTransform), typeof(Image));
                    line.transform.SetParent(cell.transform, false);

                    RectTransform lineRect = line.GetComponent<RectTransform>();
                    lineRect.anchorMin = new Vector2(1f, 0f);
                    lineRect.anchorMax = new Vector2(1f, 1f);
                    lineRect.pivot = new Vector2(1f, 0.5f);

                    lineRect.sizeDelta = new Vector2(3f, 0f);
                    lineRect.offsetMin = new Vector2(lineRect.offsetMin.x, 0f);
                    lineRect.offsetMax = new Vector2(lineRect.offsetMax.x, 0f);

                    Image lineImage = line.GetComponent<Image>();
                    lineImage.color = Color.white;
                    lineImage.raycastTarget = false;
                }
            }

            int unlockPrecentage = totalAchievements > 0 ? (int)Math.Round((double)unlockedCount / totalAchievements * 100) : 0;

            GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(progressObject.transform, false);

            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(unlockPrecentage / 100f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObject.GetComponent<Image>();
            fillImage.color = Color.white;
            fillImage.raycastTarget = false;

            if (unlockPrecentage > 0)
            {
                GameObject textObj = new GameObject("PrecentageText", typeof(RectTransform), typeof(TextMeshProUGUI));

                var textTmp = textObj.GetComponent<TextMeshProUGUI>();
                textTmp.text = $"{additional} {unlockPrecentage}%";
                textTmp.fontSize = 18;
                textTmp.color = Color.black;
                textTmp.font = this.font;

                RectTransform textRect = textObj.GetComponent<RectTransform>();

                if (unlockPrecentage < 50)
                {
                    textObj.transform.SetParent(fillObject.transform, false);
                    textTmp.alignment = TextAlignmentOptions.MidlineRight;
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = new Vector2(-10f, 0f);
                }
                else
                {
                    textObj.transform.SetParent(progressObject.transform, false);
                    textTmp.alignment = TextAlignmentOptions.Midline;
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                }
            }
            return progressObject;
        }
        private Button BuildButton(Transform parent, string name, string label)
        {
            GameObject buttonTemplate = this.buttonTemplate;
            if (buttonTemplate == null) return null;

            GameObject buttonObject = Instantiate(buttonTemplate, parent, false);
            buttonObject.name = name;
            buttonObject.SetActive(false);

            Button button = buttonObject.GetComponent<Button>() ?? buttonObject.GetComponentInChildren<Button>(true);
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

        private TMP_InputField BuildInputField(Transform parent, string name, string placeholderText)
        {
            GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObject.transform.SetParent(parent.transform, false);

            Image inputImage = inputObject.GetComponent<Image>();
            inputImage.color = Color.black;

            Outline outline = inputObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3f, 3f);

            GameObject viewportObject = new GameObject("TextViewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObject.transform.SetParent(inputObject.transform, false);

            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8f, 4f);
            viewportRect.offsetMax = new Vector2(-8f, -4f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(viewportObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI textTmp = textObject.GetComponent<TextMeshProUGUI>();
            textTmp.fontSize = 16;
            textTmp.color = Color.white;
            textTmp.font = this.font;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            textTmp.enableWordWrapping = false;
            textTmp.raycastTarget = false;

            GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderObject.transform.SetParent(viewportObject.transform, false);

            RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            TextMeshProUGUI placeholderTmp = placeholderObject.GetComponent<TextMeshProUGUI>();
            placeholderTmp.text = placeholderText;
            placeholderTmp.fontSize = 16;
            placeholderTmp.color = new Color(1f, 1f, 1f, 0.5f);
            placeholderTmp.font = this.font;
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.raycastTarget = false;

            TMP_InputField inputField = inputObject.GetComponent<TMP_InputField>();
            inputField.textViewport = viewportRect;
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;
            inputField.fontAsset = this.font;
            inputField.pointSize = 16;

            return inputField;
        }

        private void BuildModal()
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

            GameObject template = Resources.Load<GameObject>("Special/SettingsMenu");
            if (template == null)
            {
                CUAchievementsPlugin.Logger.LogError("SettingsMenu template not found.");
                return;
            }

            vanillaPanel = Instantiate(template, transform, false);
            vanillaPanel.name = "Modal";

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
            content = scrollRect?.content;

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

            BuildTabs();
            statsPanel = AchievementStatsPanel.Create(vanillaPanel.transform, font);
            BuildInfoPanel();
        }

        private void BuildInfoPanel()
        {
            Transform panelTransform = vanillaPanel.transform.Find("Panel");
            if (panelTransform == null) return;

            GameObject infoPanel = Instantiate(panelTransform, vanillaPanel.transform).gameObject;
            infoPanel.name = "InfoPanel";

            foreach (Transform child in infoPanel.transform)
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

            GameObject infoScroll = infoPanel.transform.Find("Scroll").gameObject;
            RectTransform infoScrollRect = infoScroll.GetComponent<RectTransform>();
            infoScrollRect.sizeDelta = new Vector2(370f, 700f);

            TextMeshProUGUI description = infoPanel.GetComponentInChildren<TextMeshProUGUI>();
            description.text = "achievementName"; // TODO: Localization

            RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
            infoRect.anchorMax = new Vector2(0f, 0.5f);
            infoRect.anchorMin = new Vector2(0f, 0.5f);
            infoRect.pivot = new Vector2(0f, 0.5f);
            infoRect.sizeDelta = new Vector2(380f, 800f);
            infoRect.anchoredPosition = new Vector2(20f, 0f);
        }

        private void RefreshAchievementList(string searchFilter = "")
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                if (content.GetChild(i).name == "AchievementPluginContainer")
                    Destroy(content.GetChild(i).gameObject);
            }

            registeredAchievementsPerPlugin = AchievementRegistry.GetAchievementsPerPlugin(true, searchFilter);

            foreach (var pluginAchievement in registeredAchievementsPerPlugin)
            {
                var achievements = pluginAchievement.Value;

                var unlocked = achievements
                    .Where(a => a.Progress.Unlocked)
                    .OrderByDescending(a => a.Progress.UnlockedAt)
                    .ThenBy(a => (int)a.Definition.Difficulty)
                    .ToList();

                var lockedVisible = achievements
                    .Where(a => !a.Progress.Unlocked && !a.Definition.IsHidden)
                    .OrderBy(a => (int)a.Definition.Difficulty)
                    .ThenBy(a => a.Definition.Title)
                    .ToList();

                var hidden = achievements
                    .Where(a => !a.Progress.Unlocked && a.Definition.IsHidden)
                    .OrderBy(a => (int)a.Definition.Difficulty)
                    .ThenBy(a => a.Definition.Title)
                    .ToList();

                var sortedAchievements = unlocked
                    .Concat(lockedVisible)
                    .Concat(hidden)
                    .ToList();

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

        private void BuildAchievementRow(string pluginName, List<RegisteredAchievement> pluginRegisteredAchievements)
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

            CreateAchievementRowHeader(achievementPluginContainer.transform, pluginName, pluginRegisteredAchievements);

            List<RegisteredAchievement> hiddenAchievements = new List<RegisteredAchievement>();
            foreach (var achievement in pluginRegisteredAchievements)
            {
                if (!achievement.IsUnlocked && achievement.Definition.IsHidden)
                {
                    hiddenAchievements.Add(achievement);
                    continue;
                }
                CreateAchievementObject(achievementPluginContainer.transform, achievement);
            }
            if (hiddenAchievements.Count > 0)
            {
                CreateHiddenAchievementObject(achievementPluginContainer.transform, hiddenAchievements);
            }
        }

        private void CreateAchievementRowHeader(Transform parent, string pluginName, List<RegisteredAchievement> pluginRegisteredAchievements)
        {
            GameObject buttonTemplate = this.buttonTemplate;
            if (buttonTemplate == null) return;

            int totalAchievements = pluginRegisteredAchievements.Count;
            int unlockedCount = pluginRegisteredAchievements.Where(a => a.IsUnlocked).Count();

            GameObject headerObject = Instantiate(buttonTemplate, parent, false);
            headerObject.SetActive(false);
            headerObject.name = "PluginInfoHeader";

            LayoutElement headerLayoutElement = headerObject.AddComponent<LayoutElement>();
            headerLayoutElement.preferredHeight = 75f;
            headerLayoutElement.minHeight = 75f;

            Button headerButton = headerObject.GetComponent<Button>() ?? headerObject.GetComponentInChildren<Button>(true);
            headerButton.onClick = new Button.ButtonClickedEvent();
            headerButton.interactable = false;

            TMP_Text headerTmpText = headerButton.GetComponentInChildren<TMP_Text>();
            if (headerTmpText != null)
            {
                headerTmpText.text = $"{pluginName} [{unlockedCount}/{totalAchievements}]";
                headerTmpText.fontSize = 12;
            }

            Destroy(headerButton);

            VerticalLayoutGroup headerVerticalLayout = headerObject.AddComponent<VerticalLayoutGroup>();
            headerVerticalLayout.childControlHeight = true;
            headerVerticalLayout.childControlWidth = true;
            headerVerticalLayout.childForceExpandHeight = true;
            headerVerticalLayout.childForceExpandWidth = true;
            headerVerticalLayout.spacing = 10f;
            headerVerticalLayout.padding = new RectOffset(12, 12, 12, 12);

            BuildProgressObject(headerObject.transform, totalAchievements, unlockedCount);

            headerObject.SetActive(true);
        }

        private GameObject CreateAchievementObject(Transform parent, RegisteredAchievement achievement, bool isTemplate = false)
        {
            if (achievement == null) return null;

            GameObject achievementObject = new GameObject("AchievementContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(CanvasGroup));
            achievementObject.transform.SetParent(parent, false);

            LayoutElement achievementLayout = achievementObject.GetComponent<LayoutElement>();
            achievementLayout.preferredHeight = 140f;

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
            frameImage.sprite = GetAchievementFrame(achievement.Definition.Difficulty);
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
            iconImage.sprite = achievement.IsUnlocked ? achievement.Definition.Icon : achievement.Definition.LockedIcon;
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

            GameObject headerObject = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            headerObject.transform.SetParent(textObject.transform, false);

            HorizontalLayoutGroup headerHorizontalLayout = headerObject.GetComponent<HorizontalLayoutGroup>();
            headerHorizontalLayout.childControlWidth = true;
            headerHorizontalLayout.childControlHeight = true;
            headerHorizontalLayout.childForceExpandWidth = false;
            headerHorizontalLayout.childForceExpandHeight = true;
            headerHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;

            GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            titleObject.transform.SetParent(headerObject.transform, false);

            TextMeshProUGUI titleTmp = titleObject.GetComponent<TextMeshProUGUI>();
            titleTmp.text = achievement.Definition.Title;
            titleTmp.fontSize = 24;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.font = this.font;
            titleTmp.enableWordWrapping = true;
            titleTmp.overflowMode = TextOverflowModes.Ellipsis;
            titleTmp.maxVisibleLines = 1;
            titleTmp.alignment = TextAlignmentOptions.Center;

            GameObject headerSpacerObject = new GameObject("HeaderSpacer", typeof(RectTransform), typeof(LayoutElement));
            headerSpacerObject.transform.SetParent(headerObject.transform, false);

            LayoutElement headerSpacerLayout = headerSpacerObject.GetComponent<LayoutElement>();
            headerSpacerLayout.flexibleWidth = 1f;

            if (achievement.Definition.Type == AchievementType.Counter && !isTemplate)
            {
                GameObject progressRowObject = new GameObject("ProgressRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                progressRowObject.transform.SetParent(headerObject.transform, false);

                HorizontalLayoutGroup progressRowHorizontalLayout = progressRowObject.GetComponent<HorizontalLayoutGroup>();
                progressRowHorizontalLayout.childControlWidth = true;
                progressRowHorizontalLayout.childControlHeight = true;
                progressRowHorizontalLayout.childForceExpandWidth = false;
                progressRowHorizontalLayout.childForceExpandHeight = false;
                progressRowHorizontalLayout.spacing = 10f;
                progressRowHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;

                LayoutElement progressRowLayout = progressRowObject.GetComponent<LayoutElement>();
                progressRowLayout.preferredHeight = 24f;

                GameObject progressNumberObject = new GameObject("ProgressNumber", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
                progressNumberObject.transform.SetParent(progressRowObject.transform, false);

                TextMeshProUGUI progressNumberTmp = progressNumberObject.GetComponent<TextMeshProUGUI>();
                string format = $"n{achievement.Definition.ProgressDecimals}";
                progressNumberTmp.text = $"{achievement.Progress.Progress.ToString(format)}/{achievement.Definition.MaxProgress.ToString(format)}";
                progressNumberTmp.fontSize = 18;
                progressNumberTmp.color = Color.white;
                progressNumberTmp.font = this.font;

                GameObject squaresObject = new GameObject("ProgressSquares", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                squaresObject.transform.SetParent(progressRowObject.transform, false);

                HorizontalLayoutGroup squaresHorizontalLayout = squaresObject.GetComponent<HorizontalLayoutGroup>();
                squaresHorizontalLayout.spacing = 4f;
                squaresHorizontalLayout.childControlWidth = true;
                squaresHorizontalLayout.childControlHeight = true;
                squaresHorizontalLayout.childForceExpandWidth = false;
                squaresHorizontalLayout.childForceExpandHeight = false;
                squaresHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;

                LayoutElement squaresLayout = squaresObject.GetComponent<LayoutElement>();
                squaresLayout.preferredHeight = 24f;

                int filledSquares = Mathf.FloorToInt((achievement.Progress.Progress / achievement.Definition.MaxProgress) * 10);
                for (int i = 0; i < 10; i++)
                {
                    GameObject squareObject = new GameObject($"Square{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                    squareObject.transform.SetParent(squaresObject.transform, false);

                    LayoutElement squareLayout = squareObject.GetComponent<LayoutElement>();
                    squareLayout.preferredWidth = 24f;
                    squareLayout.preferredHeight = 24f;

                    Image squareImage = squareObject.GetComponent<Image>();
                    squareImage.color = i < filledSquares ? Color.white : new Color(1f, 1f, 1f, 0.1f);
                }
            }


            GameObject descObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
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

            return achievementObject;
        }

        private void CreateHiddenAchievementObject(Transform parent, List<RegisteredAchievement> hiddenAchievements)
        {
            if (hiddenAchievements == null || hiddenAchievements.Count == 0) return;

            var achievementObject = CreateAchievementObject(parent, hiddenAchievements[0], true);
            achievementObject.name = "AchievementHiddenContainer";

            TextMeshProUGUI titleTmp = achievementObject.transform.Find("Text/Header/Title").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descTmp = achievementObject.transform.Find("Text/Description").GetComponent<TextMeshProUGUI>();
            titleTmp.text = string.Format("Скрытых достижений осталось: {0}", hiddenAchievements.Count()); // TODO: Localization
            descTmp.text = "Тебе придётся пошевелить мозгами, чтобы открыть все из них!"; // TODO: Localization

            Image frameImage = achievementObject.transform.Find("Frame").GetComponent<Image>();
            frameImage.sprite = AssetLoader.GetCachedSprite("achievement_frame_normal");

            Image iconImage = achievementObject.transform.Find("Icon").GetComponent<Image>();
            iconImage.sprite = AssetLoader.GetCachedSprite("achievement_default_hidden");
        }

        private Sprite GetAchievementFrame(AchievementDifficulty rank)
        {
            switch (rank)
            {
                case AchievementDifficulty.Normal:
                    return AssetLoader.GetCachedSprite("achievement_frame_normal");
                case AchievementDifficulty.Difficult:
                    return AssetLoader.GetCachedSprite("achievement_frame_difficult");
                case AchievementDifficulty.Hard:
                    return AssetLoader.GetCachedSprite("achievement_frame_hard");
            }
            return AssetLoader.GetCachedSprite("achievement_frame_normal");
        }

        private void OnDestroy()
        {
            AchievementEvents.AchievementsChanged -= OnAchievementsChanged;
        }
    }
}
