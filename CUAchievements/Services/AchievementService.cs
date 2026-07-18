using CUAchievements.Persistence;
using CUAchievements.Registry;

namespace CUAchievements.Services
{
    public sealed class AchievementService
    {
        public static bool AddProgress(string id, float amount = 1)
        {
            if (!AchievementRegistry.TryGet(id, out var registeredAchievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: AchievementDefinition with id {id} does not exists.");
                return false;
            }

            var progress = registeredAchievement.Progress;

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

        public static bool Unlock(string id)
        {
            if (!AchievementRegistry.TryGet(id, out var registeredAchievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed unlock achievement. Error: AchievementDefinition with id {id} does not exists.");
                return false;
            }

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
