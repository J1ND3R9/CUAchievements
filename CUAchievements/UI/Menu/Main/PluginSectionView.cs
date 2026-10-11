using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.Main
{
    internal sealed class PluginSectionView
    {
        public GameObject Root { get; private set; }
        public Transform Body { get; private set; }

        public static PluginSectionView Create(Transform parent, string pluginName, List<RegisteredAchievement> achievements, TMP_FontAsset font)
        {
            int total = achievements.Count;
            int unlocked = achievements.Count(a => a.IsUnlocked);

            GameObject root = UIUtility.Create("AchievementsPluginContainer", parent, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.GetComponent<VerticalLayoutGroup>().Configure(spacing: 12f, expandW: true);

            ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject header = UIUtility.CreatePanel("PluginHeader", root.transform);
            header.AddComponent<VerticalLayoutGroup>().Configure(spacing: 10f,
                expandW: true,
                padding: new RectOffset(12, 12, 12, 12));

            UIUtility.CreateText(header.transform, "Title", $"{pluginName} [{unlocked}/{total}]", font, 18f);
            UIUtility.CreateLineProgress(header.transform, unlocked, total, font);

            GameObject body = UIUtility.Create("Achievements", root.transform, typeof(VerticalLayoutGroup));
            body.GetComponent<VerticalLayoutGroup>().Configure(spacing: 12f, expandW: true);

            header.AddComponent<AchievementPluginUI>().Initialize(body, go => go.SetActive(!go.activeSelf));

            return new PluginSectionView { Root = root, Body = body.transform };
        }

        public static PluginSectionView CreateNoAchievement(Transform parent, TMP_FontAsset font, bool noRegisteredAchievements)
        {
            var noAchievementsTitle = LocaleRegistry.Get("achievements_list_no_results_title", "No achievements found!");
            var noAchievementsDesc = LocaleRegistry.Get("achievements_list_no_results_desc", "We didn't find any achievements with those filters.");

            if (noRegisteredAchievements)
            {
                noAchievementsTitle = LocaleRegistry.Get("achievements_list_no_achievements_title", "No achievements :(");
                noAchievementsDesc = LocaleRegistry.Get("achievements_list_no_achievements_desc", "You don't have any mods installed that add achievements.\nFix this by installing, for example, this mod!"); // TODO: Кликабельная ссылка
            }

            GameObject root = UIUtility.Create("AchievementsPluginContainer", parent, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.GetComponent<VerticalLayoutGroup>().Configure(spacing: 12f, expandW: true);

            ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject header = UIUtility.CreatePanel("PluginHeader", root.transform);
            header.AddComponent<VerticalLayoutGroup>().Configure(spacing: 10f,
                expandW: true,
                align: TextAnchor.MiddleCenter,
                padding: new RectOffset(12, 12, 12, 12));

            UIUtility.CreateText(header.transform, "Title", noAchievementsTitle, font, 24f, TextAlignmentOptions.Center);
            var descTmp = UIUtility.CreateText(header.transform, "Description", noAchievementsDesc, font, 18f, TextAlignmentOptions.Center);
            descTmp.color = new Color(1f, 1f, 1f, 0.7f);

            return new PluginSectionView { Root = root, Body = null };
        }
    }
}