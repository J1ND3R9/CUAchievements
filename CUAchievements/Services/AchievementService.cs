using CUAchievements.Persistence;
using CUAchievements.Registry;
using CUAchievements.UI;

namespace CUAchievements.Services
{
    public sealed class AchievementService
    {
        /// <summary>
        /// Adds progress to a registered achievement.
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="amount">Amount of progress to add. Default is 1.</param>
        /// <returns>
        /// True if the progress was added successfully; otherwise false.
        /// </returns>
        public static bool AddProgress(string id, float amount = 1)
        {
            if (!AchievementRegistry.TryGet(id, out RegisteredAchievement achievement))
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

        public static bool Unlock(string id)
        {
            if (!AchievementRegistry.TryGet(id, out RegisteredAchievement achievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed unlock achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            var progress = achievement.Progress;

            if (progress.Unlocked)
                return true;

            UnlockCore(achievement);
            AchievementPersistence.Save(achievement.PluginGuid);
            AchievementPopupManager.Show(achievement.Definition);

            return true;
        }

        private static void UnlockCore(RegisteredAchievement registeredAchievement)
        {
            if (registeredAchievement.Progress.Unlocked)
                return;

            registeredAchievement.Progress.Unlocked = true;
        }
    }
}
