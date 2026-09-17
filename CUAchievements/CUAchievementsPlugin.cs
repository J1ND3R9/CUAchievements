using BepInEx;
using BepInEx.Logging;
using CUAchievements.Assets;
using CUAchievements.Definitions;
using CUAchievements.Persistence;
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
    public class CUAchievementsPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.github.j1nd3r9.cuachievements";
        public const string ModName = "CUAchievements";
        public const string ModVersion = "1.0.0.0";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony(ModGUID);
        public static CUAchievementsPlugin Instance { get; private set; } = null;

        private const float AutoSaveInterval = 10f;
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

        private void Start()
        {
            StartCoroutine(AutoSaveCoroutine());
        }

        private void OnApplicationQuit()
        {
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
        private void RegisterModOptions()
        {
            ModOptionsRegistry.Register(ModOptionDefinition.Bool(
               "cuachievements.enabled",
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
               "cuachievements.alerttype",
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
               "cuachievements.achievementdisplay",
               "Achievement display",
               "Achievement display (only work on popup achievements)",
               "Achievements",
               PlayerPrefs.GetInt("Achievements_AchievementDisplay", 0),
               new[]
               {
                   new ModDropdownChoice("bottom", "Down > Up"),
                   new ModDropdownChoice("left","Right > Left"),
                   new ModDropdownChoice("up", "Up > Down")
               },
               value =>
               {
                   PlayerPrefs.SetInt("Achievements_AchievementDisplay", value);
                   PlayerPrefs.Save();
                   AchievementPopupManager.DebugShow();
               }
           ));

            //  ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            //    "cuachievements.achievementpopupstyle",
            //    "Achievement popup style",
            //    "Achievement popup style",
            //    "Achievements",
            //    PlayerPrefs.GetInt("Achievements_AchievementPopupStyle", 0),
            //    new[]
            //    {
            //         new ModDropdownChoice("v1", "V1"),
            //         new ModDropdownChoice("v2", "V2"),
            //         new ModDropdownChoice("v3", "V3")
            //    },
            //    value =>
            //    {
            //        PlayerPrefs.SetInt("Achievements_AchievementPopupStyle", value);
            //        PlayerPrefs.Save();
            //        AchievementPopupManager.DebugShow();
            //    }
            //));

            //  ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            //    "cuachievements.achievementsound",
            //    "Achievement sound",
            //    "Achievement sound (only work on popup achievements)",
            //    "Achievements",
            //    PlayerPrefs.GetInt("Achievements_AchievementUnlockSound", 0),
            //    new[]
            //    {
            //         new ModDropdownChoice("lvlup", "Lvl-Up"),
            //    },
            //    value =>
            //    {
            //        PlayerPrefs.SetInt("Achievements_AchievementUnlockSound", value);
            //        PlayerPrefs.Save();
            //        CUCoreUtils.PlaySoundAt(AssetLoader.GetCachedAudioClip($"unlock{value}"), volume: 0.5f);
            //    }
            //));

            ModOptionsRegistry.Register(ModOptionDefinition.Float(
                "cuachievements.achievementsoundvolume",
                "Achievement unlock volume",
                "Achievement unlock volume",
                "Achievements",
                PlayerPrefs.GetFloat("Achievements_UnlockVolume", 0.8f),
                0f,
                1f,
                value =>
                {
                    PlayerPrefs.SetFloat("Achievements_UnlockVolume", value);
                    PlayerPrefs.Save();
                },
                value => Mathf.RoundToInt(value * 100f) + "%"
                ));
        }

        internal void RegisterDebugShit()
        {
            var achievements = Services.CUAchievements.Create(ModGUID);

            for (int i = 1; i < 5; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testCounter{i}", "Ачивка счётчик", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 10, null));
            }

            for (int i = 1; i < 5; i++)
            {
                achievements.Register(AchievementDefinition.Instant($"testInstant{i}", "Ачивка", "Приятно, граждане, наблюдать, как сторонники тоталитаризма в науке освещают чрезвычайно интересные особенности картины в целом, однако конкретные выводы, разумеется, заблокированы в рамках своих собственных рациональных ограничений. Но высокое качество позиционных исследований не оставляет шанса для инновационных методов управления процессами.", null));
            }

            for (int i = 1; i < 3; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testCounterDifficult{i}", "Сложная ачивка", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 3, null, null, AchievementDifficulty.Difficult));
            }

            for (int i = 1; i < 3; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testCounterHard{i}", "Оч. сложная ачивка", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 3, null, null, AchievementDifficulty.Hard));
            }

            for (int i = 1; i < 3; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testHidden{i}", "Скрытая ачивка", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 3, null, null, AchievementDifficulty.Hard, null, true));
            }

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
                        AchievementPopupManager.Show(ach);
                },
                null,
                ("id", "Achievement ID")
                );
        }
    }
}
