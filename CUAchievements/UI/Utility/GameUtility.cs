using CUAchievements.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUAchievements.UI.Utility
{
    internal static class GameUtility
    {
        public static bool IsTutorialOrSandbox() => 
            WorldGeneration.world.biomeOverride == WorldGeneration.OverrideSceneType.Tutorial || AchievementConfig.AllowSandboxAchievements.Value;
    }
}
