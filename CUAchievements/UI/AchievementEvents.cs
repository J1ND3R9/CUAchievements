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
            var handler = AchievementsChanged;
            if (handler == null) return;

            foreach (Delegate d in handler.GetInvocationList())
            {
                try { ((Action)d)(); }
                catch (Exception e)
                {
                    CUAchievementsPlugin.Logger.LogError($"AchievementsChanged subscriber failed: {e}");
                }
            }
        }
    }
}
