using CUAchievements.Plugin;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu
{
    internal sealed class MainMenuWindow : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        public Transform Panel { get; private set; }
        public Transform Header { get; private set; }

        public event Action Opened;
        public event Action Closed;

        public void Open() => SetVisible(true);
        public void Close() => SetVisible(false);

        public void Initialize()
        {
            BuildModal();
            SetVisible(false);
        }

        void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
        }
        private void BuildModal()
        {
            UIUtility.Stretch(transform);

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;

            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.AddComponent<CanvasGroup>();

            GameObject template = Resources.Load<GameObject>("Special/SettingsMenu");
            if (template == null)
            {
                Log.Error("SettingsMenu template not found.");
                return;
            }

            Root = Instantiate(template, transform, false);
            Root.name = "Modal";

            ClearModal(Root);

            Panel = Root.transform.Find("Panel");
            UIUtility.ClearPanel(Panel);
            Panel.gameObject.AddComponent<VerticalLayoutGroup>().Configure(spacing: 12f, expandW: true, padding: new RectOffset(12, 12, 12, 12));

            CreateHeaderAndBottom(out GameObject header, out GameObject bottom);

            TextMeshProUGUI descriptionComponent = Root.GetComponentInChildren<TextMeshProUGUI>();
            descriptionComponent.text = LocaleRegistry.Get("achievements_button", "Achievements");
            descriptionComponent.gameObject.AddComponent<LayoutIgnorer>();
            descriptionComponent.transform.SetParent(bottom.transform, false);
            descriptionComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            ScrollRect modalScroll = Root.GetComponentInChildren<ScrollRect>(true);
            Content = modalScroll?.content;
            Content?.SetupScrollContent();

            modalScroll.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            CreateBackButton(bottom);

            Header = header.transform;
        }

        private void CreateBackButton(GameObject bottom)
        {
            Button[] buttons = Root.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                Button closeButton = buttons[0];
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => SetVisible(false));
                TextMeshProUGUI closeButtonTextComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                closeButtonTextComponent.text = Locale.GetOther("back");

                closeButton.transform.SetParent(bottom.transform, false);
                UIUtility.ButtonTemplate = closeButton.gameObject;

                LayoutElement le = closeButton.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 48f;
                le.preferredWidth = 250f;
                UIUtility.CreateSpacer(bottom.transform);
            }
        }

        private void CreateHeaderAndBottom(out GameObject header, out GameObject bottom)
        {
            header = UIUtility.Create("Header", Panel, typeof(HorizontalLayoutGroup));
            bottom = UIUtility.Create("Bottom", Panel, typeof(HorizontalLayoutGroup));
            header.transform.SetAsFirstSibling();
            bottom.transform.SetAsLastSibling();

            header.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 6f, expandW: true);
            bottom.GetComponent<HorizontalLayoutGroup>().Configure(expandW: false);
        }

        private void SetVisible(bool show)
        {
            IsOpen = show;

            Root?.SetActive(show);

            if (show)
                Opened?.Invoke();
            else
                Closed?.Invoke();
        }

        private void ClearModal(GameObject modalPanel)
        {
            foreach (MonoBehaviour monoBehaviourObject in modalPanel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (monoBehaviourObject == null) continue;
                string monoNamespace = monoBehaviourObject.GetType().Namespace ?? "";
                if (!monoNamespace.StartsWith("UnityEngine.UI") && !monoNamespace.StartsWith("TMPro"))
                    Destroy(monoBehaviourObject);
            }
        }

        private void OnDestroy() => IsOpen = false;
    }
}