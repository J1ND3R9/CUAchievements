using System;
using System.Collections.Generic;

namespace CUAchievements.Persistence
{
    internal sealed class AchievementProgressData
    {
        public string Id { get; set; }
        public float Progress { get; set; }
        internal float LastRunProgress { get; set; }
        public bool Unlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
        public Dictionary<string, bool> Flags { get; set; }

        public AchievementProgressData(string id, float progress = 0, bool unlocked = false, DateTime? unlockedAt = null, Dictionary<string, bool> flags = null)
        {
            Id = id;
            Progress = progress;
            Unlocked = unlocked;
            UnlockedAt = unlockedAt;
            Flags = flags;
        }

        public void Reset()
        {
            Progress = 0;
            LastRunProgress = 0;
            Unlocked = false;
            UnlockedAt = null;
            Flags = null;
        }
    }
}
