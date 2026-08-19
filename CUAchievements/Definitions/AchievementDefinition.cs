using CUCoreLib.Helpers;
using UnityEngine;

namespace CUAchievements.Definitions
{
    /// <summary>
    /// Represents an achievement definition registered by a plugin.
    /// </summary>
    public sealed class AchievementDefinition
    {
        public AchievementType Type { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public AchievementDifficulty Difficulty { get; private set; }
        public Sprite Sprite { get; private set; }
        public Sprite LockedSprite { get; private set; }
        public Sprite FrameOverride { get; private set; }
        public bool IsHidden { get; private set; }
        public float MaxProgress { get; private set; }

        private AchievementDefinition() { }

        private static AchievementDefinition CreateBase(string id,
                                                        string title,
                                                        string description,
                                                        AchievementDifficulty rank,
                                                        Sprite sprite,
                                                        Sprite lockedSprite,
                                                        Sprite frameOverride,
                                                        bool hidden)
        {
            if (sprite == null)
            {
                CUAchievementsPlugin.Logger.LogWarning($"Achievement sprite with ID '{id}' is null. Using default sprite.");
                sprite = AssetLoader.GetCachedSprite("achievement_default_sprite");
            }
            if (lockedSprite == null || hidden)
            {
                lockedSprite = AssetLoader.GetCachedSprite("achievement_default_locked");
            }
            return new AchievementDefinition
            {
                Id = id,
                Title = title,
                Description = description,
                Difficulty = rank,
                Sprite = sprite,
                FrameOverride = frameOverride,
                LockedSprite = lockedSprite,
                IsHidden = hidden
            };
        }
        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Counter"/>.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="title">Displayed achievement title.</param>
        /// <param name="description">Displayed achievement description.</param>
        /// <param name="sprite">Displayed achievement icon. If null, the default sprite will be used.</param>
        /// <param name="difficulty">Achievement difficulty.Affects the border around the achievement in the list.</param>
        /// <param name="maxProgress">The progress required for the achievement to unlock.</param>
        /// <param name="lockedSprite">The icon displayed for a locked achievement.</param>
        /// <param name="frameOverride">Add a custom frame around the achievement. Make sure to use 9-slicing.</param>
        /// <param name="hidden">If true, the achievement's title, description, and sprite are hidden from the player until unlocked.</param>
        /// <remarks>
        /// If <paramref name="hidden"/> is true, <paramref name="lockedSprite"/> and <paramref name="frameOverride"/> are ignored until the achievement is unlocked.
        /// </remarks>
        public static AchievementDefinition Counter(string id,
                                                    string title,
                                                    string description,
                                                    float maxProgress,
                                                    Sprite sprite,
                                                    Sprite lockedSprite = null,
                                                    AchievementDifficulty difficulty = AchievementDifficulty.Normal,
                                                    Sprite frameOverride = null,
                                                    bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, title, description, difficulty, sprite, lockedSprite, frameOverride, hidden);

            def.MaxProgress = maxProgress;
            def.Type = AchievementType.Counter;

            return def;
        }

        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Instant"/>.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="title">Displayed achievement title.</param>
        /// <param name="description">Displayed achievement description.</param>
        /// <param name="sprite">Displayed achievement icon.</param>
        /// <param name="difficulty">Achievement difficulty.Affects the border around the achievement in the list.</param>
        /// <param name="lockedSprite">The icon displayed for a locked achievement.</param>
        /// <param name="frameOverride">Add a custom frame around the achievement. Make sure to use 9-slicing.</param>
        /// <param name="hidden">If true, the achievement's title, description, and sprite are hidden from the player until unlocked.</param>
        /// <remarks>
        /// If <paramref name="hidden"/> is true, <paramref name="lockedSprite"/> and <paramref name="frameOverride"/> are ignored until the achievement is unlocked.
        /// </remarks>
        public static AchievementDefinition Instant(string id,
                                                    string title,
                                                    string description,
                                                    Sprite sprite,
                                                    Sprite lockedSprite = null,
                                                    AchievementDifficulty difficulty = AchievementDifficulty.Normal,
                                                    Sprite frameOverride = null,
                                                    bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, title, description, difficulty, sprite, lockedSprite, frameOverride, hidden);

            def.Type = AchievementType.Instant;
            return def;
        }
    }
}
