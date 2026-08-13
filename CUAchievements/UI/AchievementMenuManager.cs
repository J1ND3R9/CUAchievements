using CUAchievements.Definitions;
using CUAchievements.Persistence;
using CUAchievements.Registry;
using CUCoreLib.Registries;
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
        internal GameObject modalBackdrop;
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
            if (modalBackdrop != null) modalBackdrop.SetActive(show);
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
            modalCanvas.sortingOrder = 30000;

            modalRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            modalCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            modalBackdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            modalBackdrop.transform.SetParent(transform, false);
            RectTransform backdropRect = modalBackdrop.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            Image backdropImage = modalBackdrop.GetComponent<Image>();
            backdropImage.color = new Color(0f, 0f, 0f, 0.72f);
            backdropImage.raycastTarget = true;

            GameObject template = Resources.Load<GameObject>("Special/SettingsMenu");
            if (template == null)
            {
                CUAchievementsPlugin.Logger.LogError("SettingsMenu template not found.");
                return;
            }

            vanillaPanel = Instantiate(template, transform, false);
            vanillaPanel.name = "AchievementsModal";
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
            Transform panelTransform = vanillaPanel.transform.Find("Panel");
            panelTransform.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
            BuildTabs();
        }

        internal void RefreshAchievementList()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                if (content.GetChild(i).name == "AchievementRow")
                    Destroy(content.GetChild(i).gameObject);
            }

            foreach (var registeredAchievement in AchievementRegistry.GetAll())
            {
                AddAchievementRow(registeredAchievement.Value);

                // DEBUG INFO
                CUAchievementsPlugin.Logger.LogInfo($"[UI INFO] [{registeredAchievement.Key}]|[{registeredAchievement.Value.Definition.Title}] | UNLOCKED: {registeredAchievement.Value.Progress.Unlocked}");
            }
        }
        internal void AddAchievementRow(RegisteredAchievement achievement)
        {
            if (content == null) return;

            GameObject row = new GameObject("AchievementRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(content, false);

            LayoutElement layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 90f;

            HorizontalLayoutGroup horizontalLayoutGroup = row.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            horizontalLayoutGroup.spacing = 12f;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;

            GameObject iconGameObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGameObject.transform.SetParent(row.transform, false);
            iconGameObject.GetComponent<LayoutElement>().preferredHeight = 74f;
            iconGameObject.GetComponent<LayoutElement>().preferredWidth = 74f;
            Image iconImg = iconGameObject.GetComponent<Image>();
            iconImg.sprite = achievement.Definition.Sprite;
            iconImg.preserveAspect = true;
            iconImg.color = achievement.Progress.Unlocked ? Color.white : new Color(1f, 1f, 1f, 0.3f);

            GameObject textGameObject = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textGameObject.transform.SetParent(row.transform, false);
            textGameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var titleTmp = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            titleTmp.transform.SetParent(textGameObject.transform, false);
            titleTmp.text = achievement.Definition.Title;
            titleTmp.fontSize = 20;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.font = this.font;

            var descTmp = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            descTmp.transform.SetParent(textGameObject.transform, false);
            descTmp.text = achievement.Definition.Description;
            descTmp.fontSize = 15;
            descTmp.color = new Color(1f, 1f, 1f, 0.7f);
            descTmp.font = this.font;
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
