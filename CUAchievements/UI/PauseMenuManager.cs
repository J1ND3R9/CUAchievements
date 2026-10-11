using CUAchievements.Registry;
using CUAchievements.UI.Menu.Main;
using CUAchievements.UI.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal sealed class PauseMenuWindow : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        public Transform Panel { get; private set; }

        public event Action Opened;
        public event Action Closed;

        private const float PanelWidth = 380f;
        private const float PanelHeight = 800f;
        private const float ScrollWidth = PanelWidth - 10f;
        private const float ScrollHeight = PanelHeight - 100f;

        private Vector2 HiddenPos = new(600f, 0f);
        private Vector2 ShownPos = new(-20f, 0f);

        public void Initialize()
        {
            Build();
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
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
            Root.name = "Menu";

            ScrollRect scroll = Root.GetComponentInChildren<ScrollRect>(true);

            Content = scroll?.content;
            Root.transform.ClearPanel();
            Root.transform.ClearButtons();
            Content?.SetupScrollContent();

            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            rect.anchoredPosition = HiddenPos;
        }
    }

    internal sealed class PauseMenuManager : MonoBehaviour
    {
        private PauseMenuWindow window;
        private AchievementListView list;
        private bool needsRefresh = true;

        public void Initialize()
        {
            window = gameObject.AddComponent<PauseMenuWindow>();
            window.Initialize();

            list = new AchievementListView(window.Content, GameFonts.Retro, ShowDetails);

            window.Opened += OnOpened;
            AchievementEvents.AchievementsChanged += OnAchievementsChanged;
        }
        private void ShowDetails(RegisteredAchievement achievement)
        {
            throw new NotImplementedException();
        }

        private void OnOpened()
        {
            if (needsRefresh)
                Refresh();
        }

        private void OnAchievementsChanged()
        {
            throw new NotImplementedException();
        }

        private void Refresh()
        {
            throw new NotImplementedException();
        }

    }
}
