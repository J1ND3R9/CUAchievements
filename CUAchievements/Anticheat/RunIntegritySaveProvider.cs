using CUAchievements.UI.Menu.CustomButton;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;

namespace CUAchievements.Anticheat
{
    internal static class RunIntegrity
    {
        public static bool Cheated { get; private set; }
        public static void MarkCheated() => Cheated = true;
        public static void Reset() => Cheated = false;
    }
    internal sealed class RunIntegritySaveProvider : ICustomSaveProvider
    {
        public int GetVersion() => 1;
        
        public JToken Capture()
        {
            return new JObject { ["cheated"] = RunIntegrity.Cheated };
        }

        public void Restore(JToken payload, int version, SaveRestoreContext context)
        {
            JObject obj = payload as JObject;

            bool cheated = (bool?)obj?["cheated"] ?? false;

            RunIntegrity.Reset();

            if (cheated)
            {
                PauseMenuButtonBuilder.SetTooltip();
                RunIntegrity.MarkCheated();
            }
        }
    }
}
