using CUAchievements.Registry;
using CUAchievements.UI.Menu;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal sealed class MenuManager : MonoBehaviour
    {
        private ModalWindow window;
        private AchievementListView list;
        private StatsPanel statsPanel;
        private AchievementDetailsPanel detailsPanel;
        private string currentFilter = "";
        private bool needsRefresh = true;
        public void Initialize(TMP_FontAsset font)
        {
            window = gameObject.AddComponent<ModalWindow>();
            window.Initialize();

            BuildTabs();
            statsPanel = StatsPanel.Create(window.Root.transform, window.Panel);
            detailsPanel = AchievementDetailsPanel.Create(window.Root.transform);
            list = new AchievementListView(window.Content, font, ShowDetails);

            window.Opened += OnOpened;
            AchievementEvents.AchievementsChanged += OnAchievementsChanged;
            window.Hide();
        }
        internal static MenuManager Create(Transform canvas, TMP_FontAsset font)
        {
            var gameObject = UIUtility.Create("AchievementModal", canvas);
            var manager = gameObject.AddComponent<MenuManager>();
            manager.Initialize(font);
            return manager;
        }
        public void Open() => window.Show();
        private void OnOpened()
        {
            if (needsRefresh)
                Refresh();
        }
        private void OnSearchChanged(string text)
        {
            currentFilter = text; Refresh();
        }
        private void OnStatisticClick()
        {
            if (StatsPanel.IsStatsOpen)
                statsPanel.Close();
            else
                statsPanel.Open();
        }

        private void OnAchievementsChanged()
        {
            needsRefresh = true;

            if (ModalWindow.IsModalOpen)
                Refresh();
        }

        private void Refresh()
        {
            list.Rebuild(AchievementRegistry.GetAchievementsPerPlugin(true, currentFilter));
            needsRefresh = false;
        }

        private void ShowDetails(RegisteredAchievement achievement) =>
            detailsPanel.Refresh(achievement, AchievementFrames.Get(achievement.Definition.Difficulty));

        private void BuildTabs()
        {
            GameObject headerObject = UIUtility.Create("ModalHeader", window.Panel, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            RectTransform headerRect = headerObject.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 48f);
            headerRect.anchoredPosition = Vector2.zero;

            GameObject tabsObject = UIUtility.Create("Tabs", headerObject.transform, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            tabsObject.GetComponent<LayoutElement>().preferredHeight = 48f;

            var tabsHlg = tabsObject.GetComponent<HorizontalLayoutGroup>();
            tabsHlg.Configure(spacing: 8f,
                expandW: true,
                expandH: true,
                padding: new RectOffset(6, 6, 3, 3)
                );

            string searchLocale = LocaleRegistry.Get("achievements_panel_search_placeholder", "Search...");
            UIUtility.CreateInputField("SearchField", tabsObject.transform, searchLocale, GameFonts.Retro, OnSearchChanged);
            string statsLocale = LocaleRegistry.Get("achievements_panel_stats_button", "Stats");
            UIUtility.CreateButton(tabsObject.transform, "StatsButton", statsLocale, OnStatisticClick);
        }
        private void OnDestroy() => AchievementEvents.AchievementsChanged -= OnAchievementsChanged;
    }
}