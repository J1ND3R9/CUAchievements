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

namespace CUAchievements.UI
{
    internal sealed class AchievementStatsPanel : MonoBehaviour
    {
        private RectTransform statsPanelRect;
        private TextMeshProUGUI totalUnlockedText;
        private TextMeshProUGUI unlockedText;
        private TextMeshProUGUI lockedText;
        private GameObject pluginContainer;
        private TMP_FontAsset fontAsset;
        private bool isVisible = false;
        private Vector2 hiddenPos = new Vector2(600f, 0f);
        private Vector2 shownPos = new Vector2(-20f, 0f);

        public static AchievementStatsPanel Create(Transform parent, TMP_FontAsset font)
        {
            Transform panelTransform = parent.Find("Panel");
            if (panelTransform == null) return null;

            GameObject statsPanel = Instantiate(panelTransform, parent).gameObject;
            statsPanel.name = "StatsPanel";

            AchievementStatsPanel panelComponent = statsPanel.AddComponent<AchievementStatsPanel>();
            panelComponent.fontAsset = font;

            panelComponent.Initialize();

            AchievementEvents.AchievementsChanged += panelComponent.OnAchievementsChanged;

            return panelComponent;
        }

        private void Initialize()
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Scroll")
                {
                    var content = child.Find("Content");
                    if (content != null)
                    {
                        foreach (Transform child2 in content.transform)
                            Destroy(child2.gameObject);
                    }
                    continue;
                }
                if (child.name != "Description")
                    Destroy(child.gameObject);
            }

            GameObject statsScroll = transform.Find("Scroll").gameObject;
            RectTransform statsScrollRect = statsScroll.GetComponent<RectTransform>();
            statsScrollRect.sizeDelta = new Vector2(370f, 700f);

            TextMeshProUGUI description = GetComponentInChildren<TextMeshProUGUI>();
            if (description != null)
            {
                string descriptionLocale = LocaleRegistry.Get("statspanel_statistic", "Statistic");
                description.text = descriptionLocale;
            }

            RectTransform statsRect = GetComponent<RectTransform>();
            statsRect.anchorMax = new Vector2(1f, 0.5f);
            statsRect.anchorMin = new Vector2(1f, 0.5f);
            statsRect.pivot = new Vector2(1f, 0.5f);
            statsRect.sizeDelta = new Vector2(380f, 800f);
            statsRect.anchoredPosition = hiddenPos;

            statsPanelRect = statsRect;

            Transform contentTransform = statsScroll.transform.Find("Viewport/Content");

            GameObject statisticGameObject = new GameObject("Statistic", typeof(RectTransform), typeof(VerticalLayoutGroup));
            statisticGameObject.transform.SetParent(contentTransform, false);

            VerticalLayoutGroup statisticVerticalLayout = statisticGameObject.GetComponent<VerticalLayoutGroup>();
            statisticVerticalLayout.spacing = 16f;

            (int, int) achievementsProgress = AchievementRegistry.GetTotalProgress();

