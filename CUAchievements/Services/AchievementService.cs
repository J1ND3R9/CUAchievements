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
                Log.Error($"Failed add progress to achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            return AddProgressInternal(achievement, amount);
        }

        internal bool Unlock(string id)
        {
            if (!AchievementRegistry.TryGet(_pluginGuid, id, out RegisteredAchievement achievement))
            {
                Log.Error($"Failed unlock achievement. Error: Achievement with id {id} does not exists.");
                return false;
            }

            return UnlockInternal(achievement);
        }

        internal static bool AddProgressInternal(RegisteredAchievement achievement, float amount)
        {
            if (achievement.Definition.Type != AchievementType.Counter)
            {
                Log.Error($"Failed add progress to achievement. Error: Achievement with id {achievement.Definition.Id} is not a counter type.");
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

            AchievementProgressData progress = achievement.ProgressData;

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
            if (achievement.ProgressData.Unlocked) return false;

#if !DEBUG
            if (!achievement.Definition.CanGetInTutorial)
            {
                if (GameUtility.IsTutorialOrSandbox()) return false;
            }
#endif

            if (RunIntegrity.Cheated)
                return false;

            achievement.ProgressData.Unlocked = true;
            achievement.ProgressData.UnlockedAt = DateTime.UtcNow;
            achievement.ProgressData.Progress = achievement.Definition.MaxProgress;

            AchievementPopupManager.Show(achievement);
            AchievementPersistence.SaveNow(achievement.PluginGuid);
            AchievementEvents.RaiseAchievementsChanged();

            return true;
        }

        internal static void EndRun()
        {
            RunIntegrity.Reset();
            foreach (RegisteredAchievement achievement in AchievementRegistry.GetAll().Values)
            {
                if (achievement.IsUnlocked)
                    continue;

                if (achievement.Definition.Scope != AchievementScope.Run)
                    continue;

                if (achievement.Definition.Type == AchievementType.Counter)
                {
                    if (achievement.ProgressData.Progress != 0)
                    {
                        achievement.ProgressData.LastRunProgress = achievement.ProgressData.Progress;
                    }

                    achievement.ProgressData.Progress = 0;
                }

                achievement.ConditionalTracker?.Clear();
            }
        }
    }
}
