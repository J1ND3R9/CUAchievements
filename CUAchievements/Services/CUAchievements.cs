using CUAchievements.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUAchievements.Services
{
    public sealed class CUAchievements
    {
        public AchievementRegistry Registry { get; }
        public AchievementService Achievements { get; }

        private CUAchievements(string pluginGuid)
        {
            Registry = AchievementRegistry.Create(pluginGuid);
            Achievements = AchievementService.Create(pluginGuid);
        }

        /// <summary>
        /// Create a new <see cref="CUAchievements"/>. Combines <see cref="AchievementRegistry"/> and <see cref="AchievementService"/> for API convenience.
        /// </summary>
        /// <param name="pluginGuid">Unique plugin id. Use your actual GUID.</param>
        /// <returns>
        /// </returns>
        public static CUAchievements Create(string pluginGuid)
        {
            return new CUAchievements(pluginGuid);
        }
    }
}
