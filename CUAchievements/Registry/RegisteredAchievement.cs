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

        /// <summary>
        /// Adds progress to a registered achievement.
        /// </summary>
        /// <param name="amount">Amount of progress to add. Default is 1.</param>
        /// <returns>
        /// True if the progress was added successfully; otherwise false.
        /// </returns>
        public bool AddProgress(float amount = 1)
        {
            return AchievementService.AddProgressInternal(this, amount);
        }

        /// <summary>
        /// Unlock a registered achievement.
        /// </summary>
        /// <returns>
        /// True if the unlock was successfully; otherwise false.
        /// </returns>
        public bool Unlock()
        {
            return AchievementService.UnlockInternal(this);
        }
    }
}
