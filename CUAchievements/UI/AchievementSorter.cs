using CUAchievements.Registry;
using System.Collections.Generic;
using System.Linq;

namespace CUAchievements.UI
{
    internal static class AchievementSorter
    {
        public static List<RegisteredAchievement> Sort(IEnumerable<RegisteredAchievement> source)
        {
            List<RegisteredAchievement> all = [.. source];

            IOrderedEnumerable<RegisteredAchievement> unlocked = all.Where(a => a.IsUnlocked).OrderByDescending(a => a.UnlockedAt);

            return unlocked.Concat(Locked(all)).ToList();
        }

        private static IEnumerable<RegisteredAchievement> Locked(List<RegisteredAchievement> all) => all
            .Where(a => !a.IsUnlocked);
    }
}