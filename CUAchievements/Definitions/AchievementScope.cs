using CUAchievements.Services;

namespace CUAchievements.Definitions
{
    /// <summary>
    /// A set of parameters for completing the achievement.
    /// </summary>
    public enum AchievementScope
    {
        /// <summary>
        /// Achievement tracking is global: the <see cref="ConditionalAchievementTracker"/> is not cleared after each run, and progress is not reset.
        /// </summary>
        Lifetime,
        /// <summary>
        /// The achievement must be completed within a single run: after the run, the <see cref="ConditionalAchievementTracker"/> is cleared and progress is reset.
        /// </summary>
        Run
    }
}
