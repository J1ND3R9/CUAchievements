using BepInEx;
using CUAchievements.Definitions;
using CUAchievements.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace CUAchievements.Persistence
{
    internal sealed class AchievementPersistence
    {
        private static readonly Dictionary<string, AchievementSaveFile> Saves = [];

        private static readonly HashSet<string> DirtyPlugins = [];

        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.None
        };

        public static void MarkDirty(string modGuid)
        {
            DirtyPlugins.Add(modGuid);
        }

        public static void EnsureLoaded(string modGuid)
        {
            if (Saves.ContainsKey(modGuid))
                return;

            AchievementSaveFile save = null;
            string saveFile = GetSaveFilePath(modGuid);

            if (File.Exists(saveFile))
            {
                save = TryLoadSaveFile(saveFile, modGuid);
            }

            string backupFile = $"{saveFile}.backup";

            if (save == null && File.Exists(backupFile))
            {
                CUAchievementsPlugin.Logger.LogWarning($"Main save for {modGuid} is corrupted or missing. Trying to load from backup...");
                save = TryLoadSaveFile(backupFile, modGuid);

                if (save != null)
                {
                    try
                    {
                        File.Copy(backupFile, saveFile, true);
                    }
                    catch (Exception ex)
                    {
                        CUAchievementsPlugin.Logger.LogWarning($"Failed to restore backup for {modGuid}: {ex.Message}");
                    }
                }
            }

            save ??= CreateSave(modGuid, 1);

            Saves[modGuid] = save;
        }

        private static AchievementSaveFile TryLoadSaveFile(string path, string modGuid)
        {
            try
            {
                byte[] compressed = File.ReadAllBytes(path);

                if (compressed.Length == 0)
                    return null;

                string json = SaveCompression.Decompress(compressed);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                AchievementSaveFile loaded = JsonConvert.DeserializeObject<AchievementSaveFile>(json, Settings);

                if (loaded.PluginGuid != modGuid)
                    return null;

                if (loaded != null)
                {
                    loaded.Achievements ??= [];
                    return loaded;
                }
            }
            catch (Exception ex)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed to load achievements for plugin {modGuid}: {ex}");
            }

            return null;
        }

        public static void Restore(RegisteredAchievement achievement)
        {
            EnsureLoaded(achievement.PluginGuid);

            AchievementSaveFile save = Saves[achievement.PluginGuid];

            if (!save.Achievements.TryGetValue(achievement.Definition.Id, out AchievementProgressData progress))
            {
                return;
            }

            if (achievement.Definition.Scope == AchievementScope.Lifetime)
            {
                achievement.Progress.Progress = progress.Progress;
            }
            achievement.Progress.Unlocked = progress.Unlocked;
            achievement.Progress.UnlockedAt = progress.UnlockedAt;
        }

        private static bool Save(string modGuid)
        {
            try
            {
                EnsureLoaded(modGuid);

                AchievementSaveFile save = Saves[modGuid];

                foreach (KeyValuePair<string, RegisteredAchievement> pair in AchievementRegistry.GetAll())
                {
                    RegisteredAchievement achievement = pair.Value;

                    if (achievement.PluginGuid != modGuid)
                        continue;

                    save.Achievements[achievement.Definition.Id] = achievement.Progress;
                }

                string json = JsonConvert.SerializeObject(save, Settings);
                byte[] compressed = SaveCompression.Compress(json);

                string saveFile = GetSaveFilePath(modGuid);

                if (!SaveFile(compressed, saveFile))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed to save achievements for plugin {modGuid}: {ex}");

                return false;
            }
        }
        public static void SaveNow(string modGuid)
        {
            Save(modGuid);
        }
        public static void SaveDirty()
        {
            foreach (string modGuid in DirtyPlugins.ToArray())
            {
                if (Save(modGuid))
                    DirtyPlugins.Remove(modGuid);
            }
        }

        private static bool SaveFile(byte[] compressed, string saveFilePath)
        {
            try
            {
                string tempPath = $"{saveFilePath}.tmp";
                string backupPath = $"{saveFilePath}.backup";

                using (FileStream fs = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    fs.Write(compressed, 0, compressed.Length);
                    fs.Flush(flushToDisk: true);
                }

                if (File.Exists(saveFilePath))
                {
                    try
                    {
                        File.Replace(tempPath, saveFilePath, backupPath);
                    }
                    catch (IOException)
                    {
                        File.Copy(tempPath, saveFilePath, true);
                        File.Delete(tempPath);
                    }
                }
                else
                {
                    File.Move(tempPath, saveFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                CUAchievementsPlugin.Logger.LogError($"Failed to write save file {saveFilePath}: {ex}");
                return false;
            }
        }

        private static string GetDirectory()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string directoryPath = Path.Combine(
                Directory.GetParent(localAppData).FullName,
                "LocalLow",
                "Orsoniks",
                "CasualtiesUnknown",
                "CUAchievements",
                "Data");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }

        private static string GetSaveFilePath(string modGuid)
        {
            return Path.Combine(GetDirectory(), $"{modGuid}.sv");
        }

        private static AchievementSaveFile CreateSave(string modGuid, int version)
        {
            return new AchievementSaveFile
            {
                Version = version,
                PluginGuid = modGuid,
                Achievements = []
            };
        }

    }
}
