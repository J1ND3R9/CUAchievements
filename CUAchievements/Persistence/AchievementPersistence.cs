using BepInEx;
using CUAchievements.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace CUAchievements.Persistence
{
    internal sealed class AchievementPersistence
    {
        internal static bool Load(string modGuid)
        {
            string saveFile = GetSaveFilePath(modGuid);
            string json = File.ReadAllText(saveFile);

            if (string.IsNullOrWhiteSpace(json))
                return true;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            AchievementSaveFile achievementSaveFile = JsonConvert.DeserializeObject<AchievementSaveFile>(json, settings);

            if (achievementSaveFile == null)
                return false;

            foreach (KeyValuePair<string, AchievementProgressData> achievementProgress in achievementSaveFile.Achievements)
            {
                if (!AchievementRegistry.TryGet(achievementSaveFile.PluginGuid, achievementProgress.Key, out RegisteredAchievement achievement))
                    continue;
                achievement.Progress.Progress = achievementProgress.Value.Progress;
                achievement.Progress.Unlocked = achievementProgress.Value.Unlocked;
                achievement.Progress.UnlockedAt = achievementProgress.Value.UnlockedAt;
            }

            return true;
        }
        internal static bool Save(string modGuid)
        {
            string saveFile = GetSaveFilePath(modGuid);
            AchievementSaveFile save;
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            if (File.Exists(saveFile))
            {
                string existingJson = File.ReadAllText(saveFile);
                if (!string.IsNullOrWhiteSpace(existingJson))
                    save = JsonConvert.DeserializeObject<AchievementSaveFile>(existingJson, settings);
                else
                    save = CreateSave(modGuid, 1);
            }
            else
            {
                save = CreateSave(modGuid, 1);
            }

            foreach (KeyValuePair<string, RegisteredAchievement> achievement in AchievementRegistry.GetAll())
            {
                if (achievement.Value.PluginGuid != modGuid)
                    continue;
                save.Achievements[achievement.Value.Definition.Id] = achievement.Value.Progress;
            }

            string newJson = JsonConvert.SerializeObject(save, settings);
            File.WriteAllText(saveFile, newJson);

            return true;
        }

        private static string GetDirectory()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string directoryPath = Path.Combine(Directory.GetParent(localAppData).FullName, "LocalLow", "Orsoniks", "CasualtiesUnknown", "CUAchievements", "Data");
            if (!Directory.Exists(directoryPath))
            {
                CUAchievementsPlugin.Logger.LogInfo("Directory with achievements not exists. Creating...");
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        private static string GetSaveFilePath(string modGuid)
        {
            string directoryPath = GetDirectory();
            string filePath = Path.Combine(directoryPath, $"{modGuid}.json");
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
