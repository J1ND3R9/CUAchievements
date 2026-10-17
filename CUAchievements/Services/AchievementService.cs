using CUAchievements.Anticheat;
using CUAchievements.Persistence;
using CUAchievements.Plugin;
using CUAchievements.Registry;
using CUAchievements.UI;
using CUAchievements.UI.Utility;
using System;

namespace CUAchievements.Services
{
    internal sealed class AchievementService
    {
        internal readonly string _pluginGuid;

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
            if (achievement.Definition.Type != AchievementType.Counter)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {achievement.Definition.Id} is not a counter type.");
                return false;
            }

#if !DEBUG
            if (!achievement.Definition.CanGetInTutorial)
            {
                if (GameUtility.IsTutorialOrSandbox()) return false;
            }
#endif

            if (RunIntegrity.Cheated)
                return false;

            var progress = achievement.Progress;

            if (progress.Unlocked)
                return false;

            progress.Progress += amount;

            if (progress.Progress >= achievement.Definition.MaxProgress)
            {
                progress.Progress = achievement.Definition.MaxProgress;
                UnlockInternal(achievement);
            }

            else
            {
                AchievementPersistence.MarkDirty(achievement.PluginGuid);
            }

            AchievementEvents.RaiseAchievementsChanged();

            return true;
        }

        internal static bool UnlockInternal(RegisteredAchievement achievement)
        {
            if (achievement.Progress.Unlocked) return false;

#if !DEBUG
            if (!achievement.Definition.CanGetInTutorial)
            {
                if (GameUtility.IsTutorialOrSandbox()) return false;
            }
#endif

            if (RunIntegrity.Cheated)
                return false;

            achievement.Progress.Unlocked = true;
            achievement.Progress.UnlockedAt = DateTime.UtcNow;
            achievement.Progress.Progress = achievement.Definition.MaxProgress;

            AchievementPopupManager.Show(achievement);
            AchievementPersistence.SaveNow(achievement.PluginGuid);
            AchievementEvents.RaiseAchievementsChanged();

            return true;
        }

        internal static void EndRun()
        {
            foreach (RegisteredAchievement achievement in AchievementRegistry.GetAll().Values)
            {
                if (achievement.IsUnlocked)
                    continue;

                if (achievement.Definition.Scope != AchievementScope.Run)
                    continue;

                if (achievement.Definition.Type == AchievementType.Counter)
                {
                    if (achievement.Progress.Progress != 0)
                    {
                        achievement.Progress.LastRunProgress = achievement.Progress.Progress;
                    }

                    achievement.Progress.Progress = 0;
                }

                achievement.ConditionalTracker?.Clear();
            }
        }
    }
}
