using CUAchievements.UI.Menu.CustomButton;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CUAchievements.Anticheat
{
    [HarmonyPatch]
    internal static class AnticheatPatches
    {
        public static bool AnticheatDisabled = false;

        [HarmonyPatch(typeof(Command), MethodType.Constructor,
        [
            typeof(string),
            typeof(string),
            typeof(Command.Action),
            typeof(Dictionary<int, List<string>>),
            typeof(ValueTuple<string, string>[])
        ])]
        [HarmonyPostfix]
        static void CommandPostfix(Command __instance)
        {
            if (AnticheatDisabled) return;

            var safetyDesc = GetDescriptionAddition(CommandSafety.Unknown);
            if (ConsoleCommands.Commands.TryGetValue(__instance.name, out var safety) && safety != CommandSafety.Unknown)
                safetyDesc = GetDescriptionAddition(safety);

            __instance.description += $"\nThe console command is marked as: {safetyDesc}";
        }

        [HarmonyPatch(typeof(ConsoleScript), "TryExecuteCommand")]
        [HarmonyPrefix]
        static void TryExecutePrefix(string[] args)
        {
            if (AnticheatDisabled) return;

            if (WorldGeneration.world != null)
            {
                if (GameUtility.IsTutorialOrSandbox())
                    return;
            }

            if (args.Length == 0 || string.IsNullOrEmpty(args[0])) return;

            Command command = ConsoleScript.SearchExact(args[0]);

            if (command == null) return;

            if (!CUCoreUtils.IsInWorld()) return;

            if (ConsoleCommands.Commands.TryGetValue(args[0], out var safety) && safety == CommandSafety.Safe) return;

            if (RunIntegrity.Cheated) return;

            RunIntegrity.MarkCheated();
            CUCoreUtils.ConsoleLog(ConsoleScript.instance, "This run is marked as \"cheated.\" Achievements cannot be earned for the remainder of the run.");
            PauseMenuButtonBuilder.SetTooltip();
        }

        static string GetDescriptionAddition(CommandSafety safety)
        {
            switch (safety)
            {
                case CommandSafety.Safe:
                    return "SAFE.";
                case CommandSafety.Unsafe:
                    return "UNSAFE. Using it will prevent you from earning achievements during this run!";
                case CommandSafety.Unknown:
                    return "UNKNOWN. Using it will prevent you from earning achievements during this run!";
                default:
                    break;
            }
            return "UNKNOWN. Using it will prevent you from earning achievements during this run!";
        }
    }
}