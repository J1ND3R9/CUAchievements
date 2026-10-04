using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu
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

            root.AddComponent<AchievementPluginUI>().Initialize(body, go => go.SetActive(!go.activeSelf)); // FIXME: Работает и при нажатии на блок скрытых достижений.

            return new PluginSectionView { Root = root, Body = body.transform };
        }
    }
}