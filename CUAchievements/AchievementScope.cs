using CUAchievements.Registry;

namespace CUAchievements
{
    /// <summary>
    /// Determines how long the progress and conditions of an achievement are kept before they are reset.
    /// </summary>
    public enum AchievementScope
    {
        /// <summary>
        /// Progress and conditions are kept between runs: neither <see cref="RegisteredAchievement.CurrentProgress"/>
        /// nor <see cref="RegisteredAchievement.ConditionalTracker"/> is ever reset.
        /// </summary>
        Lifetime,
        /// <summary>
        /// The achievement has to be earned within a single run. When the character dies without the achievement
        /// being unlocked, <see cref="RegisteredAchievement.CurrentProgress"/> and
        /// <see cref="RegisteredAchievement.ConditionalTracker"/> are cleared.
        /// </summary>
        /// <remarks>
        /// An achievement that is already unlocked stays unlocked.
        /// </remarks>
        Run
    }
}
