namespace CUAchievements.Persistence
{
    public sealed class AchievementProgressData
    {
        public string Id { get; set; }
        public float Progress { get; set; }
        public bool Unlocked { get; set; }

        public AchievementProgressData(string id, float progress = 0, bool unlocked = false)
        {
            Id = id;
            Progress = progress;
            Unlocked = unlocked;
        }
    }
}
