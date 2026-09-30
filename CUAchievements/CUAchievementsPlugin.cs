using BepInEx;
using BepInEx.Logging;
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
        private void Awake()
        {
            Logger = base.Logger;

            PluginAssetLoader.LoadAssets();
            _harmony.PatchAll();

            AchievementOptions.Register();

#if DEBUG
            AchievementDebug.Register();
#endif
            Logger.LogInfo("Plugin loaded!");
        }

        private void Start()
        {
            StartCoroutine(AutoSaveCoroutine());
        }

        private void OnApplicationQuit()
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
