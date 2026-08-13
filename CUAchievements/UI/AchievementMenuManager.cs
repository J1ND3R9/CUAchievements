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
    public class AchievementMenuManager : MonoBehaviour
    {
        private GameObject modalBackdrop;
        private GameObject vanillaPanel;
        private RectTransform vanillaPanelRect;
        private ScrollRect scrollRect;
        private RectTransform content;
        private Button closeButton;
        private Canvas modalCanvas;
        private CanvasGroup modalCanvasGroup;
        private GraphicRaycaster modalRaycaster;

        private bool visible;
        public static bool IsModalOpen { get; private set; }

        public void Initialize()
        {
            BuildModal();
            SetVisible(false);
        }

        public void SetVisible(bool show)
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
            }
        }

        public void AddAchievements()
        {

        }
    }
}
