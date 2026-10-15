using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CUAchievements.UI.Utility
{
    internal static class UIUtility
    {
        public static GameObject ButtonTemplate { get; set; }
        public static GameObject Create(string name, Transform parent, params Type[] components)
        {
            var types = new Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            components.CopyTo(types, 1);

            var gameObject = new GameObject(name, types);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }
        public static RectTransform Stretch(this Transform transform, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static T Configure<T>(this T g,
                                     float spacing = 0f,
                                     bool controlW = true,
                                     bool controlH = true,
                                     bool expandW = false,
                                     bool expandH = false,
                                     TextAnchor? align = null,
                                     RectOffset padding = null)
            where T : HorizontalOrVerticalLayoutGroup
        {
            g.spacing = spacing;

            g.childControlWidth = controlW;
            g.childControlHeight = controlH;
            g.childForceExpandWidth = expandW;
            g.childForceExpandHeight = expandH;

            if (align.HasValue) g.childAlignment = align.Value;
            if (padding != null) g.padding = padding;
            return g;
        }

        public static TMP_InputField CreateInputField(string name, Transform parent, string placeholder, TMP_FontAsset font, bool interactable = true, UnityAction<string> onValueChanged = null)
        {
            GameObject inputObject = Create(name, parent, typeof(Image));
            inputObject.SetActive(false);

            var inputImage = inputObject.GetComponent<Image>();
            inputImage.type = Image.Type.Sliced;
            inputImage.sprite = ButtonTemplate?.GetComponent<Image>().sprite;

            GameObject viewport = Create("TextViewport", inputObject.transform, typeof(RectMask2D));
            RectTransform viewportRect = viewport.transform.Stretch(new Vector2(8f, 4f), new Vector2(-8f, -4f));

            TextMeshProUGUI text = CreateText(viewport.transform, "Text", "", font, 16f, TextAlignmentOptions.MidlineLeft);
            text.transform.Stretch();
            text.enableWordWrapping = false;
            text.raycastTarget = false;

            TextMeshProUGUI hint = CreateText(viewport.transform, "Placeholder", placeholder, font, 16f, TextAlignmentOptions.MidlineLeft);
            hint.transform.Stretch();
            hint.color = new Color(1f, 1f, 1f, 0.5f);
            hint.fontStyle = FontStyles.Italic;
            hint.raycastTarget = false;

            var input = inputObject.AddComponent<TMP_InputField>();
            input.textViewport = viewportRect;
            input.interactable = interactable;
            input.textComponent = text;
            input.placeholder = hint;
            input.fontAsset = font;
            input.pointSize = 16f;

            if (onValueChanged != null) input.onValueChanged.AddListener(onValueChanged);

            inputObject.SetActive(true);
            return input;
        }

        public static (TextMeshProUGUI labelTmp, TextMeshProUGUI valueTmp) CreateStatisticRow(Transform parent, string name, string label, string value)
        {
            GameObject row = Create(name, parent, typeof(HorizontalLayoutGroup));
            row.GetComponent<HorizontalLayoutGroup>().Configure(expandH: true);

            TextMeshProUGUI labelTmp = CreateText(row.transform, "Label", label, GameFonts.Retro, 18f);
            CreateSpacer(row.transform);
            TextMeshProUGUI valueTmp = CreateText(row.transform, "Value", value, GameFonts.Retro, 18f);

            return (labelTmp, valueTmp);
        }

        public static Button CreateButton(Transform parent, string name, string label, bool interactable = true, UnityAction onClick = null)
        {
            if (ButtonTemplate == null)
            {
                CUAchievementsPlugin.Logger.LogError("UIUtility.ButtonTemplate is not set.");
                return null;
            }

            GameObject buttonObject = UnityEngine.Object.Instantiate(ButtonTemplate, parent, false);
            buttonObject.name = name;

            Button button = buttonObject.GetComponent<Button>();
            if (button == null) button = buttonObject.GetComponentInChildren<Button>(true);
            if (button == null)
            {
                UnityEngine.Object.Destroy(buttonObject);
                return null;
            }

            button.onClick = new Button.ButtonClickedEvent();
            if (onClick != null) button.onClick.AddListener(onClick);
            button.interactable = interactable;

            TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
            tmp?.text = label;

            return button;
        }

        public static GameObject CreatePanel(string name, Transform parent, Vector2 thickness = default)
        {
            GameObject panelObject = Create(name, parent, typeof(Image), typeof(Outline));

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = Color.black;

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = Color.white;
            panelOutline.effectDistance = thickness == default ? new Vector2(3f, 3f) : thickness;

            return panelObject;
        }
        public static T GetOrAdd<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component ?? go.AddComponent<T>();
        }

        public static void ClearChildren(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }

        public static VerticalLayoutGroup SetupScrollContent(this RectTransform content, float spacing = 24f, RectOffset padding = null)
        {
            if (content == null) return null;

            content.ClearChildren();

            content.gameObject.GetOrAdd<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return content.gameObject.GetOrAdd<VerticalLayoutGroup>().Configure(
                spacing: spacing,
                expandW: true,
                expandH: true,
                padding: padding ?? new RectOffset(10, 10, 10, 10));
        }

        public static ScrollRect HideScrollbar(this ScrollRect scrollRect)
        {
            if (scrollRect == null) return null;

            scrollRect.viewport.offsetMax = Vector2.zero;
            scrollRect.verticalScrollbar?.GetComponent<Image>()?.enabled = false;
            scrollRect.verticalScrollbar?.targetGraphic?.enabled = false;

            return scrollRect;
        }

        public static void ClearPanel(this Transform panel, bool clearScroll = false, bool clearDescription = false, bool clearButton = false)
        {
            if (panel == null) return;
            foreach (Transform child in panel)
            {
                //if (child.name == "Scroll" || child.name == "Description" || child.name == "Button") continue;
                if (child.name == "Scroll" && !clearScroll) continue;
                if (child.name == "Description" && !clearDescription) continue;
                if (child.name == "Button" && !clearButton) continue;
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public static void ClearButtons(this Transform panel)
        {
            if (panel == null) return;
            Button[] buttons = panel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                UnityEngine.Object.Destroy(b.gameObject);
            }
        }

        public static GameObject CreateSquareProgress(Transform parent, float squareSize, int filled)
        {
            GameObject squaresObject = Create("SquareProgress", parent, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            squaresObject.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 4f,
                align: TextAnchor.MiddleLeft);

            squaresObject.GetComponent<LayoutElement>().preferredHeight = squareSize;

            for (int i = 0; i < 10; i++)
            {
                GameObject squareObject = Create($"ProgressSquare{i}", squaresObject.transform, typeof(Image), typeof(LayoutElement));
                LayoutElement squareLayout = squareObject.GetComponent<LayoutElement>();
                squareLayout.preferredHeight = squareSize;
                squareLayout.preferredWidth = squareSize;
                squareLayout.minHeight = squareSize;
                squareLayout.minWidth = squareSize;

                Image squareImage = squareObject.GetComponent<Image>();
                squareImage.color = i < filled ? Color.white : new Color(1f, 1f, 1f, 0.1f);
            }

            return squaresObject;
        }

        public static GameObject CreateLineProgress(Transform parent,
                                                    int unlocked,
                                                    int total,
                                                    TMP_FontAsset font,
                                                    float height = 24f,
                                                    int divisions = 16)
        {
            GameObject progressObject = CreatePanel("LineProgress", parent);
            LayoutElement progressLe = progressObject.AddComponent<LayoutElement>();
            progressLe.preferredHeight = height;

            GameObject divisionObject = Create("DivisionGrid", progressObject.transform, typeof(HorizontalLayoutGroup));
            Stretch(divisionObject.transform);
            divisionObject.GetComponent<HorizontalLayoutGroup>().Configure(expandH: true, expandW: true);
            BuildCells(divisionObject.transform, divisions);
            int precentage = total > 0 ? (int)Math.Round((double)unlocked / total * 100, MidpointRounding.AwayFromZero) : 0;

            GameObject fillObject = Create("Fill", progressObject.transform, typeof(Image));
            RectTransform fillRect = Stretch(fillObject.transform);
            fillRect.anchorMax = new Vector2(precentage / 100f, 1f);

            Image fillImage = fillObject.GetComponent<Image>();
            fillImage.color = Color.white;
            fillImage.raycastTarget = false;

            if (precentage > 0)
            {
                TextMeshProUGUI textTmp = CreateText(progressObject.transform, "Precentage", $"{precentage}%", font, 18f, alignment: TextAlignmentOptions.Midline);
                textTmp.color = Color.black;
                var textRect = Stretch(textTmp.transform);

                if (precentage < 50)
                {
                    textTmp.alignment = TextAlignmentOptions.MidlineRight;
                    textTmp.transform.SetParent(fillObject.transform, false);
                    textRect.offsetMax = new Vector2(-10f, 0f);
                }
            }

            return progressObject;
        }

        private static void BuildCells(Transform parent, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject cellObject = Create($"Cell{i}", parent);

                if (i < count - 1)
                {
                    GameObject lineObject = Create("Line", cellObject.transform, typeof(Image));

                    RectTransform lineRect = lineObject.GetComponent<RectTransform>();
                    lineRect.anchorMax = Vector2.one;
                    lineRect.anchorMin = Vector2.right;
                    lineRect.pivot = new Vector2(1f, 0.5f);

                    lineRect.sizeDelta = new Vector2(3f, 0f);
                    lineRect.offsetMax = new Vector2(lineRect.offsetMax.x, 0f);
                    lineRect.offsetMin = new Vector2(lineRect.offsetMin.x, 0f);

                    Image lineImage = lineRect.GetComponent<Image>();
                    lineImage.color = Color.white;
                    lineImage.raycastTarget = false;
                }
            }
        }

        public static TextMeshProUGUI CreateText(Transform parent,
                                                 string name,
                                                 string text,
                                                 TMP_FontAsset font,
                                                 float fontSize = 12f,
                                                 TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            var tmp = Create(name, parent, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            return tmp;
        }

        public static GameObject CreateSeparator(Transform parent, float preferredHeight = 2f)
        {
            GameObject separator = Create("Separator", parent, typeof(Image), typeof(LayoutElement));

            separator.transform.SetParent(parent, false);

            LayoutElement layout = separator.GetComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;
            layout.flexibleWidth = 1f;

            return separator;
        }

        public static GameObject CreateSpacer(Transform parent, float flexibleWidth = 1f)
        {
            GameObject spacer = Create("Spacer", parent, typeof(LayoutElement));
            spacer.GetComponent<LayoutElement>().flexibleWidth = flexibleWidth;
            return spacer;
        }
    }
}
