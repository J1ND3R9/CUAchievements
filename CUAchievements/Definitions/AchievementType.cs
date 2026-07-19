namespace CUAchievements.Definitions
{
    /// <summary>
    /// Defines how an achievement is unlocked.
    /// </summary>
    public enum AchievementType
    {
        /// <summary>
        /// Requires accumulating progress.
        /// </summary>
        Counter,

        /// <summary>
        /// Unlocks immediately.
        /// </summary>
        Instant
    }
}
