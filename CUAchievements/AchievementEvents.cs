using CUAchievements.Plugin;
using CUAchievements.Registry;
using System;

namespace CUAchievements
{
    /// <summary>
    /// Events raised by CUAchievements.
    /// </summary>
    public static class AchievementEvents
    {
        /// <summary>
        /// Raised after the state of any achievement changes: its progress was updated or it was unlocked.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The event carries no data. Read the current state from
        /// <see cref="RegisteredAchievement"/>.
        /// </para>
        /// <para>
        /// It is raised synchronously on the thread that changed the achievement (normally the Unity main thread),
        /// so handlers should be fast. Do not change achievement progress from a handler: that raises the event again
        /// from inside the handler.
        /// </para>
        /// <para>
        /// An exception thrown by a handler is caught and logged; the other handlers still run and the code
        /// that changed the achievement is not affected.
        /// </para>
        /// <para>
        /// Subscribe and unsubscribe symmetrically (for example in <c>OnEnable</c> and <c>OnDisable</c>),
        /// otherwise handlers of destroyed objects stay subscribed.
        /// </para>
        /// </remarks>
        public static event Action AchievementsChanged;

        internal static void RaiseAchievementsChanged()
        {
            var handler = AchievementsChanged;
            if (handler == null) return;

            foreach (Delegate d in handler.GetInvocationList())
            {
                try { ((Action)d)(); }
                catch (Exception e)
                {
                    CUAchievementsPlugin.Logger.LogError($"AchievementsChanged subscriber failed: {e}");
                }
            }
        }
    }
}
