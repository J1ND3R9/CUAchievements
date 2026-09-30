using System;

namespace CUAchievements.UI
{
    /// <summary>
    /// A class containing all events related to achievements.
    /// </summary>
    public static class AchievementEvents
    {
        /// <summary>
        /// Raises when an achievement's progress changes (unlocked or progress updated)
        /// </summary>
        public static event Action AchievementsChanged;

        internal static void RaiseAchievementsChanged()
        {
            AchievementsChanged?.Invoke();
        }
    }
}
