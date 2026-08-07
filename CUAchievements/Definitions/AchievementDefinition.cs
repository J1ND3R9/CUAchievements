namespace CUAchievements.Definitions
{
    public sealed class AchievementDefinition
    {
        public AchievementType Type { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
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

        public static AchievementDefinition Counter(string id, string name, string description, string icon, float maxProgress, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, name, description, icon, hidden);

            def.MaxProgress = maxProgress;
            def.Type = AchievementType.Counter;

            return def;
        }
        public static AchievementDefinition Instant(string id, string name, string description, string icon, bool hidden = false)
        {
            AchievementDefinition def = CreateBase(id, name, description, icon, hidden);

            def.Type = AchievementType.Instant;

            return def;
        }
    }
}
