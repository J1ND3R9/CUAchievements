using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Utility
{
    internal static class UIUtility
    {
        public static (GameObject, TextMeshProUGUI) CreateText(Transform parent,
                                                 string name,
                                                 string text,
                                                 TMP_FontAsset font,
                                                 float fontSize = 12f,
                                                 TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = gameObject.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;

            return (gameObject, tmp);
        }

        public static GameObject CreateSeparator(Transform parent, float height = 2f)
        {
            GameObject separator = new GameObject("Separator", typeof(RectTransform), typeof(Image), typeof(LayoutElement));

            separator.transform.SetParent(parent, false);

            LayoutElement layout = separator.GetComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.flexibleWidth = 1f;

            return separator;
        }
    }
}
