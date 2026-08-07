using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUCoreLib.Helpers;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal enum AlertType
    {
        PopUp = 0,
        Alert = 1,
        Hidden = 2
    }

    internal enum DisplayType
    {
        Bottom = 0,
        Left = 1,
        Up = 2
    }

    internal enum PopupFrameType
    {
        V1 = 0,
        V1Alt = 1,
        V2 = 2,
        V2Alt = 3,
        V3 = 4,
        V3Alt = 5
    }
    public class AchievementPopupManager : MonoBehaviour
    {
        public static AchievementPopupManager Instance { get; private set; }


        private static AudioClip achievementPopUpAudio;
        private RectTransform popupRoot;
        private Image iconImage;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descText;
        private GameObject popupCanvasGameObject;

        private readonly Queue<(Sprite icon, string title, string desc)> queue = new Queue<(Sprite icon, string title, string desc)>();
        private bool isShowing;

        private const float PanelWidth = 420f;
        private const float PanelHeight = 120f;
        private const float ShowDuration = 5f;
        private const float Margin = 20f;

        private Vector2 hiddenPos;
        private Vector2 shownPos;

        private Coroutine debugShowCoroutine;

        private static DisplayType displayType;
        private static PopupFrameType popupFrameType;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            GameObject gameObject = new GameObject("AchievementPopupManager");
            DontDestroyOnLoad(gameObject);
            Instance = gameObject.AddComponent<AchievementPopupManager>();
            Instance.BuildUI();
        }

        public static void Show(AchievementDefinition achievementDef)
        {
            AlertType alertType = (AlertType)PlayerPrefs.GetInt("Achievements_AlertType", 0);

            if (alertType == AlertType.Hidden) return;
            if (AlertShow(achievementDef)) return;

            DisplayType playerPrefDisplayType = (DisplayType)PlayerPrefs.GetInt("Achievements_AchievementDisplay", 0);
            PopupFrameType playerPrefFrameType = (PopupFrameType)PlayerPrefs.GetInt("Achievements_AchievementFrame", 0);

            bool needsRebuild = Instance != null && (displayType != playerPrefDisplayType || popupFrameType != playerPrefFrameType);
            displayType = playerPrefDisplayType;
            popupFrameType = playerPrefFrameType;
            EnsureCreated();

            if (needsRebuild)
            {
                Instance.RebuildUI();
            }

            CUCoreUtils.PlaySoundAt(achievementPopUpAudio, null);

            Instance.queue.Enqueue((achievementDef.Sprite, achievementDef.Title, achievementDef.Description));
            if (!Instance.isShowing)
            {
                Instance.StartCoroutine(Instance.ProcessQueue());
            }
        }

        internal static void DebugShow()
        {
            var achievementDef = AchievementDefinition.Instant("test", "CUAchievements", "This is CUAchievements!", null);

            AlertType alertType = (AlertType)PlayerPrefs.GetInt("Achievements_AlertType", 0);

            if (alertType == AlertType.Hidden) return;
            if (AlertShow(achievementDef)) return;

            DisplayType playerPrefDisplayType = (DisplayType)PlayerPrefs.GetInt("Achievements_AchievementDisplay", 0);
            PopupFrameType playerPrefFrameType = (PopupFrameType)PlayerPrefs.GetInt("Achievements_AchievementFrame", 0);

            bool needsRebuild = Instance != null && (displayType != playerPrefDisplayType || popupFrameType != playerPrefFrameType);
            displayType = playerPrefDisplayType;
            popupFrameType = playerPrefFrameType;
            EnsureCreated();

            if (needsRebuild)
            {
                Instance.RebuildUI();
            }

            CUCoreUtils.PlaySoundAt(achievementPopUpAudio, null);

            if (Instance.debugShowCoroutine != null)
            {
                Instance.StopCoroutine(Instance.debugShowCoroutine);
            }
            Instance.debugShowCoroutine = Instance.StartCoroutine(Instance.DebugShowCoroutine(achievementDef));
        }


        private static bool AlertShow(AchievementDefinition achievementDef)
        {
            AlertType alertType = (AlertType)PlayerPrefs.GetInt("Achievements_AlertType", 0);

            if (achievementPopUpAudio == null)
            {
                achievementPopUpAudio = AssetLoader.GetCachedAudioClip("achievement_sound_unlocked");
            }

            if (alertType == AlertType.Alert && !CUCoreUtils.TryGetCamera(out _))
            {
                return false;
            }

            if (alertType == AlertType.PopUp)
            {
                return false;
            }
            else
            {
                ShowAlertNotification(achievementDef);
                return true;
            }
        }

        private void BuildUI()
        {
            GameObject canvasGameObject = new GameObject("AchievementPopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGameObject.transform.SetParent(transform, false);
            popupCanvasGameObject = canvasGameObject;

            Canvas canvas = canvasGameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler canvasScaler = canvasGameObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            GameObject rootGameObject = new GameObject("PopupRoot", typeof(RectTransform));
            rootGameObject.transform.SetParent(canvasGameObject.transform, false);

            popupRoot = rootGameObject.GetComponent<RectTransform>();
            popupRoot.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            ConfigureAnchorsForDisplayType();

            popupRoot.anchoredPosition = hiddenPos;

            GameObject frameGameObject = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frameGameObject.transform.SetParent(popupRoot, false);
            RectTransform frameRect = frameGameObject.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            Image frameImage = frameGameObject.GetComponent<Image>();
            frameImage.sprite = AssetLoader.GetCachedSprite("achievement_popup_frame_v1");
            frameImage.type = Image.Type.Simple;
            frameImage.preserveAspect = false;
            frameImage.raycastTarget = false;

            GameObject iconGameObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGameObject.transform.SetParent(popupRoot, false);
            RectTransform iconRect = iconGameObject.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(90f, 90f);
            iconRect.anchoredPosition = new Vector2(60f, 0f);
            iconImage = iconGameObject.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            titleText = CreateText("Title", popupRoot, new Vector2(0.35f, 0.55f), new Vector2(1f, 0.95f), 18, FontStyles.Bold);
            descText = CreateText("Description", popupRoot, new Vector2(0.35f, 0.1f), new Vector2(1f, 0.55f), 16, FontStyles.Normal);
        }

        private void RebuildUI()
        {
            if (popupCanvasGameObject != null)
            {
                Destroy(popupCanvasGameObject);
            }
            BuildUI();
        }

        private void ConfigureAnchorsForDisplayType()
        {
            switch (displayType)
            {
                case DisplayType.Bottom:
                    popupRoot.anchorMin = popupRoot.anchorMax = new Vector2(0.5f, 0f);
                    popupRoot.pivot = new Vector2(0.5f, 0f);
                    hiddenPos = new Vector2(0f, -(PanelHeight + Margin));
                    shownPos = new Vector2(0f, Margin);
                    break;

                case DisplayType.Left:
                    popupRoot.anchorMin = popupRoot.anchorMax = new Vector2(0f, 0f);
                    popupRoot.pivot = new Vector2(0f, 0f);
                    hiddenPos = new Vector2(-(PanelWidth + Margin), Margin);
                    shownPos = new Vector2(Margin, Margin);
                    break;

                case DisplayType.Up:
                    popupRoot.anchorMin = popupRoot.anchorMax = new Vector2(0.5f, 1f);
                    popupRoot.pivot = new Vector2(0.5f, 1f);
                    hiddenPos = new Vector2(0f, PanelHeight + Margin);
                    shownPos = new Vector2(0f, -Margin);
                    break;
            }
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyles style)
        {
            TMP_FontAsset fontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Retro GamingPix");
            GameObject textGameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textGameObject.transform.SetParent(parent, false);
            RectTransform textRect = textGameObject.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmpText = textGameObject.GetComponent<TextMeshProUGUI>();
            tmpText.fontSize = fontSize;
            tmpText.fontStyle = style;
            tmpText.font = fontAsset;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.MidlineLeft;
            tmpText.enableWordWrapping = true;
            tmpText.raycastTarget = false;
            return tmpText;
        }

        private IEnumerator ProcessQueue()
        {
            isShowing = true;
            while (queue.Count > 0)
            {
                var (icon, title, desc) = queue.Dequeue();
                iconImage.sprite = icon;
                titleText.text = title;
                descText.text = desc;

                yield return SlideTo(shownPos);
                yield return new WaitForSeconds(ShowDuration);
                yield return SlideTo(hiddenPos);
            }
            isShowing = false;
        }

        private IEnumerator DebugShowCoroutine(AchievementDefinition achievement)
        {
            iconImage.sprite = achievement.Sprite;
            titleText.text = achievement.Title;
            descText.text = achievement.Description;

            yield return SlideTo(shownPos);
            yield return new WaitForSeconds(ShowDuration);
            yield return SlideTo(hiddenPos);
        }

        private IEnumerator SlideTo(Vector2 target)
        {
            Vector2 start = popupRoot.anchoredPosition;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                popupRoot.anchoredPosition = Vector2.Lerp(start, target, eased);
                yield return null;
            }

            popupRoot.anchoredPosition = target;
        }

        private static void ShowAlertNotification(AchievementDefinition achievement)
        {
            string alertMessage = string.Format("Достижение \"{0}\" разблокировано!", achievement.Title); // TODO: Add localization support for the alert message
            CUCoreUtils.ShowAlert(alertMessage);
            CUAchievementsPlugin.Logger.LogInfo($"Alert notification: Achievement '{achievement.Id}' unlocked!");
        }
    }
}
