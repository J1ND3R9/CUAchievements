using CUAchievements.Plugin;
using System.Collections.Generic;

namespace CUAchievements.Anticheat
{
    /// <summary>
    /// Part of the anti-cheat system. Keeps a registry of console commands and how safe each one is for the
    /// run's achievements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the player executes a command that is <see cref="CommandSafety.Unsafe"/> or not registered
    /// (<see cref="CommandSafety.Unknown"/>), earning achievements is blocked for the current run.
    /// </para>
    /// <para>
    /// If your plugin adds console commands, register them here so that harmless commands do not
    /// block achievements and the ones that influence a run do. Do it once at startup, for example in <c>Awake</c>.
    /// </para>
    /// <para>
    /// Not thread-safe: use it from the Unity main thread only.
    /// </para>
    /// </remarks>
    public static class ConsoleCommands
    {
        /// <summary>
        /// All registered console commands and their safety level.
        /// </summary>
        /// <remarks>
        /// A read-only live view: commands registered later appear in it. Commands that are not listed are
        /// treated as <see cref="CommandSafety.Unknown"/>.
        /// </remarks>
        public static IReadOnlyDictionary<string, CommandSafety> Commands => RegisteredCommands;
        private static readonly Dictionary<string, CommandSafety> RegisteredCommands = [];
        internal static void RegisterCUCommands()
        {
            List<string> cheatCommands =
            [
                "heal", "coagulate", "kill", "spawn", "tp", "skiplayer", "timescale", "addxp", "setbodyfield", "setlimbfield", "amputate", "unchipped", "addliquid",
                "locate", "explode", "floodfill", "starterkit", "noclip", "fullbright", "plushies", "spawncategory", "cuspawn", "settile"
            ];

            List<string> safeCommands =
            [
                "skiptext", "help", "log", "talk", "framerate", "alert", "saveandquit", "resetskills", "fucklore", "setconsoleheight", "setconsolecolor", "copylog",
                "clear", "loglocale", "nukeplayerprefs", "openfolder", "music", "bind", "echo", "ui", "errorlogging", "modlist", "bug-report", "reloadcontent",
                "autohotreload", "debugwatch"
            ];

#if DEBUG
            safeCommands.AddRange( new[] { "progress_achievement", "toggle_anticheat", "reset_cheat_flag", "unlock_achievement", "popup_achievement" });
#endif

            RegisterSafe(safeCommands);
            RegisterCheat(cheatCommands);
        }

        /// <summary>
        /// Registers a console command as <see cref="CommandSafety.Safe"/>: executing it does not affect
        /// earning achievements.
        /// </summary>
        /// <param name="commandName">The name of the console command, as the player types it.</param>
        /// <remarks>
        /// A <see langword="null"/> or empty name is ignored. If the command is already registered, the call is
        /// ignored with a warning: the first registration wins, even if it used a different
        /// <see cref="CommandSafety"/>.
        /// </remarks>
        public static void RegisterSafe(string commandName)
        {
            if (string.IsNullOrEmpty(commandName)) return;
            if (RegisteredCommands.ContainsKey(commandName))
            {
                Log.Warn($"Command \"{commandName}\" already registered. Ignored duplicate.");
                return;
            }
            RegisteredCommands[commandName] = CommandSafety.Safe;
            Log.Debug($"Console command '{commandName}' is registered as SAFE.");
        }

        /// <summary>
        /// Registers console commands as <see cref="CommandSafety.Safe"/>: executing them does not affect
        /// earning achievements.
        /// </summary>
        /// <param name="commandNames">The names of the console commands, as the player types them.</param>
        /// <remarks>
        /// A <see langword="null"/> collection is ignored. A command that is already registered is skipped with
        /// a warning: the first registration wins, even if it used a different <see cref="CommandSafety"/>.
        /// </remarks>
        public static void RegisterSafe(IEnumerable<string> commandNames)
        {
            if (commandNames == null) return;
            foreach (string commandName in commandNames)
            {
                RegisterSafe(commandName);
            }
        }

        /// <summary>
        /// Registers a console command as <see cref="CommandSafety.Unsafe"/>: executing it blocks earning
        /// achievements for the current run.
        /// </summary>
        /// <param name="commandName">The name of the console command, as the player types it.</param>
        /// <remarks>
        /// A <see langword="null"/> or empty name is ignored. If the command is already registered, the call is
        /// ignored with a warning: the first registration wins, even if it used a different
        /// <see cref="CommandSafety"/>.
        /// </remarks>
        public static void RegisterCheat(string commandName)
        {
            if (string.IsNullOrEmpty(commandName)) return;
            if (RegisteredCommands.ContainsKey(commandName))
            {
                Log.Warn($"Command \"{commandName}\" already registered. Ignored duplicate.");
                return;
            }
            RegisteredCommands[commandName] = CommandSafety.Unsafe;
            Log.Debug($"Console command '{commandName}' is registered as UNSAFE.");
        }

        /// <summary>
        /// Registers console commands as <see cref="CommandSafety.Unsafe"/>: executing them blocks earning
        /// achievements for the current run.
        /// </summary>
        /// <param name="commandNames">The names of the console commands, as the player types them.</param>
        /// <remarks>
        /// A <see langword="null"/> collection is ignored. A command that is already registered is skipped with
        /// a warning: the first registration wins, even if it used a different <see cref="CommandSafety"/>.
        /// </remarks>
        public static void RegisterCheat(IEnumerable<string> commandNames)
        {
            if (commandNames == null) return;
            foreach (string commandName in commandNames)
            {
                if (RegisteredCommands.ContainsKey(commandName))
                {
                    Log.Warn($"Command \"{commandName}\" already registered. Ignored duplicate.");
                    return;
                }
                RegisterCheat(commandName);
            }
        }
    }
}
