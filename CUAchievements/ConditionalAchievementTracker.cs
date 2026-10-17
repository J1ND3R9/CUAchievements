using CUAchievements.Registry;
using System.Collections.Generic;

namespace CUAchievements.Services
{
    /// <summary>
    /// A set of named boolean flags for achievements that depend on several conditions. It spares you from
    /// declaring a separate <see langword="bool"/> field for each condition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every <see cref="RegisteredAchievement"/> has its own tracker in
    /// <see cref="RegisteredAchievement.ConditionalTracker"/>.
    /// The whole tracker is reset according to the achievement's <see cref="AchievementScope"/>.
    /// </para>
    /// <para>
    /// The entire tracker resides solely in memory. All flags will be cleared when the game is restarted.
    /// </para>
    /// <para>
    /// Not thread-safe: use it from the Unity main thread only.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Achievement: finish a run without using painkillers.
    /// var tracker = achievement.ConditionalTracker;
    ///
    /// void OnPainkillersUsed() => tracker.SetFlag("used_painkillers", true);
    ///
    /// void OnRunFinished()
    /// {
    ///     if (!tracker.GetFlag("used_painkillers")) achievement.Unlock();
    /// }
    /// </code>
    /// </example>
    public class ConditionalAchievementTracker
    {
        private readonly Dictionary<string, bool> flags = [];

        /// <summary>
        /// Sets the value of a flag.
        /// </summary>
        /// <param name="flagName">The name of the flag. If <see langword="null"/> or empty, the call does nothing.</param>
        /// <param name="value">The value to store.</param>
        public void SetFlag(string flagName, bool value)
        {
            if (string.IsNullOrEmpty(flagName)) return;
            flags[flagName] = value;
        }

        /// <summary>
        /// Gets the value of a flag.
        /// </summary>
        /// <param name="flagName">The name of the flag. If <see langword="null"/> or empty, the result is <see langword="false"/>.</param>
        /// <returns>The value of the flag, or <see langword="false"/> if the flag has not been set.</returns>
        public bool GetFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName)) return false;
            return flags.TryGetValue(flagName, out var v) && v;
        }

        /// <summary>
        /// Removes a flag. Afterwards <see cref="GetFlag(string)"/> returns <see langword="false"/> for it.
        /// </summary>
        /// <param name="flagName">The name of the flag. If <see langword="null"/> or empty, the call does nothing.</param>
        public void ClearFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName)) return;
            flags.Remove(flagName);
        }

        /// <summary>
        /// Removes all flags.
        /// </summary>
        public void Clear() => flags.Clear();
    }
}
