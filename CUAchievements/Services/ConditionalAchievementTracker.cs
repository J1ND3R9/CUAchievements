using System.Collections.Generic;

namespace CUAchievements.Services
{
    /// <summary>
    /// Condition tracker. Allows storing flags within an achievement without creating new variables.
    /// </summary>
    public class ConditionalAchievementTracker
    {
        private readonly Dictionary<string, bool> flags = [];
        /// <summary>
        /// Creates/sets a flag
        /// </summary>
        /// <param name="flagName">Flag name</param>
        /// <param name="value">Flag value</param>
        public void SetFlag(string flagName, bool value) => flags[flagName] = value;
        /// <summary>
        /// Retrieves the flag. If the flag is missing, it returns False.
        /// </summary>
        /// <param name="flagName">Flag name</param>
        /// <returns></returns>
        public bool GetFlag(string flagName) => flags.TryGetValue(flagName, out var v) && v;
        /// <summary>
        /// Clears the flag
        /// </summary>
        /// <param name="flagName">Flag name</param>
        public void ClearFlag(string flagName) => flags.Remove(flagName);
        /// <summary>
        /// Clears all flags for this achievement.
        /// </summary>
        public void Clear() => flags.Clear();
    }
}
