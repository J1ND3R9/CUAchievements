using CUAchievements.Persistence;
using CUAchievements.Registry;
using CUAchievements.UI;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

namespace CUAchievements.Services
{
    public sealed class AchievementService
    {
        private readonly string _pluginGuid;
        private readonly ConditionalAchievementTracker _conditions = new ConditionalAchievementTracker();

        private AchievementService(string pluginGuid)
        {
            _pluginGuid = pluginGuid;
        }

        /// <summary>
        /// Create a new <see cref="AchievementService"/>. Not recommended. Use <see cref="Services.CUAchievements"/>.
        /// </summary>
        /// <param name="pluginGuid">Unique plugin id. Use your actual GUID.</param>
        /// <returns>
        /// </returns>
        public static AchievementService Create(string pluginGuid)
        {
            return new AchievementService(pluginGuid);
        }

        /// <summary>
        /// Adds progress to a registered achievement.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="amount">Amount of progress to add. Default is 1.</param>
        /// <returns>
        /// True if the progress was added successfully; otherwise false.
        /// </returns>
        public bool AddProgress(string id, float amount = 1)
        {
            if (!AchievementRegistry.TryGet(_pluginGuid, id, out RegisteredAchievement achievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            if (achievement.Definition.Type != Definitions.AchievementType.Counter)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {id} is not a counter type."); 
                return false;
            }

            var progress = achievement.Progress;

            if (progress.Unlocked)
                return true;

            progress.Progress += amount;

            CUAchievementsPlugin.Logger.LogInfo($"\"{achievement.Definition.Id}\" {progress.Progress}/{achievement.Definition.MaxProgress}"); // TODO: If debug mode is on

            if (progress.Progress >= achievement.Definition.MaxProgress)
            {
                progress.Progress = achievement.Definition.MaxProgress;
                UnlockCore(achievement);
            }

            AchievementPersistence.Save(achievement.PluginGuid);

            return true;
        }
        /// <summary>
        /// Unlock a registered achievement.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <returns>
        /// True if the unlock was successfully; otherwise false.
        /// </returns>

        public bool Unlock(string id)
        {
            if (!AchievementRegistry.TryGet(_pluginGuid, id, out RegisteredAchievement achievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed unlock achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            var progress = achievement.Progress;

            if (progress.Unlocked)
                return true;

            UnlockCore(achievement);
            AchievementPersistence.Save(achievement.PluginGuid);

            return true;
        }

        private void UnlockCore(RegisteredAchievement registeredAchievement)
        {
            if (registeredAchievement.Progress.Unlocked)
                return;
            AchievementPopupManager.Show(registeredAchievement.Definition);
            registeredAchievement.Progress.Unlocked = true;
        }

        /// <summary>
        /// Tracks the fulfillment of conditions across different layers of code, scoped to this plugin.
        /// </summary>
        public ConditionalAchievementTracker Conditions => _conditions;
        public sealed class ConditionalAchievementTracker
        {
            private readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

            public void SetFlag(string flagName, bool value) => flags[flagName] = value;
            public bool GetFlag(string flagName) => flags.TryGetValue(flagName, out var v) && v;
            public void ClearFlag(string flagName) => flags.Remove(flagName);
        }
    }
}
