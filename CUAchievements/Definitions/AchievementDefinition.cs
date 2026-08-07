using CUCoreLib.Helpers;
using UnityEngine;

namespace CUAchievements.Definitions
{
    public sealed class AchievementDefinition
    {
        public AchievementType Type { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public Sprite Sprite { get; private set; }
        public bool Hidden { get; private set; }
        public float MaxProgress { get; private set; }

        private AchievementDefinition() { }

        private static AchievementDefinition CreateBase(string id, string title, string description, Sprite sprite, bool hidden)
        {
            if (sprite == null)
            {
                CUAchievementsPlugin.Logger.LogWarning($"Achievement sprite with ID '{id}' is null. Using default sprite.");
                sprite = AssetLoader.GetCachedSprite("achievement_default_sprite");
            }
            return new AchievementDefinition
            {
                Id = id,
                Title = title,
                Description = description,
                Sprite = sprite,
                Hidden = hidden
            };
        }

        public static AchievementDefinition Counter(string id, string title, string description, Sprite sprite, float maxProgress, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, title, description, sprite, hidden);

            def.MaxProgress = maxProgress;
            def.Type = AchievementType.Counter;

            return def;
        }
        public static AchievementDefinition Instant(string id, string name, string description, Sprite sprite, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, name, description, sprite, hidden);

            def.Type = AchievementType.Instant;

            return def;
        }
    }
}
