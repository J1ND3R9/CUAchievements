using CUAchievements.Registry;
using System;
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
    /// With <see cref="AchievementScope.Run"/>, flags are stored in the game save together with the run. With
    /// <see cref="AchievementScope.Lifetime"/>, they are stored with the player's achievement progress and stay
    /// set until your plugin clears them, so use <see cref="AchievementScope.Run"/> for conditions of a single run.
    /// </para>
    /// <para>
    /// Not thread-safe: use it from the Unity main thread only.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Achievement with AchievementScope.Run: finish a run without using painkillers.
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

        internal Func<bool> CanChange;
        internal Action Changed;

        /// <summary>
        /// Sets the value of a flag.
        /// </summary>
        /// <param name="flagName">The name of the flag. If <see langword="null"/> or empty, the call does nothing.</param>
        /// <param name="value">The value to store.</param>
        /// <remarks>
        /// With <see cref="AchievementScope.Lifetime"/>, the call does nothing in the same cases as
        /// <see cref="RegisteredAchievement.AddProgress(float)"/>: the current run is marked as cheated, or the
        /// player is in the tutorial or sandbox and the achievement can't be earned there.
        /// </remarks>
        public void SetFlag(string flagName, bool value)
        {
            if (string.IsNullOrEmpty(flagName)) return;
            if (CanChange != null && !CanChange()) return;
            if (flags.TryGetValue(flagName, out bool current) && current == value) return;

            flags[flagName] = value;
            Changed?.Invoke();
        }

        /// <summary>
        /// Gets the value of a flag.
        /// </summary>
        /// <param name="flagName">The name of the flag. If <see langword="null"/> or empty, the result is <see langword="false"/>.</param>
        /// <returns>The value of the flag, or <see langword="false"/> if the flag has not been set.</returns>
        public bool GetFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName)) return false;
            return flags.TryGetValue(flagName, out bool v) && v;
        }

        /// <summary>
        /// Removes a flag. Afterwards <see cref="GetFlag(string)"/> returns <see langword="false"/> for it.
        /// </summary>
        /// <param name="flagName">The name of the flag. If <see langword="null"/> or empty, the call does nothing.</param>
        public void ClearFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName)) return;
            if (flags.Remove(flagName)) Changed?.Invoke();
        }

        /// <summary>
        /// Removes all flags.
        /// </summary>
        public void Clear()
        {
            if (flags.Count == 0) return;

            flags.Clear();
            Changed?.Invoke();
        }

        internal IReadOnlyDictionary<string, bool> Snapshot() => flags;

        internal void Load(Dictionary<string, bool> saved)
        {
            flags.Clear();

            if (saved == null) return;

            foreach (KeyValuePair<string, bool> flag in saved)
            {
                flags[flag.Key] = flag.Value;
            }
        }
    }
}
