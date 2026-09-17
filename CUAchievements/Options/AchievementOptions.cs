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
    internal static class AchievementOptions
    {
        internal static void Register()
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
    }
}
