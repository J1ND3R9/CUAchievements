namespace CUAchievements.Definitions
{
    /// <summary>
    /// Represents an achievement definition registered by a plugin.
    /// </summary>
    public sealed class AchievementDefinition
    {
        public AchievementType Type { get; private set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Icon { get; private set; }
        public bool Hidden { get; private set; }
        public float MaxProgress { get; private set; }

        private AchievementDefinition() { }

        private static AchievementDefinition CreateBase(string id, string name, string description, string icon, bool hidden)
        {
            return new AchievementDefinition
            {
                Id = id,
                Name = name,
                Description = description,
                Icon = icon,
                Hidden = hidden
            };
        }
        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Counter"/>
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="name">Displayed achievement name.</param>
        /// <param name="description">Displayed description name.</param>
        /// <param name="icon">Not yet implemented... for now.</param>
        /// <param name="maxProgress">The progress required for the achievement to unlock.</param>
        /// <param name="hidden">Not yet implemented... for now.</param>
        /// <returns>
        /// Returns new <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Counter"/>
        /// </returns>
        public static AchievementDefinition Counter(string id, string name, string description, string icon, float maxProgress, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, name, description, icon, hidden);

            def.MaxProgress = maxProgress;
            def.Type = AchievementType.Counter;

            return def;
        }

        /// <summary>
        /// Create the <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Instant"/>
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="name">Displayed achievement name.</param>
        /// <param name="description">Displayed description name.</param>
        /// <param name="icon">Not yet implemented... for now.</param>
        /// <param name="hidden">Not yet implemented... for now.</param>
        /// <returns>
        /// Returns new <see cref="AchievementDefinition"/> with type <see cref="AchievementType.Instant"/>
        /// </returns>
        public static AchievementDefinition Instant(string id, string name, string description, string icon, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, name, description, icon, hidden);

            def.Type = AchievementType.Instant;

            return def;
        }
    }
}
