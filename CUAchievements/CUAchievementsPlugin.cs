using BepInEx;
using BepInEx.Logging;
using CUAchievements.Assets;
using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUAchievements.Services;
using CUAchievements.UI;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CUAchievements
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)]
    public class CUAchievementsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.github.j1nd3r9.cuachievements";
        public const string ModName = "CUAchievements";
        public const string ModVersion = "1.0.0.0";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony(ModGUID);
        public static CUAchievementsPlugin Instance { get; private set; } = null;
        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            PluginAssetLoader.LoadAssets();
            _harmony.PatchAll();
            RegisterModOptions();
            RegisterDebugShit();
            Logger.LogInfo("Plugin loaded!");
        }
        private void RegisterModOptions()
        {
            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
               "cuachievements.achievements.enabled",
               "Enable achievements",
               "Enable achievements",
               "Achievements",
               true,
               value =>
               {
                   PlayerPrefs.SetInt("Achievements_Enabled", value ? 1 : 0);
                   PlayerPrefs.Save();
               }
           ));
            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
               "cuachievements.achievements.alerttype",
               "Alert type",
               "Alert type",
               "Achievements",
               PlayerPrefs.GetInt("Achievements_AlertType", 0),
               new[]
               {
                   new ModDropdownChoice("popup", "Pop-up"),
                   new ModDropdownChoice("default", "Default alert. No icon"),
                   new ModDropdownChoice("hidden", "Hidden")
               },
               value =>
               {
                   PlayerPrefs.SetInt("Achievements_AlertType", value);
                   PlayerPrefs.Save();
               }
           ));

            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
               "cuachievements.achievements.achievementdisplay",
               "Achievement display",
               "Achievement display (only work on popup achievements)",
               "Achievements",
               PlayerPrefs.GetInt("Achievements_AchievementDisplay", 0),
               new[]
               {
                   new ModDropdownChoice("bottom", "Bottom-side"),
                   new ModDropdownChoice("left","Left-side"),
                   new ModDropdownChoice("up", "Up-side")
               },
               value =>
               {
                   PlayerPrefs.SetInt("Achievements_AchievementDisplay", value);
                   PlayerPrefs.Save();
                   AchievementPopupManager.DebugShow();
               }
           ));

            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
              "cuachievements.achievements.achievementframe",
              "Achievement frame",
              "Achievement frame (only work on popup achievements)",
              "Achievements",
              PlayerPrefs.GetInt("Achievements_AchievementFrame", 0),
              new[]
              {
                   new ModDropdownChoice("v1", "V1"),
                   new ModDropdownChoice("v1alt", "V1 Alt BG"),
                   new ModDropdownChoice("v2", "V2"),
                   new ModDropdownChoice("v2alt", "V2 Alt BG"),
                   new ModDropdownChoice("v3", "V3"),
                   new ModDropdownChoice("v3alt", "V3 Alt BG"),
              },
              value =>
              {
                  PlayerPrefs.SetInt("Achievements_AchievementFrame", value);
                  PlayerPrefs.Save();
                  AchievementPopupManager.DebugShow();
              }
          ));
        }

        internal void RegisterDebugShit()
        {
            var achievements = Services.CUAchievements.Create(ModGUID);
            for (int i = 0; i < 5; i++)
                achievements.Register(AchievementDefinition.Instant($"testInstant{i}", $"Крутое достижение #{i}", "Банальные, но неопровержимые выводы, а также представители современных социальных резервов, вне зависимости от их уровня, должны быть заблокированы в рамках своих собственных рациональных ограничений. Следует отметить, что консультация с широким активом в значительной степени обусловливает важность системы обучения кадров, соответствующей насущным потребностям. Мы вынуждены отталкиваться от того, что внедрение современных методик однозначно определяет каждого участника как способного принимать собственные решения касаемо укрепления моральных ценностей.", null));
            for (int i = 0; i < 5; i++)
                achievements.Register(AchievementDefinition.Counter($"testCounter{i}", $"Крутое достижение СЧЁТЧИК #{i}", "bruh", null, 10));
            ConsoleCommandRegistry.Register(
                "progress_achievement",
                "Add progress to achievement",
                args =>
                {
                    if (args.Length < 2) return;
                    if (string.IsNullOrEmpty(args[1]))
                        return;
                    achievements.AddProgress(args[1]);
                },
                null,
                ("id", "Achievement ID")
                );

            ConsoleCommandRegistry.Register(
                "popup_achievement",
                "Force popup for achievement",
                args =>
                {
                    if (args.Length < 2) return;
                    if (string.IsNullOrEmpty(args[1]))
                        return;

                    if (achievements.TryGet(args[1], out var ach))
                        AchievementPopupManager.Show(ach.Definition);
                },
                null,
                ("id", "Achievement ID")
                );


            var eat100foodAchievemnt = achievements.Register(AchievementDefinition.Counter("eat_100_food", "сожри его ебло тупое", "qwada", null, 100));
        }
    }
}
