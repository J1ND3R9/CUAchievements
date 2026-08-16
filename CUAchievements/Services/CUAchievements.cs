using CUAchievements.Definitions;
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
        private AchievementRegistry Registry { get; }
        private AchievementService Service { get; }
        private string PluginGUID { get; }

        private CUAchievements(string pluginGuid)
        {
            Registry = AchievementRegistry.Create(pluginGuid);
            Service = AchievementService.Create(pluginGuid);
            PluginGUID = pluginGuid;
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

        /// <summary>
        /// Registers a new <see cref="AchievementDefinition"/>.
        /// </summary>
        /// <returns>
        /// <see cref="RegisteredAchievement"/> if achievement registration was successfully; otherwise null.
        /// </returns>
        public RegisteredAchievement Register(AchievementDefinition achievement) => Registry.Register(achievement);

        /// <summary>
        /// Attempt to retrieve a <see cref="RegisteredAchievement"/> by ID
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="achievement">Out <see cref="RegisteredAchievement"/> if it registered.</param>
        /// <returns>
        /// True if <see cref="RegisteredAchievement"/> is registered; otherwise false.
        /// </returns>
        public bool TryGet(string id, out RegisteredAchievement achievement) => Registry.TryGet(id, out achievement);

        /// <summary>
        /// Adds progress to a registered achievement.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="amount">Amount of progress to add. Default is 1.</param>
        /// <returns>
        /// True if the progress was added successfully; otherwise false.
        /// </returns>
        public bool AddProgress(string id, float amount = 1) => Service.AddProgress(id, amount);

        /// <summary>
        /// Unlock a registered achievement.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <returns>
        /// True if the unlock was successfully; otherwise false.
        /// </returns>
        public bool Unlock(string id) => Service.Unlock(id);

        /// <summary>
        /// Tracks the fulfillment of conditions across different layers of code, scoped to this plugin.
        /// </summary>
        public ConditionalAchievementTracker Conditions => Service._conditions;

        /// <summary>
        /// Retrieves a IReadOnlyList of all achievements registered by your plugin.
        /// </summary>
        public IReadOnlyList<RegisteredAchievement> GeyMyAchievements()
        {
            return AchievementRegistry.GetAll()
                .Where(kv => kv.Value.PluginGuid == PluginGUID)
                .Select(kv => kv.Value)
                .ToList();
        }
    }
}
