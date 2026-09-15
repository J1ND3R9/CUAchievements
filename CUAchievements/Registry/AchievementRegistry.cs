using BepInEx;
using BepInEx.Bootstrap;
using CUAchievements.Definitions;
using CUAchievements.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CUAchievements.Registry
{
    internal sealed class AchievementRegistry
    {
        private static readonly Dictionary<string, RegisteredAchievement> RegisteredAchievements = new Dictionary<string, RegisteredAchievement>(StringComparer.Ordinal);
        private readonly string _pluginGuid;
        private AchievementRegistry(string pluginGuid)
        {
            _pluginGuid = pluginGuid;
        }
        /// <summary>
        /// Create a new <see cref="AchievementRegistry"/>. Not recommended. Use <see cref="Services.CUAchievements"/>.
        /// </summary>
        /// <param name="pluginGuid">Unique plugin id. Use your actual GUID.</param>
        /// <returns>
        /// </returns>
        internal static AchievementRegistry Create(string pluginGuid)
        {
            return new AchievementRegistry(pluginGuid);
        }
        
        internal RegisteredAchievement Register(AchievementDefinition achievement)
        {
            if (string.IsNullOrEmpty(_pluginGuid))
            {
                CUAchievementsPlugin.Logger.LogError($"Achievement registration failed. Error: AchievementRegistry was not created.");
                return null;
            }
            string errorText = Validate(achievement);
            if (!string.IsNullOrEmpty(errorText))
            {
                CUAchievementsPlugin.Logger.LogError($"Achievement registration failed. Error: {errorText}");
                return null;
            }
            string fullId = $"{_pluginGuid}::{achievement.Id}";
            if (RegisteredAchievements.ContainsKey(fullId))
            {
                CUAchievementsPlugin.Logger.LogWarning($"Achievement {achievement.Id} already registered. Ignored duplicate.");
                return null;
            }

            AchievementProgressData progress = new AchievementProgressData(fullId);
            RegisteredAchievement registeredAchievement = new RegisteredAchievement(_pluginGuid, achievement, progress);
            RegisteredAchievements[fullId] = registeredAchievement;

            CUAchievementsPlugin.Logger.LogInfo($"Ачивка \"{achievement.Title}\" зарегистрирована!");

            AchievementPersistence.Load(_pluginGuid);
            AchievementPersistence.Save(_pluginGuid);

            return registeredAchievement;
        }

        
        internal static bool TryGet(string pluginGuid, string id, out RegisteredAchievement achievement)
        {
            achievement = null;
            string fullId = $"{pluginGuid}::{id}";
            if (RegisteredAchievements.ContainsKey(fullId))
            {
                achievement = RegisteredAchievements[fullId];
                return true;
            }
            return false;
        }

        internal bool TryGet(string id, out RegisteredAchievement achievement)
        {
            achievement = null;
            string fullId = $"{_pluginGuid}::{id}";
            if (RegisteredAchievements.ContainsKey(fullId))
            {
                achievement = RegisteredAchievements[fullId];
                return true;
            }
            return false;
        }

        internal static IReadOnlyDictionary<string, RegisteredAchievement> GetAll() => RegisteredAchievements;

        internal static (int, int) GetTotalProgress() => (RegisteredAchievements.Values.Count(a => a.IsUnlocked), RegisteredAchievements.Count());

        internal static IReadOnlyDictionary<string, List<RegisteredAchievement>> GetAchievementsPerPlugin(bool nameInsteadGuid, string search = "")
        {
            Dictionary<string, List<RegisteredAchievement>> pluginAchievements = new Dictionary<string, List<RegisteredAchievement>>();

            var registeredAchievements = RegisteredAchievements.Values.ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                registeredAchievements = registeredAchievements.Where(a => a.Definition.Title.ToLower().Contains(search.ToLower()) || a.Definition.Description.ToLower().Contains(search.ToLower())).ToList();
            }

            foreach (var achievement in registeredAchievements)
            {
                string plugin = achievement.PluginGuid;
                if (nameInsteadGuid)
                {
                    if (Chainloader.PluginInfos.TryGetValue(achievement.PluginGuid, out PluginInfo info))
                    {
                        plugin = info.Metadata.Name;
                    }
                }

                if (!pluginAchievements.ContainsKey(plugin))
                    pluginAchievements[plugin] = new List<RegisteredAchievement>();

                pluginAchievements[plugin].Add(achievement);

            }
            return pluginAchievements;
        }

        private string Validate(AchievementDefinition achievement)
        {
            if (achievement == null)
            {
                return "achievement was null.";
            }
            if (string.IsNullOrEmpty(achievement.Id))
            {
                return "achievement id was null or empty.";
            }
            if (achievement.Type == AchievementType.Counter && achievement.MaxProgress <= 0)
            {
                return "achievement type is counter, but MaxProgress is or below zero.";
            }
            if (string.IsNullOrEmpty(achievement.Title))
            {
                return "achievement name was null or empty.";
            }
            if (string.IsNullOrEmpty(achievement.Description))
            {
                return "achievement description was null or empty.";
            }
            return null;
        }
    }
}
