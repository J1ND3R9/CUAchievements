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
        public ModalWindow Window;
        private AchievementListView list;
        private AchievementStatsPanel statsPanel;
        private AchievementDetailsPanel detailsPanel;
        private string currentFilter = "";
        private bool needsRefresh = true;
        public void Initialize(TMP_FontAsset font)
        {
            Window = gameObject.AddComponent<ModalWindow>();
            Window.Initialize();

            BuildTabs(font);
            statsPanel = AchievementStatsPanel.Create(Window.Root.transform, font);
            detailsPanel = AchievementDetailsPanel.Create(Window.Root.transform, font);
            list = new AchievementListView(Window.Content, font, ShowDetails);

            Window.Opened += OnOpened;
            AchievementEvents.AchievementsChanged += OnAchievementsChanged;
            Window.Hide();
        }
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
            statsPanel.SetVisible();
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

        private void BuildTabs(TMP_FontAsset font)
        {
            GameObject headerObject = UIUtility.Create("ModalHeader", Window.Panel, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
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
            UIUtility.CreateInputField("SearchField", tabsObject.transform, searchLocale, font, OnSearchChanged);
            string statsLocale = LocaleRegistry.Get("achievements_panel_stats_button", "Stats");
            UIUtility.CreateButton(tabsObject.transform, "StatsButton", statsLocale, OnStatisticClick);
        }
        private void OnDestroy() => AchievementEvents.AchievementsChanged -= OnAchievementsChanged;
    }
}