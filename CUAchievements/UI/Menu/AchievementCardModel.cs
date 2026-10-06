using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using UnityEngine;

namespace CUAchievements.UI.Menu
{
    internal sealed class AchievementCardModel
    {
        public string Title;
        public string Description;
        public Sprite Icon;
        public Sprite Frame;
        public float Alpha = 1f;
        public CounterInfo Counter;
        public RegisteredAchievement Source;

        internal sealed class CounterInfo
        {
            public string Text;
            public int FilledSquares;
        }

        public static AchievementCardModel From(RegisteredAchievement a)
        {
            var def = a.Definition;
            var model = new AchievementCardModel
            {
                Title = def.Name,
                Description = def.ShortDescription,
                Icon = a.IsUnlocked ? def.Icon : def.LockedIcon,
                Frame = AchievementFrames.Get(def.Difficulty),
                Alpha = a.IsUnlocked ? 1f : 0.35f,
                Source = a
            };

            if (def.Type == AchievementType.Counter)
            {
                var value = def.Scope == AchievementScope.Run && !a.IsUnlocked
                    ? a.Progress.LastRunProgress
                    : a.Progress.Progress;

                string format = $"n{def.ProgressDecimals}";
                model.Counter = new CounterInfo
                {
                    Text = $"{value.ToString(format)}/{def.MaxProgress.ToString(format)}",
                    FilledSquares = def.MaxProgress > 0
                    ? Mathf.Clamp(Mathf.FloorToInt((float)value / def.MaxProgress * 10), 0, 10)
                    : 0
                };
            }

            return model;
        }

        public static AchievementCardModel Hidden(int count) => new()
        {
            Title = string.Format(LocaleRegistry.Get("achievementspanel_hidden_achievements_title",
            "Hidden achievements remaining: {0}"), count),
            Description = LocaleRegistry.Get("achievementspanel_hidden_achievements_description",
            "Information about each achievement will be available after it is obtained"),
            Icon = AssetLoader.GetCachedSprite("achievement_default_hidden"),
            Frame = AchievementFrames.Get(AchievementDifficulty.Normal),
            Alpha = 0.35f
        };
    }
}