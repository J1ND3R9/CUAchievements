using BepInEx.Logging;
using CUAchievements.Options;

namespace CUAchievements.Plugin
{
    internal static class Log
    {
        private static ManualLogSource _source;
        public static void Init(ManualLogSource source) { _source = source; }
        public static void Info(string message) { _source?.LogInfo(message); }
        public static void Warn(string message) { _source?.LogWarning(message); }
        public static void Error(string message) { _source?.LogError(message); }
        public static void Debug(string message)
        {
            if (AchievementConfig.Verbose != null && AchievementConfig.Verbose.Value)
                _source?.LogInfo($"[DEBUG] {message}");
        }
    }
}
