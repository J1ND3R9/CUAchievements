namespace CUAchievements.Anticheat
{
    /// <summary>
    /// Determines how a console command affects earning achievements.
    /// </summary>
    /// <remarks>
    /// Commands are registered with <see cref="ConsoleCommands"/>.
    /// </remarks>
    public enum CommandSafety
    {
        /// <summary>
        /// A console command that has no effect on the course of the run. Executing it <b>does not</b>
        /// affect earning achievements.
        /// </summary>
        Safe,
        /// <summary>
        /// A console command that can help or change the course of a run. Executing it <b>blocks</b>
        /// earning achievements for this run.
        /// </summary>
        Unsafe,
        /// <summary>
        /// Any console command that has not been registered, for example one added by a plugin that does
        /// not use <see cref="ConsoleCommands"/>. Executing it <b>blocks</b> earning achievements for this run.
        /// </summary>
        Unknown
    }
}