            GameObject progressGameObject = new GameObject("GlobalProgress", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            progressGameObject.transform.SetParent(statisticGameObject.transform, false);
            CreateProgressBar(fontAsset, progressGameObject, achievementsProgress);

            GameObject achievedGameObject = new GameObject("AchievedTextContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            achievedGameObject.transform.SetParent(statisticGameObject.transform, false);

            string achievedLocale = LocaleRegistry.Get("statspanel_total_achieved", "ACHIEVED");
            UIUtility.CreateText(achievedGameObject.transform, "AchievedText", achievedLocale, fontAsset, 16f, TextAlignmentOptions.Center);
            TextMeshProUGUI achievedTotalTmp = UIUtility.CreateText(achievedGameObject.transform, "AchievedTotal", $"{achievementsProgress.Item1}/{achievementsProgress.Item2}", fontAsset, 20f, TextAlignmentOptions.Center);
            totalUnlockedText = achievedTotalTmp;

            UIUtility.CreateSeparator(statisticGameObject.transform);

            GameObject globalStatisticGameObject = new GameObject("StatisticObject", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            globalStatisticGameObject.transform.SetParent(statisticGameObject.transform, false);

            string totalLocale = LocaleRegistry.Get("statspanel_total", "TOTAL");
            string unlockedLocale = LocaleRegistry.Get("statspanel_unlocked", "UNLOCKED");
            string lockedLocale = LocaleRegistry.Get("statspanel_locked", "LOCKED");
            CreateStatisticRow(globalStatisticGameObject.transform, "Total", $"{totalLocale}:", $"{achievementsProgress.Item2}", fontAsset);
            (Transform _, TextMeshProUGUI _, TextMeshProUGUI value1) = CreateStatisticRow(globalStatisticGameObject.transform, "Unlocked", $"{unlockedLocale}:", $"{achievementsProgress.Item1}", fontAsset);
            (Transform _, TextMeshProUGUI _, TextMeshProUGUI value2) = CreateStatisticRow(globalStatisticGameObject.transform, "Locked", $"{lockedLocale}:", $"{achievementsProgress.Item2 - achievementsProgress.Item1}", fontAsset);

            unlockedText = value1;
            lockedText = value2;

            UIUtility.CreateSeparator(statisticGameObject.transform);

            GameObject byModGameObject = new GameObject("ByModObject", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            byModGameObject.transform.SetParent(statisticGameObject.transform, false);

            VerticalLayoutGroup byModVerticalLayout = byModGameObject.GetComponent<VerticalLayoutGroup>();
            byModVerticalLayout.spacing = 24f;

            string byModLocale = LocaleRegistry.Get("statspanel_by_mod", "BY MOD");
            UIUtility.CreateText(byModGameObject.transform, "ByModText", byModLocale, fontAsset, 18f);

            GameObject byModModificationsContainer = new GameObject("ModificationList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            byModModificationsContainer.transform.SetParent(byModGameObject.transform, false);

            var achievementsPerPlugin = AchievementRegistry.GetAchievementsPerPlugin(true);
            CreatePluginContainer(byModModificationsContainer.transform, fontAsset, achievementsPerPlugin);

            pluginContainer = byModModificationsContainer;
        }

        private void RefreshPluginContainer(Transform parent, TMP_FontAsset font)
        {
            var achievementsPerPlugin = AchievementRegistry.GetAchievementsPerPlugin(true);
            foreach (Transform child in parent)
            {
                if (achievementsPerPlugin.ContainsKey(child.name))
                    Destroy(child.gameObject);
            }
            CreatePluginContainer(parent, font, achievementsPerPlugin);
        }

        private void CreatePluginContainer(Transform parent, TMP_FontAsset font, IReadOnlyDictionary<string, List<RegisteredAchievement>> achievementsPerPlugin)
        {
            foreach (var plugin in achievementsPerPlugin)
            {
                int totalAchievements = plugin.Value.Count;
                int unlockedCount = plugin.Value.Count(a => a.IsUnlocked);
                (Transform _, TextMeshProUGUI labelText, TextMeshProUGUI _) = CreateStatisticRow(parent, plugin.Key, $"{plugin.Key}:", $"{unlockedCount}/{totalAchievements}", font);
                labelText.enableWordWrapping = true;
                labelText.overflowMode = TextOverflowModes.Ellipsis;
                labelText.maxVisibleLines = 1;
            }
        }

        private (Transform, TextMeshProUGUI, TextMeshProUGUI) CreateStatisticRow(Transform parent, string name, string label, string value, TMP_FontAsset font)
        {
            GameObject row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            TextMeshProUGUI tmpLabel = UIUtility.CreateText(row.transform, "Label", label, font, 18f);

            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(row.transform, false);

            LayoutElement spacerLayout = spacer.GetComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1f;

            TextMeshProUGUI tmpValue = UIUtility.CreateText(row.transform, "Value", value, font, 18f);

            return (row.transform, tmpLabel, tmpValue);
        }

        private void OnAchievementsChanged()
        {
            Refresh();
        }

        private static void CreateProgressBar(TMP_FontAsset font, GameObject parent, (int, int) achievementsProgress)
        {
            GameObject progressRowObject = new GameObject("ProgressRow", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            progressRowObject.transform.SetParent(parent.transform, false);

            VerticalLayoutGroup progressRowHorizontalLayout = progressRowObject.GetComponent<VerticalLayoutGroup>();
            progressRowHorizontalLayout.childControlWidth = true;
            progressRowHorizontalLayout.childControlHeight = true;
            progressRowHorizontalLayout.childForceExpandWidth = false;
            progressRowHorizontalLayout.childForceExpandHeight = false;
            progressRowHorizontalLayout.spacing = 26f;
            progressRowHorizontalLayout.childAlignment = TextAnchor.MiddleCenter;

            LayoutElement progressRowLayout = progressRowObject.GetComponent<LayoutElement>();
            progressRowLayout.preferredHeight = 24f;

            GameObject progressNumberObject = new GameObject("ProgressNumber", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            progressNumberObject.transform.SetParent(progressRowObject.transform, false);

            int unlockPrecentage =  achievementsProgress.Item2 > 0 ? (int)Math.Round((double)achievementsProgress.Item1 / achievementsProgress.Item2 * 100) : 0;

            TextMeshProUGUI progressNumberTmp = progressNumberObject.GetComponent<TextMeshProUGUI>();
            progressNumberTmp.text = $"{unlockPrecentage}%";
            progressNumberTmp.fontSize = 18;
            progressNumberTmp.color = Color.white;
            progressNumberTmp.font = font;

            GameObject squaresObject = new GameObject("ProgressSquares", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            squaresObject.transform.SetParent(progressRowObject.transform, false);

            HorizontalLayoutGroup squaresHorizontalLayout = squaresObject.GetComponent<HorizontalLayoutGroup>();
            squaresHorizontalLayout.spacing = 4f;
            squaresHorizontalLayout.childControlWidth = true;
            squaresHorizontalLayout.childControlHeight = true;
            squaresHorizontalLayout.childForceExpandWidth = false;
            squaresHorizontalLayout.childForceExpandHeight = false;
            squaresHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;

            LayoutElement squaresLayout = squaresObject.GetComponent<LayoutElement>();
            squaresLayout.preferredHeight = 24f;
            squaresLayout.minHeight = 24f;

            int filledSquares = achievementsProgress.Item2 > 0 ? Mathf.FloorToInt(((float)achievementsProgress.Item1 / achievementsProgress.Item2) * 10) : 0;
            for (int i = 0; i < 10; i++)
            {
                GameObject squareObject = new GameObject($"Square{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                squareObject.transform.SetParent(squaresObject.transform, false);

                LayoutElement squareLayout = squareObject.GetComponent<LayoutElement>();
                squareLayout.preferredWidth = 24f;
                squareLayout.preferredHeight = 24f;
                squareLayout.minWidth = 24f;
                squareLayout.minHeight = 24f;

                Image squareImage = squareObject.GetComponent<Image>();
                squareImage.color = i < filledSquares ? Color.white : new Color(1f, 1f, 1f, 0.1f);
            }
        }

        private void RefreshText()
        {
            (int, int) achievementsProgress = AchievementRegistry.GetTotalProgress();

            totalUnlockedText.text = $"{achievementsProgress.Item1}/{achievementsProgress.Item2}";
            unlockedText.text = $"{achievementsProgress.Item1}";
            lockedText.text = $"{achievementsProgress.Item2 - achievementsProgress.Item1}";
        }

        public void Refresh()
        {
           
            RefreshPluginContainer(pluginContainer.transform, fontAsset);
            RefreshText();
        }

        public void SetVisible()
        {
            isVisible = !isVisible;

            StopAllCoroutines();
            StartCoroutine(Animate(isVisible ? shownPos : hiddenPos));
        }

        private IEnumerator Animate(Vector2 target)
        {
            yield return AnimationUtility.SlideTo(target, statsPanelRect);
        }

        private void OnDestroy()
        {
            AchievementEvents.AchievementsChanged -= OnAchievementsChanged;
        }
    }
}