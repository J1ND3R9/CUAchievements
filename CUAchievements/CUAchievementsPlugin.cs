using BepInEx;
using BepInEx.Logging;
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
            LoadModAssets();
            _harmony.PatchAll();
            RegisterModOptions();
            RegisterDebugShit();
            Logger.LogInfo("Plugin loaded!");
        }
        private void LoadModAssets()
        {
            Sprite achievementButtonIcon = AssetLoader.LoadEmbeddedSprite("Assets.Images.UI.achievement_button.png", 100f);
            Sprite achievementButtonIconOverlay = AssetLoader.LoadEmbeddedSprite("Assets.Images.UI.achievement_button_overlay.png", 100f); // НЕ РАБОТАЕТ!!! РЕШИ ПРОБЛЕМУ, УБЛЮДОК

            Sprite achievementIconPlaceholder = AssetLoader.LoadEmbeddedSprite("Assets.Images.achievement_default_sprite.png", 100f);
            Sprite achievementLockedIcon = AssetLoader.LoadEmbeddedSprite("Assets.Images.achievement_default_locked.png", 100f);

            Sprite achievementPopupFrameV1 = AssetLoader.LoadEmbeddedSprite("Assets.Images.PopUp.achievement_popup_frame_v1.png", 100f);
            Sprite achievementPopupFrameV1Alt = AssetLoader.LoadEmbeddedSprite("Assets.Images.PopUp.achievement_popup_frame_v1alt.png", 100f);
            Sprite achievementPopupFrameV2 = AssetLoader.LoadEmbeddedSprite("Assets.Images.PopUp.achievement_popup_frame_v2.png", 100f);
            Sprite achievementPopupFrameV2Alt = AssetLoader.LoadEmbeddedSprite("Assets.Images.PopUp.achievement_popup_frame_v2alt.png", 100f);
            Sprite achievementPopupFrameV3 = AssetLoader.LoadEmbeddedSprite("Assets.Images.PopUp.achievement_popup_frame_v3.png", 100f);
            Sprite achievementPopupFrameV3Alt = AssetLoader.LoadEmbeddedSprite("Assets.Images.PopUp.achievement_popup_frame_v3alt.png", 100f);

            Sprite achievementImageFrame = AssetLoader.LoadEmbeddedSprite("Assets.Images.UI.achievement_frame_image.png", 100f);

            Sprite achievementCommonFrame = AssetLoader.LoadEmbeddedSprite("Assets.Images.UI.achievement_frame_common.png", 100f);
            Sprite achievementCommonFrameSliced = SpriteSlicer.MakeSliced(achievementCommonFrame, 6f);


            AudioClip achievementPopUpAudio = AssetLoader.LoadEmbeddedAudio("Assets.Audio.achievement_sound_unlocked.mp3");

            AssetLoader.CacheSprite("achievement_button", achievementButtonIcon);
            AssetLoader.CacheSprite("achievement_button_overlay", achievementButtonIconOverlay);
            AssetLoader.CacheSprite("achievement_default_sprite", achievementIconPlaceholder);
            AssetLoader.CacheSprite("achievement_default_locked", achievementLockedIcon);
            AssetLoader.CacheSprite("achievement_popup_frame_v1", achievementPopupFrameV1);
            AssetLoader.CacheSprite("achievement_popup_frame_v1alt", achievementPopupFrameV1Alt);
            AssetLoader.CacheSprite("achievement_popup_frame_v2", achievementPopupFrameV2);
            AssetLoader.CacheSprite("achievement_popup_frame_v2alt", achievementPopupFrameV2Alt);
            AssetLoader.CacheSprite("achievement_popup_frame_v3", achievementPopupFrameV3);
            AssetLoader.CacheSprite("achievement_popup_frame_v3alt", achievementPopupFrameV3Alt);
            AssetLoader.CacheSprite("achievement_frame_image", achievementImageFrame);
            AssetLoader.CacheSprite("achievement_frame_common", achievementCommonFrameSliced);

            AssetLoader.CacheAudioClip("achievement_sound_unlocked", achievementPopUpAudio);
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
                achievements.Registry.Register(AchievementDefinition.Instant($"testInstant{i}", $"Крутое достижение #{i}", "Банальные, но неопровержимые выводы, а также представители современных социальных резервов, вне зависимости от их уровня, должны быть заблокированы в рамках своих собственных рациональных ограничений. Следует отметить, что консультация с широким активом в значительной степени обусловливает важность системы обучения кадров, соответствующей насущным потребностям. Мы вынуждены отталкиваться от того, что внедрение современных методик однозначно определяет каждого участника как способного принимать собственные решения касаемо укрепления моральных ценностей.", null));
            for (int i = 0; i < 5; i++)
                achievements.Registry.Register(AchievementDefinition.Counter($"testCounter{i}", $"Крутое достижение СЧЁТЧИК #{i}", "bruh", null, 10));
            ConsoleCommandRegistry.Register(
                "progress_achievement",
                "Add progress to achievement",
                args =>
                {
                    if (args.Length < 2) return;
                    if (string.IsNullOrEmpty(args[1]))
                        return;
                    achievements.Achievements.AddProgress(args[1]);
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

                    if (achievements.Registry.TryGet(args[1], out var ach))
                        AchievementPopupManager.Show(ach.Definition);
                },
                null,
                ("id", "Achievement ID")
                );
        }
    }
}
