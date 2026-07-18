using System.Collections.Generic;

namespace CUAchievements.Persistence
{
    internal sealed class AchievementSaveFile
    {
        public int Version { get; set; } = 1;

        public string PluginGuid { get; set; }

        public Dictionary<string, AchievementProgressData> Achievements { get; set; }
    }
}
