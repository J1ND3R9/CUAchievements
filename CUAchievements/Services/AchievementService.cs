using CUAchievements.Persistence;
using CUAchievements.Registry;

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
            if (!AchievementRegistry.TryGet(id, out RegisteredAchievement registeredAchievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: AchievementDefinition with id {id} does not exists.");
                return false;
            }

            AchievementProgressData progress = registeredAchievement.Progress;

            if (progress.Unlocked)
                return true;

            progress.Progress += amount;

            CUAchievementsPlugin.Logger.LogInfo($"\"{registeredAchievement.Definition.Id}\" {progress.Progress}/{registeredAchievement.Definition.MaxProgress}"); // TODO: If debug mode is on

            if (progress.Progress >= registeredAchievement.Definition.MaxProgress)
            {
                progress.Progress = registeredAchievement.Definition.MaxProgress;
                UnlockCore(registeredAchievement);
            }

            AchievementPersistence.Save(registeredAchievement.PluginGuid);

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
            if (!AchievementRegistry.TryGet(id, out RegisteredAchievement registeredAchievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed unlock achievement. Error: AchievementDefinition with id {id} does not exists.");
                return false;
            }

            AchievementProgressData progress = registeredAchievement.Progress;

            if (progress.Unlocked)
                return true;

            UnlockCore(registeredAchievement);
            AchievementPersistence.Save(registeredAchievement.PluginGuid);

            return true;
        }

        private static void UnlockCore(RegisteredAchievement registeredAchievement)
        {
            if (registeredAchievement.Progress.Unlocked)
                return;

            registeredAchievement.Progress.Unlocked = true;
            CUAchievementsPlugin.Logger.LogInfo($"Achievement \"{registeredAchievement.Definition.Name}\" ({registeredAchievement.Definition.Id}) unlocked!");
        }
    }
}
