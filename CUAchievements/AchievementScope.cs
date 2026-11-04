using CUAchievements.Registry;

namespace CUAchievements
{
    /// <summary>
    /// Determines how long the progress and conditions of an achievement are kept before they are reset.
    /// </summary>
    public enum AchievementScope
    {
        /// <summary>
        /// Progress and conditions are kept between runs and game restarts: neither
        /// <see cref="RegisteredAchievement.CurrentProgress"/> nor <see cref="RegisteredAchievement.ConditionalTracker"/>
        /// is reset by CUAchievements.
        /// </summary>
        Lifetime,
        /// <summary>
        /// The achievement has to be earned within a single run. When the character dies or a new run starts without
        /// the achievement being unlocked, <see cref="RegisteredAchievement.CurrentProgress"/> and
        /// <see cref="RegisteredAchievement.ConditionalTracker"/> are cleared.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both are stored in the game save, so they are restored when the player continues the run.
        /// </para>
        /// <para>
        /// An achievement that is already unlocked stays unlocked.
        /// </para>
        /// </remarks>
        Run
    }
}
