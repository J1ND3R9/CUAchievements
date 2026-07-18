using BepInEx;
using CUAchievements.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;

namespace CUAchievements.Persistence
{
    internal sealed class AchievementPersistence
    {
        public static bool Load(string modGuid)
        {
            var saveFile = GetSaveFilePath(modGuid);
            var json = File.ReadAllText(saveFile);

            if (string.IsNullOrWhiteSpace(json))
                return true;

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            var achievementSaveFile = JsonConvert.DeserializeObject<AchievementSaveFile>(json, settings);

            if (achievementSaveFile == null)
                return false;

            foreach (var achievementProgress in achievementSaveFile.Achievements)
            {
                if (!AchievementRegistry.TryGet(achievementProgress.Key, out var achievement))
                    continue;
                achievement.Progress.Progress = achievementProgress.Value.Progress;
                achievement.Progress.Unlocked = achievementProgress.Value.Unlocked;
            }

            return true;
        }

        //public static bool Sync(string modGuid)
        //{
        //    return true;
        //}

        public static bool Save(string modGuid)
        {
            var saveFile = GetSaveFilePath(modGuid);
            var save = CreateSave(modGuid, 1);

            foreach (var achievement in AchievementRegistry.GetAll())
            {
                if (achievement.Value.PluginGuid != modGuid)
                    continue;
                save.Achievements.Add(achievement.Value.Definition.Id, achievement.Value.Progress);
            }
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(save, settings);
            File.WriteAllText(saveFile, json);

            return true;
        }

        private static string GetDirectory()
        {
            var directoryPath = Path.Combine(Paths.PluginPath, "CUAchievements", "Data");
            if (!Directory.Exists(directoryPath))
            {
                CUAchievementsPlugin.Logger.LogInfo("Directory with achievements not exists. Creating...");
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        private static string GetSaveFilePath(string modGuid)
        {
            var directoryPath = GetDirectory();
            var filePath = Path.Combine(directoryPath, $"{modGuid}.json");
            if (!File.Exists(filePath))
            {
                CUAchievementsPlugin.Logger.LogInfo($"File with achievements for plugin {modGuid} not exists. Creating...");
                File.WriteAllText(filePath, "");
            }
            return filePath;
        }

        private static AchievementSaveFile CreateSave(string modGuid, int version)
        {
            return new AchievementSaveFile
            {
                Version = version,
                PluginGuid = modGuid,
                Achievements = new Dictionary<string, AchievementProgressData>()
            };
        }
    }
}
