using CUAchievements.Registry;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CUAchievements.UI.Menu
{
    internal sealed class AchievementListView
    {
        private readonly RectTransform content;
        private readonly TMP_FontAsset font;
        private readonly Action<RegisteredAchievement> onSelected;
        private readonly List<GameObject> sections = [];

        public AchievementListView(RectTransform content, TMP_FontAsset font, Action<RegisteredAchievement> onSelected)
        {
            this.content = content;
            this.font = font;
            this.onSelected = onSelected;
        }
        public void BuildEmptyNote(bool emptyRegister)
        {
            Clear();
            BuildEmptySection(emptyRegister);
        }
        public void Rebuild(IReadOnlyDictionary<string, List<RegisteredAchievement>> perPlugin)
        {
            Clear();
            foreach (KeyValuePair<string, List<RegisteredAchievement>> pair in perPlugin)
            {
                if (pair.Value.Count == 0) continue;
                BuildSection(pair.Key, AchievementSorter.Sort(pair.Value));
            }
        }

        private void Clear()
        {
            foreach (GameObject s in sections)
            {
                s.SetActive(false);
                UnityEngine.Object.Destroy(s);
            }
            sections.Clear();
        }


        private void BuildSection(string pluginName, List<RegisteredAchievement> sorted)
        {
            PluginSectionView section = PluginSectionView.Create(content, pluginName, sorted, font);
            sections.Add(section.Root);

            List<RegisteredAchievement> hidden = new List<RegisteredAchievement>(0);
            foreach (RegisteredAchievement a in sorted)
            {
                if (!a.IsUnlocked && a.Definition.IsHidden)
                {
                    hidden.Add(a);
                    continue;
                }
                AchievementCardView.Create(section.Body, AchievementCardModel.From(a), font, onSelected);
            }
            if (hidden.Count > 0)
                AchievementCardView.Create(section.Body, AchievementCardModel.Hidden(hidden.Count), font);
        }

        private void BuildEmptySection(bool emptyRegister)
        {
            PluginSectionView section = PluginSectionView.CreateNoAchievement(content, font, emptyRegister);
            sections.Add(section.Root);
        }
    }
}