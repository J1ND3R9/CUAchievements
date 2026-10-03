using CUAchievements.Registry;
using System.Collections.Generic;
using System.Linq;

namespace CUAchievements.UI
{
    internal static class AchievementSorter
    {
        public static List<RegisteredAchievement> Sort(IEnumerable<RegisteredAchievement> source)
        {
            var all = source.ToList();

            var unlocked = all.Where(a => a.IsUnlocked)
                .OrderByDescending(a => a.UnlockedAt)
                .ThenBy(a => (int)a.Definition.Difficulty);

            return unlocked
                .Concat(Locked(all, hidden: false))
                .Concat(Locked(all, hidden: true))
                .ToList();
        }

        private static IEnumerable<RegisteredAchievement> Locked(List<RegisteredAchievement> all, bool hidden) => all
            .Where(a => !a.IsUnlocked && a.Definition.IsHidden == hidden)
            .OrderBy(a => (int)a.Definition.Difficulty)
            .ThenBy(a => a.Definition.Name);
    }
}