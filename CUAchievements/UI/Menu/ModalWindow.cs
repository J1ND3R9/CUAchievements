using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using CUAchievements.Persistence;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu
{
    internal sealed class ModalWindow : MonoBehaviour
    {
        public static bool IsModalOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        public Transform Panel { get; private set; }

        public event Action Opened;
        public event Action Closed;

        public void Show() => SetVisible(true);
        public void Hide() => SetVisible(false);

        public void Initialize()
        {
            BuildModal();
            SetVisible(false);
        }

        void Update()
        {
            if (IsModalOpen && Input.GetKeyDown(KeyCode.Escape)) Hide();
        }
        private void BuildModal()
        {
            UIUtility.Stretch(GetComponent<RectTransform>());

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;

            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.AddComponent<CanvasGroup>();

            GameObject template = Resources.Load<GameObject>("Special/SettingsMenu");
            if (template == null)
            {
                CUAchievementsPlugin.Logger.LogError("SettingsMenu template not found.");
                return;
            }

            Root = Instantiate(template, transform, false);
            Root.name = "Modal";

            ClearModal(Root);

            UIUtility.Stretch(Root.transform);

            var descriptionComponent = Root.GetComponentInChildren<TextMeshProUGUI>();
            descriptionComponent.text = LocaleRegistry.Get("achievements_button", "Achievements");

            var modalScroll = Root.GetComponentInChildren<ScrollRect>(true);
            Content = modalScroll?.content;

            ConfigureModalContent();

            Button[] buttons = Root.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                b.gameObject.SetActive(b == buttons[0]);
            }
            if (buttons.Length > 0)
            {
                var closeButton = buttons[0];
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => SetVisible(false));
                var closeButtonTextComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                closeButtonTextComponent.text = Locale.GetOther("back");

                UIUtility.ButtonTemplate = closeButton.gameObject;
            }

            Panel = Root.transform.Find("Panel");
        }

        private void SetVisible(bool show)
        {
            IsModalOpen = show;

            if (Root != null) Root.SetActive(show);

            if (show)
                Opened?.Invoke();
            else
                Closed?.Invoke();
        }

        private void ConfigureModalContent()
        {
            if (Content != null)
            {
                for (int i = Content.childCount - 1; i >= 0; i--)
                    Destroy(Content.GetChild(i).gameObject);

                VerticalLayoutGroup modalVlg = Content.gameObject.AddComponent<VerticalLayoutGroup>();
                modalVlg.Configure(spacing: 24f,
                    controlW: true,
                    controlH: true,
                    expandW: true,
                    expandH: true,
                    padding: new RectOffset(10, 10, 10, 10)
                    );

                ContentSizeFitter modalFitter = Content.gameObject.AddComponent<ContentSizeFitter>();
                modalFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        private void ClearModal(GameObject modalPanel)
        {
            foreach (var monoBehaviourObject in modalPanel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (monoBehaviourObject == null) continue;
                string monoNamespace = monoBehaviourObject.GetType().Namespace ?? "";
                if (!monoNamespace.StartsWith("UnityEngine.UI") && !monoNamespace.StartsWith("TMPro"))
                    Destroy(monoBehaviourObject);
            }
        }
    }
}