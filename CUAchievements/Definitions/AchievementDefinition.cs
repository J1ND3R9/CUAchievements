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
        /// <summary>
        /// Unique achievement identifier
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Achievement name
        /// </summary>
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ShortDescription { get; private set; }
        public AchievementDifficulty Difficulty { get; private set; }
        public AchievementScope Scope { get; private set; }
        public Sprite Icon { get; private set; }
        public Sprite LockedIcon { get; private set; }
        public Sprite CustomFrame { get; private set; }
        public bool IsHidden { get; private set; }
        public float MaxProgress { get; private set; }
        public int ProgressDecimals { get; private set; }
        public bool CanGetInTutorial { get; private set; }
        private AchievementDefinition() { }

        private static AchievementDefinition CreateBase(string id,
                                                        string title,
                                                        string description,
                                                        string shortDescription,
                                                        AchievementDifficulty rank,
                                                        AchievementScope scope,
                                                        Sprite sprite,
                                                        Sprite lockedSprite,
                                                        Sprite frameOverride,
                                                        bool hidden,
                                                        bool canGetInTutorial)
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
            if (string.IsNullOrWhiteSpace(shortDescription))
            {
                shortDescription = description;
            }
            return new AchievementDefinition
            {
                Id = id,
                Name = title,
                Description = description,
                ShortDescription = shortDescription,
                Difficulty = rank,
                Scope = scope,
                Icon = sprite,
                CustomFrame = frameOverride,
                LockedIcon = lockedSprite,
                IsHidden = hidden,
                CanGetInTutorial = canGetInTutorial
            };
        }
        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Counter"/>.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="name">Displayed achievement name.</param>
        /// <param name="description">Displayed achievement description. If the description exceeds 90 characters, it will not fit in the pop-up or the achievement list. Use <paramref name="shortDescription"/>.</param>
        /// <param name="icon">Displayed achievement icon. If null, the default sprite will be used.</param>
        /// <param name="difficulty">Achievement difficulty.Affects the border around the achievement in the list.</param>
        /// <param name="scope">Does the achievement have to be earned during a single run, or does progress count across runs.</param>
        /// <param name="maxProgress">The progress required for the achievement to unlock.</param>
        /// <param name="shortDescription">A short description of the achievement. It will be used in the pop-up and the achievement list.</param>
        /// <param name="lockedIcon">Overrides the locked achievement icon.</param>
        /// <param name="customFrame">Add a custom frame around the achievement. Make sure to use 9-slicing.</param>
        /// <param name="hidden">If true, the achievement's name, description, and icon will be hidden from the player until the achievement is unlocked.</param>
        /// <param name="canGetInTutorial">If true, the achievement can be obtained in the tutorial or sandbox.</param>
        /// <param name="progressDecimals">Number of decimal places. Displayed in the achievements list.</param>
        /// <remarks>
        /// If <paramref name="hidden"/> is true, <paramref name="lockedIcon"/> and <paramref name="customFrame"/> are ignored until the achievement is unlocked.
        /// </remarks>
        public static AchievementDefinition Counter(string id,
                                                    string name,
                                                    string description,
                                                    float maxProgress,
                                                    Sprite icon,
                                                    string shortDescription = "",
                                                    Sprite lockedIcon = null,
                                                    AchievementDifficulty difficulty = AchievementDifficulty.Normal,
                                                    AchievementScope scope = AchievementScope.Lifetime,
                                                    Sprite customFrame = null,
                                                    bool hidden = false,
                                                    bool canGetInTutorial = false,
                                                    int progressDecimals = 0)
        {
            AchievementDefinition def = CreateBase(id, name, description, shortDescription, difficulty, scope, icon, lockedIcon, customFrame, hidden, canGetInTutorial);

            def.MaxProgress = maxProgress;
            def.Type = AchievementType.Counter;
            def.ProgressDecimals = progressDecimals;

            return def;
        }

        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Instant"/>.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="name">Displayed achievement name.</param>
        /// <param name="description">Displayed achievement description. If the description exceeds 90 characters, it will not fit in the pop-up or the achievement list. Use <paramref name="shortDescription"/>.</param>
        /// <param name="icon">Displayed achievement icon.</param>
        /// <param name="shortDescription">A short description of the achievement. It will be used in the pop-up and the achievement list.</param>
        /// <param name="difficulty">Achievement difficulty.Affects the border around the achievement in the list.</param>
        /// <param name="scope">Does the achievement have to be earned during a single run, or does progress count across runs.</param>
        /// <param name="lockedIcon">The icon displayed for a locked achievement.</param>
        /// <param name="customFrame">Add a custom frame around the achievement. Make sure to use 9-slicing.</param>
        /// <param name="hidden">If true, the achievement's title, description, and sprite are hidden from the player until unlocked.</param>
        /// <param name="canGetInTutorial">If true, the achievement can be obtained in the tutorial or sandbox.</param>
        /// <remarks>
        /// If <paramref name="hidden"/> is true, <paramref name="lockedIcon"/> and <paramref name="customFrame"/> are ignored until the achievement is unlocked.
        /// </remarks>
        public static AchievementDefinition Instant(string id,
                                                    string name,
                                                    string description,
                                                    Sprite icon,
                                                    string shortDescription = "",
                                                    Sprite lockedIcon = null,
                                                    AchievementDifficulty difficulty = AchievementDifficulty.Normal,
                                                    AchievementScope scope = AchievementScope.Lifetime,
                                                    Sprite customFrame = null,
                                                    bool hidden = false,
                                                    bool canGetInTutorial = false)
        {
            AchievementDefinition def = CreateBase(id, name, description, shortDescription, difficulty, scope, icon, lockedIcon, customFrame, hidden, canGetInTutorial);

            def.Type = AchievementType.Instant;
            return def;
        }
    }
}