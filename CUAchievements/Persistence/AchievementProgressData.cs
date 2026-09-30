using System;

namespace CUAchievements.Persistence
{
    /// <summary>
    /// Represents an achievement progress registered by a plugin.
    /// </summary>
    public sealed class AchievementProgressData
    {
        public string Id { get; set; }
        public float Progress { get; set; }
        internal float LastRunProgress { get; set; }
        public bool Unlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }

        public AchievementProgressData(string id, float progress = 0, bool unlocked = false, DateTime? unlockedAt = null)
        {
            Id = id;
            Progress = progress;
            Unlocked = unlocked;
            UnlockedAt = unlockedAt;
        }
    }
}
