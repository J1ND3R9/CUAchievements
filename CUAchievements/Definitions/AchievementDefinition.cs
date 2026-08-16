using CUCoreLib.Helpers;
using UnityEngine;
using static ItemInfo;

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
        public Sprite Sprite { get; private set; }
        public Sprite LockedSprite { get; private set; }
        public bool IsHidden { get; private set; }
        public float MaxProgress { get; private set; }

        private AchievementDefinition() { }

        private static AchievementDefinition CreateBase(string id, string title, string description, Sprite sprite, Sprite lockedSprite, bool hidden)
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
                LockedSprite = lockedSprite,
                IsHidden = hidden
            };
        }
        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Counter"/>
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="title">Displayed achievement title.</param>
        /// <param name="description">Displayed achievement description.</param>
        /// <param name="sprite">Displayed achievement icon. If null, the default sprite will be used.</param>
        /// <param name="maxProgress">The progress required for the achievement to unlock.</param>
        /// <param name="lockedSprite">The icon displayed for a locked achievement.</param>
        /// <param name="hidden">Not yet implemented... for now.</param>
        /// <returns>
        /// Returns new <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Counter"/>
        /// </returns>
        public static AchievementDefinition Counter(string id, string title, string description, Sprite sprite, float maxProgress, Sprite lockedSprite = null, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, title, description, sprite, lockedSprite, hidden);

            def.MaxProgress = maxProgress;
            def.Type = AchievementType.Counter;

            return def;
        }

        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Instant"/>
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="title">Displayed achievement title.</param>
        /// <param name="description">Displayed achievement description.</param>
        /// <param name="sprite">Displayed achievement icon.</param>
        /// <param name="lockedSprite">The icon displayed for a locked achievement.</param>
        /// <param name="hidden">Not yet implemented... for now.</param>
        /// <returns>
        /// Returns new <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Instant"/>
        /// </returns>
        public static AchievementDefinition Instant(string id, string title, string description, Sprite sprite, Sprite lockedSprite = null, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, title, description, sprite, lockedSprite, hidden);

            def.Type = AchievementType.Instant;

            return def;
        }
    }
}
