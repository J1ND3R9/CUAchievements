namespace CUAchievements.Anticheat
{
    /// <summary>
    /// Console command safety types
    /// </summary>
    public enum CommandSafety
    {
        /// <summary>
        /// A console command that has no effect on the course of the run.
        /// </summary>
        Safe,
        /// <summary>
        /// A console command that can help or change the course of a run.
        /// </summary>
        Unsafe,
        /// <summary>
        /// Any unregistered console command. Works the same way as <see cref="Unsafe"/>.
        /// </summary>
        Unknown
    }
}
