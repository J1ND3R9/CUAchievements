using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal sealed class StatsPanel : MonoBehaviour
    {
        public static bool IsStatsOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        public Transform Panel { get; private set; }
        public event Action Opened;
        public event Action Closed;

        private const float PanelWidth = 370f;
        private const float PanelHeight = 700f;

        private Vector2 HiddenPos = new(600f, 0f);
        private Vector2 ShownPos = new(-20f, 0f);

        private TextMeshProUGUI unlockedPrecentageText;
        private TextMeshProUGUI outOfText;
        private TextMeshProUGUI lockedText;
        private TextMeshProUGUI unlockedText;

        private GameObject modifications;

        private Coroutine slideAnim;

        private bool needsRefresh = true;

        public static StatsPanel Create(Transform parent, Transform panel)
        {
            GameObject statsPanel = Instantiate(panel, parent).gameObject;
            statsPanel.name = "StatsPanel";

            StatsPanel panelComponent = statsPanel.AddComponent<StatsPanel>();

            panelComponent.Initialize();
            panelComponent.SetVisible(false);
            return panelComponent;
        }

        public void Open() => SetVisible(true);
        public void Close() => SetVisible(false);

        void OnEnable()
        {
            AchievementEvents.AchievementsChanged += OnAchievementsChanged;
            Opened += OnOpen;
            Closed += OnClose;
        }

        private void OnAchievementsChanged()
        {
            needsRefresh = true;
        }

        void OnDisable()
        {
            AchievementEvents.AchievementsChanged -= OnAchievementsChanged;
            Opened -= OnOpen;
            Closed -= OnClose;
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

            var scroll = Root.GetComponentInChildren<ScrollRect>(true);

            Content = scroll?.content;
            if (Content != null)
            {
                Root.transform.ClearPanel();
                var vertical = Content.SetupScrollContent(spacing: 16f);
                vertical.name = "Statistic";
            }

            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            var description = transform.GetComponentInChildren<TextMeshProUGUI>();
            description.text = LocaleRegistry.Get("achievements_stats_panel_title", "Statistic");

            RectTransform statsRect = GetComponent<RectTransform>();
            statsRect.anchorMax = new Vector2(1f, 0.5f);
            statsRect.anchorMin = new Vector2(1f, 0.5f);
            statsRect.pivot = new Vector2(1f, 0.5f);
            statsRect.sizeDelta = new Vector2(380f, 800f);
            statsRect.anchoredPosition = HiddenPos;

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

            string byModLocale = LocaleRegistry.Get("achievements_stats_panel_by_mod", "BY MOD");
            UIUtility.CreateText(parent: byModObject.transform,
                                 name: "ByModText",
                                 text: byModLocale,
                                 font: GameFonts.Retro,
                                 fontSize: 18f);

            modifications = UIUtility.Create("ModificationList", byModObject.transform, typeof(VerticalLayoutGroup));

            IReadOnlyDictionary<string, List<RegisteredAchievement>> achievementsPerPlugin = AchievementRegistry.GetAchievementsPerPlugin(nameInsteadGuid: true);
            FillModifications(parent: modifications.transform, achievements: achievementsPerPlugin);
        }

        private void FillModifications(Transform parent, IReadOnlyDictionary<string, List<RegisteredAchievement>> achievements)
        {
            foreach (var plugin in achievements)
            {
                int total = plugin.Value.Count;
                int unlocked = plugin.Value.Count(a => a.IsUnlocked);

                (TextMeshProUGUI labelText, TextMeshProUGUI _) = CreateStatisticRow(parent, plugin.Key, $"{plugin.Key}:", $"{unlocked}/{total}");
                labelText.enableWordWrapping = true;
                labelText.overflowMode = TextOverflowModes.Ellipsis;
                labelText.maxVisibleLines = 1;
            }
        }

        private void CreateStatistic(Transform parent, int unlocked, int total)
        {
            GameObject statisticObject = UIUtility.Create("Statistic", parent, typeof(VerticalLayoutGroup));

            string totalLocale = LocaleRegistry.Get("achievements_stats_panel_total", "TOTAL");
            string unlockedLocale = LocaleRegistry.Get("achievements_stats_panel_unlocked", "UNLOCKED");
            string lockedLocale = LocaleRegistry.Get("achievements_stats_panel_locked", "LOCKED");

            CreateStatisticRow(parent: statisticObject.transform, name: "Total", label: $"{totalLocale}:", $"{total}");
            (_, TextMeshProUGUI unlockedValue) = CreateStatisticRow(parent: statisticObject.transform, name: "Unlocked", label: $"{unlockedLocale}:", $"{unlocked}");
            (_, TextMeshProUGUI lockedValue) = CreateStatisticRow(parent: statisticObject.transform, name: "Locked", label: $"{lockedLocale}:", $"{total - unlocked}");

            unlockedText = unlockedValue;
            lockedText = lockedValue;
        }

        private void CreateHeader(Transform parent, int unlocked, int total)
        {
            GameObject headerObject = UIUtility.Create("Header", parent, typeof(VerticalLayoutGroup));
            headerObject.GetComponent<VerticalLayoutGroup>().Configure(spacing: 8f, align: TextAnchor.MiddleCenter);

            int precentage = total > 0 ? (int)Math.Round((double)unlocked / total * 100, MidpointRounding.AwayFromZero) : 0;

            unlockedPrecentageText = UIUtility.CreateText(parent: headerObject.transform,
                                                      name: "Precentage",
                                                      text: $"{precentage}%",
                                                      GameFonts.Retro,
                                                      fontSize: 20f,
                                                      alignment: TextAlignmentOptions.Center);

            int filledSquares = unlocked > 0 
                ? Mathf.Clamp(Mathf.FloorToInt((float)unlocked / total * 10), 0, 10) 
                : 0;

            UIUtility.CreateSquareProgress(headerObject.transform, 24f, filledSquares);

            string achievedLocale = LocaleRegistry.Get("achievements_stats_panel_achieved", "ACHIEVED");

            UIUtility.CreateText(parent: headerObject.transform,
                                 name: "Achieved",
                                 text: achievedLocale,
                                 font: GameFonts.Retro,
                                 fontSize: 20f,
                                 alignment: TextAlignmentOptions.Center);

            outOfText = UIUtility.CreateText(parent: headerObject.transform,
                                         name: "OutOf",
                                         text: $"{unlocked}/{total}",
                                         font: GameFonts.Retro,
                                         fontSize: 18f,
                                         alignment: TextAlignmentOptions.Center);
        }

        private (TextMeshProUGUI labelTmp, TextMeshProUGUI valueTmp) CreateStatisticRow(Transform parent, string name, string label, string value)
        {
            GameObject row = UIUtility.Create(name, parent, typeof(HorizontalLayoutGroup));
            row.GetComponent<HorizontalLayoutGroup>().Configure(expandH: true);

            TextMeshProUGUI labelTmp = UIUtility.CreateText(row.transform, "Label", label, GameFonts.Retro, 18f);
            UIUtility.CreateSpacer(row.transform);
            TextMeshProUGUI valueTmp = UIUtility.CreateText(row.transform, "Value", value, GameFonts.Retro, 18f);

            return (labelTmp, valueTmp);
        }

        private void SetVisible(bool show)
        {
            IsStatsOpen = show;

            if (show)
                Opened?.Invoke();
            else
                Closed?.Invoke();
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