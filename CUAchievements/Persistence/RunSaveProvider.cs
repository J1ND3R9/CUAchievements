using CUAchievements.Patches;
using CUAchievements.Plugin;
using CUAchievements.Registry;
using CUAchievements.UI.Menu.CustomButton;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;

namespace CUAchievements.Persistence
{
    internal static class RunIntegrity
    {
        public static bool Cheated { get; private set; }
        public static void MarkCheated() => Cheated = true;
        public static void Reset() => Cheated = false;
    }

    internal sealed class RunSaveProvider : ICustomSaveProvider
    {
        public int GetVersion() => 1;

        public JToken Capture()
        {
            JObject achievements = [];

            foreach (RegisteredAchievement achievement in AchievementRegistry.GetAll().Values)
            {
                if (achievement.IsUnlocked || achievement.Definition.Scope != AchievementScope.Run)
                    continue;

                achievements[achievement.FullId] = new JObject
                {
                    ["progress"] = achievement.ProgressData.Progress,
                    ["flags"] = JObject.FromObject(achievement.ConditionalTracker.Snapshot())
                };
            }

            Log.Debug($"Captured on save:\n" +
                $"Cheated: {RunIntegrity.Cheated}\n" +
                $"Achievements count: {achievements.Count}");

            return new JObject
            {
                ["cheated"] = RunIntegrity.Cheated,
                ["achievements"] = achievements
            };
        }

        public void Restore(JToken payload, int version, SaveRestoreContext context)
        {
            JObject obj = payload as JObject;

            RestoreAchievements(obj?["achievements"] as JObject);

            bool cheated = (bool?)obj?["cheated"] ?? false;

            RunIntegrity.Reset();

            if (AnticheatPatches.AnticheatDisabled) return;

            if (cheated)
            {
                RunIntegrity.MarkCheated();
                PauseMenuButtonBuilder.SetTooltip();
                Log.Debug("The run is loaded and classified as 'cheated'");
            }
        }

        private static void RestoreAchievements(JObject achievements)
        {
            if (achievements == null) return;

            foreach (JProperty saved in achievements.Properties())
            {
                if (!AchievementRegistry.TryGetFull(saved.Name, out RegisteredAchievement achievement))
                    continue;

                if (achievement.IsUnlocked || achievement.Definition.Scope != AchievementScope.Run)
                    continue;

                JObject state = saved.Value as JObject;

                achievement.ProgressData.Progress = (float?)state?["progress"] ?? 0f;

                if (state?["flags"] is not JObject flags)
                    continue;

                foreach (JProperty flag in flags.Properties())
                {
                    achievement.ConditionalTracker.SetFlag(flag.Name, (bool)flag.Value);
                }

                Log.Debug($"Achievement '{achievement.FullId}' is restored to -\n" +
                    $"Progress: {achievement.ProgressData.Progress}\n" +
                    $"ConditionalTracker: {achievement.ConditionalTracker.Snapshot().Count}");
            }
        }
    }
}
