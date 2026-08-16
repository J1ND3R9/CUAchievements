using CUAchievements.Persistence;
using CUAchievements.Registry;
using CUAchievements.UI;
using System;
using UnityEngine.Windows.Speech;

namespace CUAchievements.Services
{
    internal sealed class AchievementService
    {
        internal readonly string _pluginGuid;
        internal readonly ConditionalAchievementTracker _conditions = new ConditionalAchievementTracker();

        internal AchievementService(string pluginGuid)
        {
            _pluginGuid = pluginGuid;
        }

        internal static AchievementService Create(string pluginGuid)
        {
            return new AchievementService(pluginGuid);
        }

        internal bool AddProgress(string id, float amount)
        {
            if (!AchievementRegistry.TryGet(_pluginGuid, id, out RegisteredAchievement achievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            return AddProgressInternal(achievement, amount);
        }

        internal bool Unlock(string id)
        {
            if (!AchievementRegistry.TryGet(_pluginGuid, id, out RegisteredAchievement achievement))
            {
                CUAchievementsPlugin.Logger.LogError($"Failed unlock achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            return UnlockInternal(achievement);
        }

        internal static bool AddProgressInternal(RegisteredAchievement achievement, float amount)
        {
            if (achievement.Definition.Type != Definitions.AchievementType.Counter)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {achievement.Definition.Id} is not a counter type.");
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
                UnlockInternal(achievement);
            }

            AchievementPersistence.Save(achievement.PluginGuid);

            return true;
        }

        internal static bool UnlockInternal(RegisteredAchievement achievement)
        {
            if (achievement.Progress.Unlocked) return true;

            achievement.Progress.Unlocked = true;
            achievement.Progress.UnlockedAt = DateTime.UtcNow;
            AchievementPopupManager.Show(achievement.Definition);
            AchievementPersistence.Save(achievement.PluginGuid);
            return true;
        }
    }
}
