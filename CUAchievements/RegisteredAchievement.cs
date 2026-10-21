using CUAchievements.Anticheat;
using CUAchievements.Persistence;
using CUAchievements.Services;
using System;

namespace CUAchievements.Registry
{
    /// <summary>
    /// A registered achievement: its <see cref="Definition"/> together with the player's current state
    /// (progress and unlock status).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances are returned by <see cref="AchievementsAPI.Register(AchievementDefinition)"/> and
    /// <see cref="AchievementsAPI.TryGet(string, out RegisteredAchievement)"/>. Use <see cref="AddProgress(float)"/>
    /// and <see cref="Unlock"/> to change the state.
    /// </para>
    /// <para>
    /// Not thread-safe: use it from the Unity main thread only.
    /// </para>
    /// </remarks>
    public sealed class RegisteredAchievement
    {
        internal string PluginGuid { get; }
        internal string FullId => $"{PluginGuid}::{Definition.Id}";
        internal AchievementProgressData ProgressData { get; }
        /// <summary>
        /// The definition this achievement was registered with.
        /// </summary>
        public AchievementDefinition Definition { get; }
        /// <summary>
        /// <see langword="true"/> once the achievement has been unlocked.
        /// </summary>
        public bool IsUnlocked => ProgressData.Unlocked;
        /// <summary>
        /// Current progress of the achievement.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="AchievementDefinition.Type"/> is <see cref="AchievementType.Instant"/>, it returns 0.
        /// </para>
        /// <para>
        /// With <see cref="AchievementScope.Run"/>, the progress is cleared when the character dies
        /// before the achievement is unlocked.
        /// </para>
        /// </remarks>
        public float CurrentProgress => ProgressData.Progress;
        /// <summary>
        /// UTC date and time when the achievement was unlocked, or <see langword="null"/> if it is still locked.
        /// </summary>
        public DateTime? UnlockedAt => ProgressData.UnlockedAt;
        /// <summary>
        /// Named flags for achievements that depend on several conditions.
        /// </summary>
        /// <remarks>
        /// The tracker is reset together with the progress according to
        /// <see cref="AchievementDefinition.Scope"/>.
        /// </remarks>
        public ConditionalAchievementTracker ConditionalTracker { get; } = new();

        internal RegisteredAchievement(string pluginGuid, AchievementDefinition definition, AchievementProgressData progress)
        {
            PluginGuid = pluginGuid;
            Definition = definition;
            ProgressData = progress;
        }

        /// <summary>
        /// Adds progress to the achievement. When the progress reaches
        /// <see cref="AchievementDefinition.MaxProgress"/>, the achievement is unlocked.
        /// </summary>
        /// <param name="amount">Amount of progress to add. The default is 1.</param>
        /// <returns>
        /// <see langword="true"/> if progress was added and the achievement state changed;
        /// <see langword="false"/> if the call was a no-op: the achievement is of type
        /// <see cref="AchievementType.Instant"/>, it is already unlocked, or the current run is marked as
        /// <see cref="RunIntegrity.Cheated"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <see cref="AchievementType.Instant"/> achievements the call does nothing and a warning is
        /// written to the log. For an already unlocked achievement the call silently returns <see langword="false"/>.
        /// </para>
        /// <para>
        /// Raises <see cref="AchievementEvents.AchievementsChanged"/>. Changed progress is saved to
        /// disk by the periodic autosave (currently every 10 seconds); an unlock is saved immediately.
        /// </para>
        /// </remarks>
        public bool AddProgress(float amount = 1)
        {
            return AchievementService.AddProgressInternal(this, amount);
        }

        /// <summary>
        /// Unlocks the achievement immediately.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the achievement became unlocked because of this call;
        /// <see langword="false"/> if it was already unlocked, or if the current run is marked as
        /// <see cref="RunIntegrity.Cheated"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For an achievement of type <see cref="AchievementType.Counter"/>, <see cref="CurrentProgress"/>
        /// becomes <see cref="AchievementDefinition.MaxProgress"/>.
        /// </para>
        /// <para>
        /// The player is notified according to their alert settings. The unlock is saved to disk immediately.
        /// Calling it for an already unlocked achievement silently returns <see langword="false"/>.
        /// </para>
        /// </remarks>
        public bool Unlock()
        {
            return AchievementService.UnlockInternal(this);
        }
    }
}
