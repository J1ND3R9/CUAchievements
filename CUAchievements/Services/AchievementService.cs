using CUAchievements.Persistence;
using CUAchievements.Registry;

namespace CUAchievements.Services
{
    public sealed class AchievementService
    {
        public static bool AddProgress(string id, float amount = 1)
        {
            if (!AchievementRegistry.TryGet(id, out RegisteredAchievement achievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {id} does not exists.");
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
