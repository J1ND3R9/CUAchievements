using CUAchievements.Definitions;
using CUAchievements.Persistence;
using CUAchievements.Registry;
using CUAchievements.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

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
            if (achievement.Definition.Type != Definitions.AchievementType.Counter)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed add progress to achievement. Error: Achievement with id {achievement.Definition.Id} is not a counter type.");
                return false;
            }

            if (!achievement.Definition.CanGetInTutorial)
            {
                if (IsInTutorial()) return true;
            }

            var progress = achievement.Progress;

            if (progress.Unlocked)
                return true;

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
            if (achievement.Progress.Unlocked) return true;

            if (!achievement.Definition.CanGetInTutorial)
            {
                if (IsInTutorial()) return true;
            }

            achievement.Progress.Unlocked = true;
            achievement.Progress.UnlockedAt = DateTime.UtcNow;

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

        internal static bool IsInTutorial()
        {
            if (WorldGeneration.world != null)
            {
                if (WorldGeneration.world.biomeOverride == WorldGeneration.OverrideSceneType.Tutorial)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
