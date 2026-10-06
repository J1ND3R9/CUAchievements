using System.Linq;
using TMPro;
using UnityEngine;

namespace CUAchievements.UI
{
    internal static class GameFonts
    {
        private static TMP_FontAsset retro;

        public static TMP_FontAsset Retro
        {
            get
            {
                if (retro == null)
                    retro = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                        .FirstOrDefault(f => f.name == "Retro GamingPix");
                return retro;
            }
        }
    }
}
