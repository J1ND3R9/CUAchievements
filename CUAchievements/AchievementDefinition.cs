using CUAchievements.Options;
using CUAchievements.Plugin;
using CUAchievements.Registry;
using CUCoreLib.Helpers;
using UnityEngine;

namespace CUAchievements
{
    /// <summary>
    /// Describes an achievement: its identity, texts, icons, type and rules.
    /// </summary>
    /// <remarks>
    /// Create instances with <see cref="Instant"/> or <see cref="Counter"/> and pass them to
    /// <see cref="AchievementsAPI.Register(AchievementDefinition)"/>. A definition is read-only once created.
    /// </remarks>
    public sealed class AchievementDefinition
    {
        /// <summary>
        /// How this achievement is earned. See <see cref="AchievementType"/> for the behaviour of each kind.
        /// </summary>
        public AchievementType Type { get; private set; }
        /// <summary>
        /// Identifier of the achievement, unique within your plugin.
        /// </summary>
        /// <remarks>
        /// Together with the plugin GUID it forms the key under which the player's progress is saved.
        /// Do not change it after release, otherwise players will lose their progress.
        /// </remarks>
        public string Id { get; private set; }
        /// <summary>
        /// Display name of the achievement.
        /// </summary>
        /// <remarks>
        /// Resolved with <see cref="LocalizedText.Resolve"/> every time it is read,
        /// so it always reflects the currently active language.
        /// </remarks>
        public string Name => NameText.Resolve();
        private LocalizedText NameText;
        /// <summary>
        /// Full description of the achievement, shown in the details panel.
        /// </summary>
        /// <remarks>
        /// Resolved with <see cref="LocalizedText.Resolve"/> every time it is read,
        /// so it always reflects the currently active language.
        /// </remarks>
        public string Description => DescriptionText.Resolve();
        private LocalizedText DescriptionText;
        /// <summary>
        /// Short description of the achievement, shown in the achievements list.
        /// Equals <see cref="Description"/> if no short description was provided.
        /// </summary>
        /// <remarks>
        /// Resolved with <see cref="LocalizedText.Resolve"/> every time it is read,
        /// so it always reflects the currently active language.
        /// </remarks>
        public string ShortDescription => ShortDescriptionText.Resolve();
        private LocalizedText ShortDescriptionText;
        /// <summary>
        /// Subjective difficulty of the achievement. Selects the default frame drawn around it.
        /// </summary>
        /// <remarks>
        /// <see cref="CustomFrame"/>, if set, replaces the frame regardless of the difficulty.
        /// </remarks>
        public AchievementDifficulty Difficulty { get; private set; }
        /// <summary>
        /// When the progress and conditions of this achievement are reset.
        /// See <see cref="AchievementScope"/> for the behaviour of each kind.
        /// </summary>
        public AchievementScope Scope { get; private set; }
        /// <summary>
        /// Icon shown once the achievement is unlocked.
        /// </summary>
        /// <remarks>
        /// Never <see langword="null"/>: if no icon was provided, a default placeholder is used
        /// and a warning is logged.
        /// </remarks>
        public Sprite Icon { get; private set; }
        /// <summary>
        /// Icon shown while the achievement is locked.
        /// </summary>
        /// <remarks>
        /// Never <see langword="null"/>: the default locked icon is used when none was provided
        /// and always for hidden achievements.
        /// </remarks>
        public Sprite LockedIcon { get; private set; }
        /// <summary>
        /// Frame sprite that replaces the frame normally chosen by <see cref="Difficulty"/>.
        /// </summary>
        /// <remarks>
        /// Has the highest priority. <see langword="null"/> means the default frame is used.
        /// </remarks>
        public Sprite CustomFrame { get; private set; }
        /// <summary>
        /// Whether the achievement is hidden. Hidden achievements do not show their name, description
        /// or icon to the player until they are unlocked or revealed.
        /// </summary>
        public bool IsHidden { get; private set; }
        /// <summary>
        /// The progress required to unlock the achievement.
        /// </summary>
        /// <remarks>
        /// Only meaningful for <see cref="AchievementType.Counter"/>. For <see cref="AchievementType.Instant"/>
        /// it is 0.
        /// </remarks>
        public float MaxProgress { get; private set; }
        /// <summary>
        /// Number of digits after the decimal point used when the progress is displayed.
        /// </summary>
        /// <remarks>
        /// Only meaningful for <see cref="AchievementType.Counter"/>.
        /// </remarks>
        public int ProgressDecimals { get; private set; }
        /// <summary>
        /// Whether the achievement can be earned during the tutorial.
        /// </summary>
        public bool CanGetInTutorial { get; private set; }
        private AchievementDefinition() { }

