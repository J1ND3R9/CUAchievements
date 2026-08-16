using System.Collections.Generic;

namespace CUAchievements.Services
{
    public class ConditionalAchievementTracker
    {
        private readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

        public void SetFlag(string flagName, bool value) => flags[flagName] = value;
        public bool GetFlag(string flagName) => flags.TryGetValue(flagName, out var v) && v;
        public void ClearFlag(string flagName) => flags.Remove(flagName);
    }
}
