using CUAchievements.Options;

namespace CUAchievements.UI.Utility
{
    internal static class GameUtility
    {
        public static bool IsTutorialOrSandbox() => 
            WorldGeneration.world.biomeOverride == WorldGeneration.OverrideSceneType.Tutorial && !AchievementConfig.AllowSandboxAchievements.Value;
    }
}
