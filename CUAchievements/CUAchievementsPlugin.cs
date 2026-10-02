using BepInEx;
using BepInEx.Logging;
using CUAchievements.Anticheat;
using CUAchievements.Assets;
using CUAchievements.Debug;
using CUAchievements.Definitions;
using CUAchievements.Options;
using CUAchievements.Persistence;
using CUAchievements.Services;
using CUAchievements.UI;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace CUAchievements
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)]
    internal class CUAchievementsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.cuachievements";
        public const string ModName = "CUAchievements";
        public const string ModVersion = "0.1.0.0";

        internal static new ManualLogSource Logger;

        private readonly Harmony _harmony = new Harmony(ModGUID);

        private const float AutoSaveInterval = 10f;
        void Awake()
        {
            Logger = base.Logger;

            SaveRegistry.RegisterGlobalProvider("cuachievements.runIntegrity", new RunIntegritySaveProvider());
            PluginAssetLoader.LoadAssets();
            ConsoleCommands.RegisterCUCommands();
            AchievementOptions.Register();
#if DEBUG
            AchievementDebug.Register();
#endif
            _harmony.PatchAll();
            Logger.LogInfo("Plugin loaded!");
        }

        void Start()
        {
            StartCoroutine(AutoSaveCoroutine());
        }

        void OnApplicationQuit()
        {
            AchievementService.EndRun();
            AchievementPersistence.SaveDirty();
        }

        private IEnumerator AutoSaveCoroutine()
        {
            WaitForSecondsRealtime wait = new WaitForSecondsRealtime(AutoSaveInterval);

            while (true)
            {
                yield return wait;
                AchievementPersistence.SaveDirty();
            }
        }
    }
}
