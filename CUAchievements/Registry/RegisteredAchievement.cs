using CUAchievements.Definitions;
using CUAchievements.Persistence;

namespace CUAchievements.Registry
{
    /// <summary>
    /// Represents an registered achievement, registered by a plugin.
    /// </summary>
    public sealed class RegisteredAchievement
    {
        public string PluginGuid { get; }

        public AchievementDefinition Definition { get; }

        public AchievementProgressData Progress { get; set; }

        internal RegisteredAchievement(string pluginGuid, AchievementDefinition definition, AchievementProgressData progress)
        {
            PluginGuid = pluginGuid;
            Definition = definition;
            Progress = progress;
        }
    }
}
