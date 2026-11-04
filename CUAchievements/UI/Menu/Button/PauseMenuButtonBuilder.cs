using BepInEx.Bootstrap;
using CUAchievements.Persistence;
using CUAchievements.Plugin;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.CustomButton
{
    internal class PauseMenuButtonBuilder : MonoBehaviour
    {
        public const string ButtonName = "Plush";

        private static UITooltip buttonTooltip;
        public static RectTransform CreateButton(Transform parent, RectTransform buttonTemplate)
        {
            Sprite sprite = AssetLoader.GetCachedSprite("achievement_pause_button");

            if (sprite == null)
            {
                Log.Error("achievement_pause_button sprite not found.");
                return null;
            }

            GameObject buttonObject = Instantiate(buttonTemplate, parent).gameObject;

            buttonObject.name = "Plush";

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = sprite;

            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchoredPosition = new Vector2(380f, -180f);

            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.qol_unknown"))
                rect.anchoredPosition = new Vector2(480f, -300f);

            buttonTooltip = buttonObject.GetComponent<UITooltip>();

            SetTooltip();

            return (RectTransform)buttonObject.transform;
        }

        public static void SetTooltip()
        {
            string tipName = UIText.Achievements.Resolve();
            string tipDesc = UIText.PauseButtonDefaultTipDescription.Resolve();

            buttonTooltip.skipLocale = true;
            buttonTooltip.tipName = tipName;
            buttonTooltip.tipDesc = tipDesc;

            if (GameUtility.IsTutorialOrSandbox())
            {
                string tipDescAdditional = UIText.PauseButtonTutorialTipDescription.Resolve();
                buttonTooltip.tipDesc += $"\n\n{tipDescAdditional}";
            }
            else if (RunIntegrity.Cheated)
            {
                string tipDescAdditional = UIText.PauseButtonCheatedTipDescription.Resolve();
                buttonTooltip.tipDesc += $"\n\n{tipDescAdditional}";
            }
        }
    }
}