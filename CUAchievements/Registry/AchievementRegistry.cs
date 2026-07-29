using CUAchievements.Definitions;
using CUAchievements.Persistence;
using System;
using System.Collections.Generic;

namespace CUAchievements.Registry
{
    public sealed class AchievementRegistry
    {
        private static readonly Dictionary<string, RegisteredAchievement> RegisteredAchievements = new Dictionary<string, RegisteredAchievement>(StringComparer.Ordinal);
        private readonly string _pluginGuid;
        private AchievementRegistry(string pluginGuid)
        {
            _pluginGuid = pluginGuid;
        }
        /// <summary>
        /// Create a new <see cref="AchievementRegistry"/>.
        /// </summary>
        /// <param name="pluginGuid">Unique plugin id. Use your actual GUID.</param>
        /// <returns>
        /// </returns>
        public static AchievementRegistry Create(string pluginGuid)
        {
            return new AchievementRegistry(pluginGuid);
        }
        /// <summary>
        /// Registers a new <see cref="AchievementDefinition"/>.
        /// </summary>
        /// <returns>
        /// True achievement registration was successfully; otherwise false.
        /// </returns>
        public bool Register(AchievementDefinition achievement)
        {
            if (string.IsNullOrEmpty(_pluginGuid))
            {
                CUAchievementsPlugin.Logger.LogError($"Achievement registration failed. Error: AchievementRegistry was not created.");
                return false;
            }
            string errorText = Validate(achievement);
            if (!string.IsNullOrEmpty(errorText))
            {
                CUAchievementsPlugin.Logger.LogError($"Achievement registration failed. Error: {errorText}");
                return false;
            }
            if (RegisteredAchievements.ContainsKey(achievement.Id))
            {
                CUAchievementsPlugin.Logger.LogWarning($"Achievement {achievement.Id} already registered. Ignored duplicate.");
                return false;
            }

            AchievementProgressData progress = new AchievementProgressData(achievement.Id);
            RegisteredAchievement registeredAchievement = new RegisteredAchievement(_pluginGuid, achievement, progress);
            RegisteredAchievements[achievement.Id] = registeredAchievement;

            CUAchievementsPlugin.Logger.LogInfo($"Ачивка \"{achievement.Name}\" зарегистрирована!");

            AchievementPersistence.Load(_pluginGuid);
            AchievementPersistence.Save(_pluginGuid);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Unique achievement identifier.</param>
        /// <param name="achievement">Out <see cref="RegisteredAchievement"/> if it registered.</param>
        /// <returns>
        /// True if <see cref="RegisteredAchievement"/> is registered; otherwise false.
        /// </returns>
        public static bool TryGet(string id, out RegisteredAchievement achievement)
        {
            achievement = null;
            if (RegisteredAchievements.ContainsKey(id))
            {
                achievement = RegisteredAchievements[id];
                return true;
            }
            return false;
        }

        internal static Dictionary<string, RegisteredAchievement> GetAll()
        {
            return RegisteredAchievements;
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
            if (string.IsNullOrEmpty(achievement.Name))
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
