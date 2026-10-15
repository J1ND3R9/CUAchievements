using BepInEx.Configuration;
using CUAchievements.UI;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CUAchievements.Options
{
    internal enum AchievementAlertType { Popup = 0, Default = 1, Hidden = 2 }
    internal static class AchievementConfig
    {
        public static ConfigEntry<bool> AnticheatEnabled { get; private set; }
        public static ConfigEntry<bool> AllowSandboxAchievements { get; private set; }

        public static ConfigEntry<AchievementAlertType> AlertType { get; private set; }

        public static ConfigEntry<int> PopUpTime { get; private set; }

        public static ConfigEntry<float> UnlockVolume { get; private set; }
        public static void Initialize(ConfigFile config)
        {
            AnticheatEnabled = config.Bind("Anticheat", "AnticheatEnabled", true, "Enable or disable the command anti-cheat system – most commands are marked as \"unsafe,\" and executing any such command will disable unlocks and achievement progress for that save file.");
            AllowSandboxAchievements = config.Bind("Anticheat", "AllowSandboxAchievements", true, "Enables or disables the ability to earn achievements in the tutorial/sandbox.");

            AlertType = config.Bind("Alert", "AlertType", AchievementAlertType.Popup, "Achievement notification type.\n" +
                "0 - Pop-up\n" +
                "1 - Standard in-game Alert\n" +
                "2 - Hidden");

            PopUpTime = config.Bind("Notifications", "PopUpTime", 5, new ConfigDescription("Duration for which the achievement notification is displayed. (from 1 to 15 seconds)", new AcceptableValueRange<int>(1, 15)));

            UnlockVolume = config.Bind("Notifications", "UnlockVolume", 1f, new ConfigDescription("Volume for the achievement unlock sound (0 = muted)", new AcceptableValueRange<float>(0f, 1f)));
            RegisterOptions();
        }
        private static void RegisterOptions()
        {
            string achievementCategory = LocaleRegistry.Get("achievements_category", "Achievements");

            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
                $"{CUAchievementsPlugin.ModGUID}.AlertType",
                "Alert type",
                "How unlocked achievements are announced",
                achievementCategory,
                (int)AlertType.Value,
                new[]
                {
                    new ModDropdownChoice("popup", "Pop-up"),
                    new ModDropdownChoice("default", "Alert"),
                    new ModDropdownChoice("hidden", "Hidden")
                },
                value => AlertType.Value = (AchievementAlertType)value
                ));

            ModOptionsRegistry.Register(ModOptionDefinition.Float(
                $"{CUAchievementsPlugin.ModGUID}.PopUpTime",
                "Pop-up duration",
                "Duration of the pop-up notification",
                achievementCategory,
                PopUpTime.Value,
                1f,
                15f,
                value => PopUpTime.Value = (int)value,
                value => Mathf.RoundToInt(value) + LocaleRegistry.Get("achievements_sec", "s")
                ));

            ModOptionsRegistry.Register(ModOptionDefinition.Float(
                $"{CUAchievementsPlugin.ModGUID}.UnlockVolume",
                "Unlock volume",
                "Achievement unlock notification volume",
                achievementCategory,
                UnlockVolume.Value,
                0f,
                1f,
                value => UnlockVolume.Value = value,
                value => Mathf.RoundToInt(value * 100f) + "%"
                ));

        }
    }
}
