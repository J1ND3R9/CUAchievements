using CUCoreLib.Helpers;
using UnityEngine;

namespace CUAchievements.UI
{
    internal static class AchievementFrames
    {
        public static Sprite Get(AchievementDifficulty difficulty) => AssetLoader.GetCachedSprite(KeyFor(difficulty)) 
            ?? AssetLoader.GetCachedSprite("achievement_frame_normal");
        private static string KeyFor(AchievementDifficulty d)
        {
            switch (d)
            {
                case AchievementDifficulty.Difficult: return "achievement_frame_difficult";
                case AchievementDifficulty.Hard: return "achievement_frame_hard";
                default: return "achievement_frame_normal";
            }
        }
    }
}