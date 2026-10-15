using BepInEx.Configuration;
using CUAchievements.Anticheat;
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
    internal enum AchievementDisplayType { BottomToUp = 0, LeftToRight = 1, UpToBottom = 2, SteamLike = 3 }
    internal static class AchievementConfig
    {
        public static ConfigEntry<bool> AnticheatEnabled { get; private set; }
        public static ConfigEntry<bool> AllowSandboxAchievements { get; private set; }

        public static ConfigEntry<AchievementAlertType> AlertType { get; private set; }
        
        public static ConfigEntry<AchievementDisplayType> DisplayType { get; private set; }

        public static ConfigEntry<int> PopUpTime { get; private set; }

        public static ConfigEntry<float> UnlockVolume { get; private set; }
        public static void Initialize(ConfigFile config)
        {
            AnticheatEnabled = config.Bind("Anticheat", "AnticheatEnabled", true, "Enable or disable the command anti-cheat system – most commands are marked as \"unsafe,\" and executing any such command will disable unlocks and achievement progress for that save file.");
            AllowSandboxAchievements = config.Bind("Anticheat", "AllowSandboxAchievements", true, "Enables or disables the ability to earn achievements in the tutorial/sandbox.");

            AlertType = config.Bind("Alert", "AlertType", AchievementAlertType.Popup, "Achievement notification type\n" +
                "0 - Pop-up\n" +
                "1 - Standard in-game Alert\n" +
                "2 - Hidden");

            DisplayType = config.Bind("Alert", "DisplayType", AchievementDisplayType.BottomToUp, "Pop-up notification display type\n" +
                "0 - From bottom to top\n" +
                "1 - From the bottom-left corner to the right\n" +
                "2 - From top to bottom\n" +
                "3 - Like in Steam: from the bottom-right corner upwards");

            PopUpTime = config.Bind("Notifications", "PopUpTime", 5, new ConfigDescription("Duration for which the achievement notification is displayed. (from 1 to 15 seconds)", new AcceptableValueRange<int>(1, 15)));

            UnlockVolume = config.Bind("Notifications", "UnlockVolume", 1f, new ConfigDescription("Volume for the achievement unlock sound (0 = muted)", new AcceptableValueRange<float>(0f, 1f)));

            AnticheatPatches.AnticheatDisabled = !AnticheatEnabled.Value;
            RegisterOptions();
        }
        private static void RegisterOptions()
        {
            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
                $"{CUAchievementsPlugin.ModGUID}.AlertType",
                "Alert type",
                "How unlocked achievements are announced",
                "Achievements",
                (int)AlertType.Value,
                [
                    new ModDropdownChoice("popup", "Pop-up"),
                    new ModDropdownChoice("default", "Alert"),
                    new ModDropdownChoice("hidden", "Hidden")
                ],
                value => AlertType.Value = (AchievementAlertType)value
                ));

            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
                $"{CUAchievementsPlugin.ModGUID}.DisplayType",
                "Display type",
                "How to display a pop-up notification",
                "Achievements",
                (int)DisplayType.Value,
                [
                    new ModDropdownChoice("bottomUp", "Bottom > Up"),
                    new ModDropdownChoice("leftRight", "Left > Right"),
                    new ModDropdownChoice("upBottom", "Up > Bottom"),
                    new ModDropdownChoice("steamLike", "Steam-like"),
                ],
                value =>
                {
                    DisplayType.Value = (AchievementDisplayType)value;
                    AchievementPopupManager.DebugShow();
                }));

            ModOptionsRegistry.Register(ModOptionDefinition.Float(
                $"{CUAchievementsPlugin.ModGUID}.PopUpTime",
                "Pop-up duration",
                "Duration of the pop-up notification",
                "Achievements",
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
                "Achievements",
                UnlockVolume.Value,
                0f,
                1f,
                value => UnlockVolume.Value = value,
                value => Mathf.RoundToInt(value * 100f) + "%"
                ));

        }
    }
}
