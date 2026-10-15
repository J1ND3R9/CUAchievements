using CUAchievements.Registry;
using CUAchievements.UI.Menu.Main;
using CUAchievements.UI.Menu.SidePanels;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System.Collections.Generic;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal sealed class MainMenuManager : MonoBehaviour
    {
        private MainMenuWindow window;

        private AchievementListView list;

        private StatsPanel statsPanel;

        private DetailsPanel detailsPanel;

        private string currentFilter = "";

        private bool needsRefresh = true;
        private bool anyRegistered = true;

        private const string ObjectName = "AchievementModal";
        public void Initialize(TMP_FontAsset font)
        {
            window = gameObject.AddComponent<MainMenuWindow>();
            window.Initialize();
            
            BuildTabs();
            statsPanel = StatsPanel.Create(window.Root.transform, window.Panel);
            detailsPanel = DetailsPanel.Create(window.Root.transform, window.Panel);
            list = new AchievementListView(window.Content, font, ShowDetails);

            anyRegistered = AchievementRegistry.GetAll().Count > 0;
            window.Opened += OnOpened;
            AchievementEvents.AchievementsChanged += OnAchievementsChanged;

            window.Close();
        }
        internal static MainMenuManager Create(Transform canvas, TMP_FontAsset font)
        {
            var gameObject = UIUtility.Create(ObjectName, canvas);
            var manager = gameObject.AddComponent<MainMenuManager>();
            manager.Initialize(font);
            return manager;
        }
        public void Open() => window.Open();
        private void OnOpened()
        {
            if (needsRefresh)
                Refresh();
        }
        private void OnSearchChanged(string text)
        {
            currentFilter = text;
            Refresh();
        }
        private void OnStatisticClick()
        {
            if (StatsPanel.IsOpen)
                statsPanel.Close();
            else
                statsPanel.Open();
        }

        private void OnAchievementsChanged() => needsRefresh = true;

        private void Refresh()
        {
            if (!anyRegistered)
            {
                list.BuildEmptyNote(true);
            }
            else
            {
                IReadOnlyDictionary<string, List<RegisteredAchievement>> perPlugin = AchievementRegistry.GetAchievementsPerPlugin(true, currentFilter);
                if (perPlugin.Count > 0)
                    list.Rebuild(perPlugin);
                else
                    list.BuildEmptyNote(false);
            }
            needsRefresh = false;
        }

        private void ShowDetails(RegisteredAchievement achievement) =>
            detailsPanel.Open(achievement);

        private void BuildTabs()
        {
            string searchLocale = LocaleRegistry.Get("achievements_panel_search_placeholder", "Search...");
            var search = UIUtility.CreateInputField("SearchField", window.Header, searchLocale, GameFonts.Retro, true, true ? OnSearchChanged : null);

            var searchLe = search.gameObject.AddComponent<LayoutElement>();
            searchLe.flexibleWidth = 1f;
            searchLe.minHeight = 48f;
            searchLe.preferredWidth = 600f;

            string statsLocale = LocaleRegistry.Get("achievements_panel_stats_button", "Stats");
            var stats = UIUtility.CreateButton(window.Header, "StatsButton", statsLocale, true, true ? OnStatisticClick : null);

            var statsLe = stats.gameObject.AddComponent<LayoutElement>();
            statsLe.minHeight = 48f;
        }
        private void OnDestroy() => AchievementEvents.AchievementsChanged -= OnAchievementsChanged;

        internal static MainMenuManager GetOrCreate(Transform canvas, TMP_FontAsset font)
        {
            Transform existing = canvas.Find(ObjectName);
            if (existing != null)
            {
                MainMenuManager found = existing.GetComponent<MainMenuManager>();
                if (found != null) return found;
            }
            return Create(canvas, font);
        }
    }
}