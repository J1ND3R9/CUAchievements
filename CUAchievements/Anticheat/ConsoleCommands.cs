using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUAchievements.Anticheat
{
    /// <summary>
    /// Operates using a list of registered console commands. A sort of "anti-cheat"
    /// </summary>
    public static class ConsoleCommands
    {
        /// <summary>
        /// Returns a read-only dictionary containing all registered commands.
        /// </summary>
        public static IReadOnlyDictionary<string, CommandSafety> Commands => RegisteredCommands;
        private static readonly Dictionary<string, CommandSafety> RegisteredCommands = new Dictionary<string, CommandSafety>();
        internal static void RegisterCUCommands()
        {
            List<string> cheatCommands = new List<string>()
            {
                "heal", "coagulate", "kill", "spawn", "tp", "skiplayer", "timescale", "addxp", "setbodyfield", "setlimbfield", "amputate", "unchipped", "addliquid",
                "locate", "explode", "floodfill", "starterkit", "noclip", "fullbright", "plushies", "spawncategory", "cuspawn", "settile"
            };

            List<string> safeCommands = new List<string>()
            {
                "skiptext", "help", "log", "talk", "framerate", "alert", "saveandquit", "resetskills", "fucklore", "setconsoleheight", "setconsolecolor", "copylog",
                "clear", "loglocale", "nukeplayerprefs", "openfolder", "music", "bind", "echo", "ui", "errorlogging", "modlist", "bug-report", "reloadcontent", 
                "autohotreload", "debugwatch"
            };

#if DEBUG
            safeCommands.AddRange( new[] { "progress_achievement", "toggle_anticheat", "reset_cheat_flag", "unlock_achievement", "popup_achievement" });
#endif

            RegisterSafe(safeCommands);
            RegisterCheat(cheatCommands);
        }
        /// <summary>
        /// Registers the command as <see cref="CommandSafety.Safe"/>.
        /// </summary>
        /// <param name="commandName">Console command name</param>
        public static void RegisterSafe(string commandName)
        {
            if (RegisteredCommands.ContainsKey(commandName))
            {
                CUAchievementsPlugin.Logger.LogWarning($"Command \"{commandName}\" already registered. Ignored duplicate.");
                return;
            }
            RegisteredCommands[commandName] = CommandSafety.Safe;
        }

        /// <summary>
        /// Registers commands as <see cref="CommandSafety.Safe"/>.
        /// </summary>
        /// <param name="commandNames">List of console commands</param>
        public static void RegisterSafe(IEnumerable<string> commandNames)
        {
            foreach (var commandName in commandNames)
            {
                if (RegisteredCommands.ContainsKey(commandName))
                {
                    CUAchievementsPlugin.Logger.LogWarning($"Command \"{commandName}\" already registered. Ignored duplicate.");
                    return;
                }
                RegisteredCommands[commandName] = CommandSafety.Safe;
            }
        }
        /// <summary>
        /// Registers the command as <see cref="CommandSafety.Unsafe"/>.
        /// </summary>
        /// <param name="commandName">Console command name</param>
        public static void RegisterCheat(string commandName)
        {
            if (RegisteredCommands.ContainsKey(commandName))
            {
                CUAchievementsPlugin.Logger.LogWarning($"Command \"{commandName}\" already registered. Ignored duplicate.");
                return;
            }
            RegisteredCommands[commandName] = CommandSafety.Unsafe;
        }
        /// <summary>
        /// Registers commands as <see cref="CommandSafety.Unsafe"/>.
        /// </summary>
        /// <param name="commandNames">List of console commands</param>
        public static void RegisterCheat(IEnumerable<string> commandNames)
        {
            foreach (var commandName in commandNames)
            {
                if (RegisteredCommands.ContainsKey(commandName))
                {
                    CUAchievementsPlugin.Logger.LogWarning($"Command \"{commandName}\" already registered. Ignored duplicate.");
                    return;
                }
                RegisteredCommands[commandName] = CommandSafety.Unsafe;
            }
        }
    }
}
