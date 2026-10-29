using BepInEx.Configuration;
using CUAchievements.Patches;
using CUAchievements.Plugin;
using CUAchievements.UI;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using UnityEngine;

namespace CUAchievements.Options
{
    internal enum AchievementAlertType { Popup = 0, Default = 1, Hidden = 2 }
    internal enum AchievementDisplayType { BottomToUp = 0, LeftToRight = 1, UpToBottom = 2, SteamLike = 3 }
    internal static class AchievementConfig
    {
        public static ConfigEntry<bool> AnticheatEnabled { get; private set; }
        public static ConfigEntry<bool> AllowSandboxAchievements { get; private set; }
        public static ConfigEntry<bool> EnabledGreetingMessage { get; private set; }
        public static ConfigEntry<bool> RevealableHiddenAchievements { get; private set; }
        public static ConfigEntry<bool> PrintScopeInAchievementList { get; private set; }
        public static ConfigEntry<bool> EasterEggsEnabled { get; private set; }
        public static ConfigEntry<bool> Verbose { get; private set; }

        public static ConfigEntry<AchievementAlertType> AlertType { get; private set; }

        public static ConfigEntry<AchievementDisplayType> DisplayType { get; private set; }

        public static ConfigEntry<int> PopUpTime { get; private set; }

        public static ConfigEntry<float> UnlockVolume { get; private set; }
        public static void Initialize(ConfigFile config)
        {
            RevealableHiddenAchievements = config.Bind("General", "RevealableHiddenAchievements", true, "Changes the appearance of hidden achievements: instead of a single panel showing a count, it displays multiple panels with hidden text that are revealed when clicked.");
            PrintScopeInAchievementList = config.Bind("General", "PrintScopeInAchievementList", false, "Toggle information to the right-hand side of an achievement in list, indicating the scope within which it must be completed. Does not affect how the scope is displayed in the achievement details.");
            EasterEggsEnabled = config.Bind("General", "EnableEasterEggs", true, "Toggles the appearance of Easter eggs. Easter eggs do not affect gameplay in any way and are purely visual in nature.");

            AnticheatEnabled = config.Bind("Anticheat", "AnticheatEnabled", true, "Toggles the command anti-cheat system – most commands are marked as \"unsafe,\" and executing any such command will disable unlocks and achievement progress for that save file.");
            AllowSandboxAchievements = config.Bind("Anticheat", "AllowSandboxAchievements", false, "Toggles the ability to earn achievements in the tutorial/sandbox, regardless of whether it permits the achievement.");

            EnabledGreetingMessage = config.Bind("Alert", "EnabledGreetingMessage", true, "Toggles the initial pop-up message");
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

            Verbose = config.Bind("Debug", "Verbose", false, "Toggles verbose logging");

            AnticheatPatches.AnticheatDisabled = !AnticheatEnabled.Value;
            RegisterOptions();
        }
        private static void RegisterOptions()
        {
            bool isLaunch = true;

            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
                $"{CUAchievementsPlugin.ModGUID}.GreetingsMessage",
                "Greetings message",
                "Displays a pop-up initial message",
                "Achievements",
                true,
                value => EnabledGreetingMessage.Value = value
                ));

            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
               $"{CUAchievementsPlugin.ModGUID}.RevealableHiddenAchievements",
               "Revealable hidden achievements",
               "Changes the appearance of hidden achievements: " +
               "instead of a single panel showing a count, it displays multiple panels with hidden text that are revealed when clicked.",
               "Achievements",
               true,
               value =>
               {
                   RevealableHiddenAchievements.Value = value;
                   if (!isLaunch)
                       AchievementEvents.RaiseUISettingsChanged();
               }
               ));

            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
                $"{CUAchievementsPlugin.ModGUID}.PrintScopeInAchievementList",
                "Scope in achievement list",
                "Toggles the display of the scope in the achievements list.",
                "Achievements",
                false,
                value =>
                {
                    PrintScopeInAchievementList.Value = value;
                    if (!isLaunch)
                        AchievementEvents.RaiseUISettingsChanged();
                }
                ));

            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
                $"{CUAchievementsPlugin.ModGUID}.EasterEggsEnabled",
                "Easter eggs",
                "Toggles the appearance of Easter eggs. Easter eggs do not affect gameplay in any way and are purely visual in nature.",
                "Achievements",
                true,
                value => EasterEggsEnabled.Value = value
                ));

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
                    if (!isLaunch)
                        AchievementPopupManager.DebugShow();

                    else if (EnabledGreetingMessage.Value)
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

            isLaunch = false;
        }
    }
}
