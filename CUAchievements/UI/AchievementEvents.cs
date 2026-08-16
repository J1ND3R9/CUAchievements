using System;

namespace CUAchievements.UI
{
    public static class AchievementEvents
    {
        public static event Action AchievementsChanged;

        internal static void RaiseAchievementsChanged()
        {
            AchievementsChanged?.Invoke();
        }
    }
}
