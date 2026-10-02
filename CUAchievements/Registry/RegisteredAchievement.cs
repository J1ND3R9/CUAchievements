using CUAchievements.Definitions;
using CUAchievements.Persistence;
using CUAchievements.Services;
using System;

namespace CUAchievements.Registry
{
    /// <summary>
    /// Represents an registered achievement, registered by a plugin.
    /// </summary>
    public sealed class RegisteredAchievement
    {
        internal string PluginGuid { get; }
        /// <summary>
        /// Returns <see cref="AchievementDefinition"/>
        /// </summary>
        public AchievementDefinition Definition { get; }
        internal AchievementProgressData Progress { get; }
        /// <summary>
        /// Returns true if the achievement has already been unlocked.
        /// </summary>
        public bool IsUnlocked => Progress.Unlocked;
        /// <summary>
        /// Returns the current progress of the achievement.
        /// </summary>
        public float CurrentProgress => Progress.Progress;
        /// <summary>
        /// Returns the date the achievement was unlocked.
        /// </summary>
        public DateTime? UnlockedAt => Progress.UnlockedAt;
        /// <summary>
        /// Returns/creates a <see cref="ConditionalAchievementTracker"/> instance.
        /// </summary>
        public ConditionalAchievementTracker ConditionalTracker = new ConditionalAchievementTracker();

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
