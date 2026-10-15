using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
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

            GameObject upperObject = UIUtility.Create("UpperHeader", header.transform, typeof(HorizontalLayoutGroup));
            upperObject.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 4f);

            var titleTmp = UIUtility.CreateText(upperObject.transform, "Title", $"{pluginName} [{unlocked}/{total}]", font, 18f);
            titleTmp.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            GameObject arrow = UIUtility.Create("Arrow", upperObject.transform, typeof(Image), typeof(LayoutElement));

            Image arrowImage = arrow.GetComponent<Image>();
            arrowImage.sprite = AssetLoader.GetCachedSprite("statusarrow");

            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(20f, 20f);
            arrowRect.eulerAngles = new Vector3(0f, 0f, 270f);

            LayoutElement arrowLayout = arrow.GetComponent<LayoutElement>();
            arrowLayout.minHeight = 20f;
            arrowLayout.minWidth = 20f;
            arrowLayout.preferredHeight = 20f;
            arrowLayout.preferredWidth = 20f;

            UIUtility.CreateLineProgress(header.transform, unlocked, total, font);

            GameObject body = UIUtility.Create("Achievements", root.transform, typeof(VerticalLayoutGroup));
            body.GetComponent<VerticalLayoutGroup>().Configure(spacing: 12f, expandW: true);    

            header.AddComponent<AchievementPluginUI>().Initialize(body, go =>
            {
                go.SetActive(!go.activeSelf);
                arrowRect.eulerAngles = go.activeSelf ? new Vector3(0f, 0f, 270f) : new Vector3(0f, 0f, 90f);
            });

            return new PluginSectionView { Root = root, Body = body.transform };
        }

        public static PluginSectionView CreateNoAchievement(Transform parent, TMP_FontAsset font, bool noRegisteredAchievements)
        {
            var noAchievementsTitle = LocaleRegistry.Get("achievements_list_no_results_title", "No achievements found!");
            var noAchievementsDesc = LocaleRegistry.Get("achievements_list_no_results_desc", "We didn't find any achievements with those filters.");

            if (noRegisteredAchievements)
            {
                noAchievementsTitle = LocaleRegistry.Get("achievements_list_no_achievements_title", "No achievements :(");
                noAchievementsDesc = LocaleRegistry.Get("achievements_list_no_achievements_desc", "You don't have any mods installed that add achievements.\nFix this by installing, for example, <color=\"yellow\">Floydba's Achievements</color>!"); // TODO: Кликабельная ссылка
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