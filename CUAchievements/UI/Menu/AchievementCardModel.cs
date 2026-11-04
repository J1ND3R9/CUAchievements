using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using UnityEngine;

namespace CUAchievements.UI.Menu
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
        public AchievementScope Scope;
        public RegisteredAchievement Source;

        internal sealed class CounterInfo
        {
            public string Text;
            public int FilledSquares;
        }

        public static AchievementCardModel From(RegisteredAchievement a)
        {
            AchievementDefinition def = a.Definition;
            AchievementCardModel model = new()
            {
                Title = def.Name,
                Description = def.Description,
                ShortDescription = def.ShortDescription,
                Icon = a.IsUnlocked ? def.Icon : def.LockedIcon,
                Frame = a.Definition.CustomFrame ?? AchievementFrames.Get(def.Difficulty),
                Alpha = a.IsUnlocked ? 1f : 0.35f,
                Scope = a.Definition.Scope,
                Source = a
            };

            if (def.Type == AchievementType.Counter)
            {
                float value = a.ProgressData.Progress;
                if (a.IsUnlocked)
                    value = def.MaxProgress;
                else if (def.Scope == AchievementScope.Run && a.ProgressData.LastRunProgress > 0)
                    value = a.ProgressData.LastRunProgress;

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
            Title = string.Format(UIText.CardHiddenTitle.Resolve(), count),
            ShortDescription = UIText.CardHiddenDescription.Resolve(),
            Icon = AssetLoader.GetCachedSprite("achievement_default_hidden"),
            Frame = AchievementFrames.Get(AchievementDifficulty.Normal),
            Alpha = 0.35f
        };

        internal static AchievementCardModel Greetings() => new()
        {
            Title = UIText.GreetingsAchievementTitle.Resolve(),
            ShortDescription = UIText.GreetingsAchievementDescription.Resolve(),
            Icon = AssetLoader.GetCachedSprite("achievement_preview"),
            Frame = AchievementFrames.Get(AchievementDifficulty.Normal)
        };
    }
}