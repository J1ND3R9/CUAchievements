using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.SidePanels
{
    internal sealed class StatsPanel : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        //public event Action Opened;
        //public event Action Closed;

        private const float PanelWidth = 380f;
        private const float PanelHeight = 800f;
        private const float ScrollWidth = PanelWidth - 10f;
        private const float ScrollHeight = PanelHeight - 100f;

        private Coroutine slideAnim;

        private Vector2 HiddenPos = new(600f, 0f);
        private Vector2 ShownPos = new(-20f, 0f);

        private TextMeshProUGUI unlockedPrecentageText;
        private TextMeshProUGUI outOfText;
        private TextMeshProUGUI lockedText;
        private TextMeshProUGUI unlockedText;

        private GameObject modifications;

        private bool needsRefresh = true;

        public static StatsPanel Create(Transform parent, Transform panel)
        {
            GameObject statsPanel = Instantiate(panel, parent).gameObject;
            statsPanel.name = "Stats";

            StatsPanel panelComponent = statsPanel.AddComponent<StatsPanel>();

            panelComponent.Initialize();
            return panelComponent;
        }

        public void Open() => SetVisible(true);
        public void Close() => SetVisible(false);

        void OnEnable()
        {
            AchievementEvents.AchievementsChanged += OnAchievementsChanged;
        }

        private void OnAchievementsChanged()
        {
            needsRefresh = true;
        }

        void OnDisable()
        {
            AchievementEvents.AchievementsChanged -= OnAchievementsChanged;
        }

        void OnOpen()
        {
            if (needsRefresh) Refresh();
            PlayAnim(ShownPos);
        }

        void OnClose() => PlayAnim(HiddenPos);

        private void Initialize()
        {
            Root = transform.gameObject;

            ScrollRect scroll = Root.GetComponentInChildren<ScrollRect>(true);
            scroll.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            scroll.HideScrollbar();

            Content = scroll?.content;
            if (Content != null)
            {
                Root.transform.ClearPanel();
                Root.transform.ClearButtons();
                VerticalLayoutGroup vertical = Content.SetupScrollContent(spacing: 16f);
                vertical.name = "Statistic";
            }

            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(ScrollWidth, ScrollHeight);

            //TextMeshProUGUI description = transform.GetComponentInChildren<TextMeshProUGUI>();
            //description.text = LocaleRegistry.Get("achievements_stats_title", "Statistic");
            //description.transform.SetAsLastSibling();

            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            rect.anchoredPosition = HiddenPos;

            (int unlocked, int total) = AchievementRegistry.GetTotalProgress();

            CreateHeader(parent: Content, unlocked, total);
            UIUtility.CreateSeparator(parent: Content);

            CreateStatistic(parent: Content, unlocked, total);
            UIUtility.CreateSeparator(parent: Content);

            CreateByMod(parent: Content);
        }

        private void CreateByMod(Transform parent)
        {
            GameObject byModObject = UIUtility.Create("ByMod", parent, typeof(VerticalLayoutGroup));
            byModObject.GetComponent<VerticalLayoutGroup>().Configure(spacing: 24f);

            string byModLocale = LocaleRegistry.Get("achievements_stats_by_mod", "BY MOD");
            UIUtility.CreateText(byModObject.transform, "ByModText", byModLocale, GameFonts.Retro, 18f);

            modifications = UIUtility.Create("ModificationList", byModObject.transform, typeof(VerticalLayoutGroup));

            IReadOnlyDictionary<string, List<RegisteredAchievement>> achievementsPerPlugin = AchievementRegistry.GetAchievementsPerPlugin(nameInsteadGuid: true);
            FillModifications(parent: modifications.transform, achievements: achievementsPerPlugin);
        }

        private void FillModifications(Transform parent, IReadOnlyDictionary<string, List<RegisteredAchievement>> achievements)
        {
            foreach (KeyValuePair<string, List<RegisteredAchievement>> plugin in achievements)
            {
                int total = plugin.Value.Count;
                int unlocked = plugin.Value.Count(a => a.IsUnlocked);

                (TextMeshProUGUI labelText, TextMeshProUGUI _) = UIUtility.CreateStatisticRow(parent, plugin.Key, $"{plugin.Key}:", $"{unlocked}/{total}");
                labelText.enableWordWrapping = true;
                labelText.overflowMode = TextOverflowModes.Ellipsis;
                labelText.maxVisibleLines = 1;
            }
        }

        private void CreateStatistic(Transform parent, int unlocked, int total)
        {
            GameObject statisticObject = UIUtility.Create("Statistic", parent, typeof(VerticalLayoutGroup));

            string totalLocale = LocaleRegistry.Get("achievements_stats_total", "TOTAL");
            string unlockedLocale = LocaleRegistry.Get("achievements_unlocked", "UNLOCKED");
            string lockedLocale = LocaleRegistry.Get("achievements_locked", "LOCKED");

            UIUtility.CreateStatisticRow(parent: statisticObject.transform, name: "Total", label: $"{totalLocale}:", $"{total}");
            (_, unlockedText) = UIUtility.CreateStatisticRow(statisticObject.transform, "Unlocked", $"{unlockedLocale}:", $"{unlocked}");
            (_, lockedText) = UIUtility.CreateStatisticRow(statisticObject.transform, "Locked", $"{lockedLocale}:", $"{total - unlocked}");
        }

        private void CreateHeader(Transform parent, int unlocked, int total)
        {
            GameObject headerObject = UIUtility.Create("Header", parent, typeof(VerticalLayoutGroup));
            headerObject.GetComponent<VerticalLayoutGroup>().Configure(spacing: 8f, align: TextAnchor.MiddleCenter);

            int precentage = total > 0 ? (int)Math.Round((double)unlocked / total * 100, MidpointRounding.AwayFromZero) : 0;

            unlockedPrecentageText = UIUtility.CreateText(headerObject.transform, "Precentage", $"{precentage}%", GameFonts.Retro, 20f, TextAlignmentOptions.Center);

            int filledSquares = unlocked > 0
                ? Mathf.Clamp(Mathf.FloorToInt((float)unlocked / total * 10), 0, 10)
                : 0;

            UIUtility.CreateSquareProgress(headerObject.transform, 24f, filledSquares);

            string achievedLocale = LocaleRegistry.Get("achievements_stats_achieved", "ACHIEVED");

            UIUtility.CreateText(headerObject.transform, "Achieved", achievedLocale, GameFonts.Retro, 20f, TextAlignmentOptions.Center);

            outOfText = UIUtility.CreateText(headerObject.transform, "OutOf", $"{unlocked}/{total}", GameFonts.Retro, 24f, TextAlignmentOptions.Center);
        }

        private void SetVisible(bool show)
        {
            IsOpen = show;

            if (show)
                OnOpen();
            else
                OnClose();
        }

        private void Refresh()
        {
            RefreshText();
            RefreshModificationList();
        }

        private void RefreshText()
        {
            (int unlocked, int total) = AchievementRegistry.GetTotalProgress();
            int precentage = total > 0 ? (int)Math.Round((double)unlocked / total * 100, MidpointRounding.AwayFromZero) : 0;

            unlockedPrecentageText?.text = $"{precentage}%";
            outOfText?.text = $"{unlocked}/{total}";
            lockedText?.text = $"{total - unlocked}";
            unlockedText?.text = $"{unlocked}";
        }

        private void RefreshModificationList()
        {
            IReadOnlyDictionary<string, List<RegisteredAchievement>> achievementsPerPlugin = AchievementRegistry.GetAchievementsPerPlugin(nameInsteadGuid: true);
            foreach (Transform child in modifications.transform)
            {
                if (achievementsPerPlugin.ContainsKey(child.name))
                    Destroy(child.gameObject);
            }
            FillModifications(modifications.transform, achievementsPerPlugin);
        }
        private void PlayAnim(Vector2 target)
        {
            if (slideAnim != null) StopCoroutine(slideAnim);
            slideAnim = StartCoroutine(Animate(target, (RectTransform)Root.transform));
        }
        private IEnumerator Animate(Vector2 target, RectTransform rect)
        {
            yield return AnimationUtility.SlideTo(target, rect);
        }
    }
}