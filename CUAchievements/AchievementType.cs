using CUAchievements.Registry;

namespace CUAchievements
{
    /// <summary>
    /// Determines how an achievement is earned.
    /// </summary>
    public enum AchievementType
    {
        /// <summary>
        /// Earned by accumulating progress up to <see cref="AchievementDefinition.MaxProgress"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Progress is added with <see cref="RegisteredAchievement.AddProgress(float)"/> and is clamped to
        /// <see cref="AchievementDefinition.MaxProgress"/>. The achievement unlocks automatically when that value is reached.
        /// </para>
        /// <para>
        /// Calling <see cref="RegisteredAchievement.Unlock"/> unlocks it immediately and sets
        /// <see cref="RegisteredAchievement.CurrentProgress"/> to <see cref="AchievementDefinition.MaxProgress"/>.
        /// </para>
        /// </remarks>
        Counter,


        /// <summary>
        /// Earned in a single step by calling <see cref="RegisteredAchievement.Unlock"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="RegisteredAchievement.AddProgress(float)"/> has no effect on this type: it returns
        /// <see langword="false"/> and writes a warning to the log.
        /// </remarks>
        Instant
    }
}
