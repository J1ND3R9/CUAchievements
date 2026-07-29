using BepInEx;
using BepInEx.Logging;
using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUAchievements.Services;
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
            RegisterModOptions();
            Logger.LogInfo("Plugin loaded!");
        }
        private void RegisterModOptions()
        {
            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
               "cuachievements.achievements.enabled",
               LocaleRegistry.Get("gamesetcuachievements.achievements.enabled", "Enable achievements"),
               LocaleRegistry.Get("gamesetcuachievements.achievements.enableddsc", "Enable achievements"),
               LocaleRegistry.Get("gamesetcuachievements.achievements.category", "Achievements"),
               true,
               value =>
               {
                   PlayerPrefs.SetInt("Achievements_Enabled", value ? 1 : 0);
                   PlayerPrefs.Save();
               }
           ));
            ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
               "cuachievements.achievements.alerttype",
               LocaleRegistry.Get("gamesetcuachievements.achievements.alerttype", "Alert type"),
               LocaleRegistry.Get("gamesetcuachievements.achievements.alerttypedsc", "Alert type"),
               LocaleRegistry.Get("gamesetcuachievements.achievements.category", "Achievements"),
               PlayerPrefs.GetInt("Achievements_AlertType", 0),
               new[]
               {
                   new ModDropdownChoice("steam", LocaleRegistry.Get("gamesetcuachievements.achievements.alerttypesteam", "Steam-Like")),
                   new ModDropdownChoice("default", LocaleRegistry.Get("gamesetcuachievements.achievements.alerttypedefault", "Default alert")),
                   new ModDropdownChoice("hidden", LocaleRegistry.Get("gamesetcuachievements.achievements.alerttypehidden", "Hidden"))
               },
               value =>
               {
                   PlayerPrefs.SetInt("Achievements_AlertType", value);
                   PlayerPrefs.Save();
               }
           ));
        }
    }
}
