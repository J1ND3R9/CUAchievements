using BepInEx;
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
        private readonly Harmony _harmony = new(CUAchievementsInfo.GUID);

        private const float AutoSaveInterval = 10f;
        void Awake()
        {
            Log.Init(base.Logger);

            SaveRegistry.RegisterGlobalProvider("cuachievements.runIntegrity", new RunIntegritySaveProvider());
            AchievementConfig.Initialize(Config);
            PluginAssetLoader.LoadAssets();
            ConsoleCommands.RegisterCUCommands();
#if DEBUG
            AchievementDebug.Register();
#endif
            _harmony.PatchAll();
            Log.Info("Plugin loaded!");
        }

        void Start()
        {
            StartCoroutine(AutoSaveCoroutine());
        }

        void OnApplicationQuit()
        {
            AchievementService.EndRun(); // FIXME: Нужно переделать - Достижения со Scope = Run будут очищены даже при сейве и просто выходе из игры.
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