        private static AchievementDefinition CreateBase(string id,
                                                        LocalizedText title,
                                                        LocalizedText description,
                                                        LocalizedText shortDescription,
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
                Log.Warn($"Achievement sprite with ID '{id}' is null. Using default sprite.");
                sprite = AssetLoader.GetCachedSprite("achievement_default_sprite");
            }
            if (lockedSprite == null)
            {
                lockedSprite = AssetLoader.GetCachedSprite("achievement_default_locked");
            }
            if (shortDescription.IsEmpty)
            {
                shortDescription = description;
            }
            return new AchievementDefinition
            {
                Id = id,
                NameText = title,
                DescriptionText = description,
                ShortDescriptionText = shortDescription,
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
        /// Creates a definition of type <see cref="AchievementType.Counter"/>: an achievement earned by
        /// accumulating progress until <paramref name="maxProgress"/> is reached.
        /// </summary>
        /// <param name="id">Identifier unique within your plugin. Do not change it after release: saved progress is keyed by it.</param>
        /// <param name="name">Display name.</param>
        /// <param name="description">Full description, shown in the details panel.</param>
        /// <param name="maxProgress">The progress value at which the achievement unlocks.</param>
        /// <param name="icon">Icon shown once unlocked. If <see langword="null"/>, a default icon is used and a warning is logged.</param>
        /// <param name="shortDescription">Short text for the achievements list. If omitted, <paramref name="description"/> is used.</param>
        /// <param name="lockedIcon">Icon shown while locked. If <see langword="null"/>, the default locked icon is used.</param>
        /// <param name="difficulty">Difficulty, which selects the default frame.</param>
        /// <param name="scope">When progress and conditions are reset. See <see cref="AchievementScope"/>.</param>
        /// <param name="customFrame">Frame sprite that replaces the one chosen by <paramref name="difficulty"/>.</param>
        /// <param name="hidden">If <see langword="true"/>, the player sees no details of the achievement until it is unlocked.</param>
        /// <param name="canGetInTutorial">Whether the achievement can be earned during the tutorial.</param>
        /// <param name="progressDecimals">Number of digits after the decimal point when the progress is displayed.</param>
        /// <returns>A new <see cref="AchievementDefinition"/>.</returns>
        /// <remarks>
        /// If <paramref name="hidden"/> is <see langword="true"/> and <see cref="AchievementConfig.RevealableHiddenAchievements"/> is <see langword="false"/>, <paramref name="lockedIcon"/> is ignored and
        /// <paramref name="customFrame"/> is not shown until the achievement is unlocked.
        /// </remarks>
        /// <example>
        /// <code>
        /// api.Register(AchievementDefinition.Counter(
        ///     id: "walk_10000",
        ///     name: "Marathon",
        ///     description: "Walk 10,000 steps.",
        ///     maxProgress: 10000,
        ///     icon: marathonIcon));
        /// </code>
        /// </example>
        public static AchievementDefinition Counter(string id,
                                                    LocalizedText name,
                                                    LocalizedText description,
                                                    float maxProgress,
                                                    Sprite icon,
                                                    LocalizedText shortDescription = default,
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
        /// Creates a definition of type <see cref="AchievementType.Instant"/>: an achievement earned in a single
        /// step by calling <see cref="RegisteredAchievement.Unlock"/>.
        /// </summary>
        /// <param name="id">Identifier unique within your plugin. Do not change it after release: saved progress is keyed by it.</param>
        /// <param name="name">Display name.</param>
        /// <param name="description">Full description, shown in the details panel.</param>
        /// <param name="icon">Icon shown once unlocked. If <see langword="null"/>, a default icon is used and a warning is logged.</param>
        /// <param name="shortDescription">Short text for the achievements list. If omitted, <paramref name="description"/> is used.</param>
        /// <param name="lockedIcon">Icon shown while locked. If <see langword="null"/>, the default locked icon is used.</param>
        /// <param name="difficulty">Difficulty, which selects the default frame.</param>
        /// <param name="scope">When conditions are reset. See <see cref="AchievementScope"/>.</param>
        /// <param name="customFrame">Frame sprite that replaces the one chosen by <paramref name="difficulty"/>.</param>
        /// <param name="hidden">If <see langword="true"/>, the player sees no details of the achievement until it is unlocked.</param>
        /// <param name="canGetInTutorial">Whether the achievement can be earned during the tutorial.</param>
        /// <returns>A new <see cref="AchievementDefinition"/>.</returns>
        /// <remarks>
        /// If <paramref name="hidden"/> is <see langword="true"/> and <see cref="AchievementConfig.RevealableHiddenAchievements"/> is <see langword="false"/>, <paramref name="lockedIcon"/> is ignored and
        /// <paramref name="customFrame"/> is not shown until the achievement is unlocked.
        /// </remarks>
        /// <example>
        /// <code>
        /// api.Register(AchievementDefinition.Instant(
        ///     id: "first_blood",
        ///     name: new LocalizedText("achievement_first_blood_name", "First blood"),
        ///     description: new LocalizedText("achievement_first_blood_desc", "Defeat your first enemy."),
        ///     icon: firstBloodIcon,
        ///     hidden: true));
        /// </code>
        /// </example>
        public static AchievementDefinition Instant(string id,
                                                    LocalizedText name,
                                                    LocalizedText description,
                                                    Sprite icon,
                                                    LocalizedText shortDescription = default,
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