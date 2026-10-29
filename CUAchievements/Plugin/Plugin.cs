using BepInEx;
using BepInEx.Logging;
using CUAchievements.Anticheat;
using CUAchievements.Assets;
using CUAchievements.Options;
using CUAchievements.Persistence;
using CUAchievements.Services;
using CUCoreLib.Registries;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace CUAchievements.Plugin
{
    [BepInPlugin(CUAchievementsInfo.GUID, CUAchievementsInfo.Name, CUAchievementsInfo.Version)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private readonly Harmony _harmony = new(CUAchievementsInfo.GUID);

        private const float AutoSaveInterval = 10f;
        void Awake()
        {
            Logger = base.Logger;

            SaveRegistry.RegisterGlobalProvider("cuachievements.runIntegrity", new RunIntegritySaveProvider());
            AchievementConfig.Initialize(Config);
            PluginAssetLoader.LoadAssets();
            ConsoleCommands.RegisterCUCommands();
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
            WaitForSecondsRealtime wait = new(AutoSaveInterval);

            while (true)
            {
                yield return wait;
                AchievementPersistence.SaveDirty();
            }
        }
    }
}
