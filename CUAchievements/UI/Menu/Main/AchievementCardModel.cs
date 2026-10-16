using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using UnityEngine;

namespace CUAchievements.UI.Menu.Main
{
    internal sealed class AchievementCardModel
    {
        public string Title;
        public string Description;
        public string ShortDescription;
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
                Description = def.Description,
                ShortDescription = def.ShortDescription,
                Icon = a.IsUnlocked ? def.Icon : def.LockedIcon,
                Frame = AchievementFrames.Get(def.Difficulty),
                Alpha = a.IsUnlocked ? 1f : 0.35f,
                Source = a
            };

            if (def.Type == AchievementType.Counter)
            {
                //var value = def.Scope == AchievementScope.Run && !a.IsUnlocked && a.Progress.LastRunProgress > 0
                //    ? a.Progress.LastRunProgress
                //    : a.Progress.Progress;

                var value = a.Progress.Progress;
                if (a.IsUnlocked) 
                    value = def.MaxProgress;
                else if (def.Scope == AchievementScope.Run && a.Progress.LastRunProgress > 0)
                    value = a.Progress.LastRunProgress;

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
            Title = string.Format(LocaleRegistry.Get("achievements_panel_hidden_achievements_title",
            "Hidden achievements remaining: {0}"), count),
            ShortDescription = LocaleRegistry.Get("achievements_panel_hidden_achievements_description",
            "Information about each achievement will be available after it is obtained"),
            Icon = AssetLoader.GetCachedSprite("achievement_default_hidden"),
            Frame = AchievementFrames.Get(AchievementDifficulty.Normal),
            Alpha = 0.35f
        };

        internal static AchievementCardModel Debug() => new()
        {
            Title = LocaleRegistry.Get("achievement_debug_title", "Thanks for using CUAchievements!"),
            ShortDescription = LocaleRegistry.Get("achievement_debug_description", "oh my god! Hello!! Hi!!! hii!!! HEEELlOOO!!!"),
            Icon = AssetLoader.GetCachedSprite("achievement_preview"),
            Frame = AchievementFrames.Get(AchievementDifficulty.Normal)
        };
    }
}