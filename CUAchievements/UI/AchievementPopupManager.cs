using CUAchievements.Definitions;
using CUAchievements.Registry;
using CUCoreLib.Helpers;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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

    internal class AchievementPopupManager : MonoBehaviour
    {
        public static AchievementPopupManager Instance { get; private set; }

        private static DisplayType displayType;
        //private static PopupStyle popupStyle;

        private const float PanelWidth = 460f;
        private const float PanelHeight = 120f;
        private const float ShowDuration = 5f;
        private const float Margin = 20f;

        private RectTransform popupRoot;
        private GameObject popupCanvasObject;
        private static AudioSource uiAudioSource;

        private Coroutine debugShowCoroutine;

        private readonly Queue<(AchievementDifficulty difficulty, Sprite icon, string title, string desc)> queue = new Queue<(AchievementDifficulty difficulty, Sprite icon, string title, string desc)>();
        private bool isShowing;

        private Vector2 hiddenPos;
        private Vector2 shownPos;

        private Image frameImage;
        private Image iconImage;
        private TextMeshProUGUI titleTmp;
        private TextMeshProUGUI descTmp;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            GameObject popupManager = new GameObject("AchievementPopupManager");
            DontDestroyOnLoad(popupManager);
            Instance = popupManager.AddComponent<AchievementPopupManager>();
            Instance.BuildUI();
        }

        public static void Show(RegisteredAchievement achievement)
        {
            AlertType alertType = (AlertType)PlayerPrefs.GetInt("Achievements_AlertType", 0);
            if (!NeedPopup(alertType, achievement)) return;

            BuildOnPlayerPrefs();

            AchievementDefinition achievementDef = achievement.Definition;
            Instance.queue.Enqueue((achievementDef.Difficulty, achievementDef.Icon, achievementDef.Title, achievementDef.Description));

            float playerPrefVolume = PlayerPrefs.GetFloat("Achievements_UnlockVolume", 0.5f);
            AudioClip unlockSound = GetUnlockSound();
            uiAudioSource.PlayOneShot(unlockSound, playerPrefVolume);

            if (!Instance.isShowing)
            {
                Instance.StartCoroutine(Instance.ProcessQueue());
            }
        }

        internal static void DebugShow()
        {
            AchievementDefinition debugAchievement = AchievementDefinition.Instant("test", "Thanks for using CUAchievements!", "This is CUAchievements! Hello!! Hi!!! hii!!! HEEELlOOO!!!", null);

            AlertType alertType = (AlertType)PlayerPrefs.GetInt("Achievements_AlertType", 0);
            if (!NeedPopup(alertType, null)) return;

            BuildOnPlayerPrefs();
            if (Instance.debugShowCoroutine != null)
                Instance.StopCoroutine(Instance.debugShowCoroutine);
            Instance.debugShowCoroutine = Instance.StartCoroutine(Instance.DebugShowCoroutine(debugAchievement));
        }

        private IEnumerator ProcessQueue()
        {
            isShowing = true;
            while (queue.Count > 0)
            {
                (AchievementDifficulty difficulty, Sprite icon, string title, string desc) = queue.Dequeue();

                yield return AnimatePopup(difficulty, icon, title, desc);
            }
            isShowing = false;
        }

        private void BuildAchievementObject()
        {
            if (popupRoot == null) return;
            TMP_FontAsset fontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Retro GamingPix");

            GameObject achievementObject = new GameObject("AchievementObject", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(CanvasGroup));
            achievementObject.transform.SetParent(popupRoot, false);

            RectTransform achievementRect = achievementObject.GetComponent<RectTransform>();
            achievementRect.anchorMin = Vector2.zero;
            achievementRect.anchorMax = Vector2.one;
            achievementRect.offsetMin = Vector2.zero;
            achievementRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup achievementHorizontalLayout = achievementObject.GetComponent<HorizontalLayoutGroup>();
            achievementHorizontalLayout.padding = new RectOffset(8, 8, 8, 8);
            achievementHorizontalLayout.spacing = 16f;
            achievementHorizontalLayout.childControlWidth = true;
            achievementHorizontalLayout.childControlHeight = false;
            achievementHorizontalLayout.childForceExpandWidth = false;
            achievementHorizontalLayout.childForceExpandHeight = false;
            achievementHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;

            GameObject bgObject = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(LayoutIgnorer));
            bgObject.transform.SetParent(achievementObject.transform, false);

            RectTransform bgRect = bgObject.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgObject.GetComponent<Image>();
            bgImage.color = Color.black;
            bgImage.raycastTarget = false;

            GameObject frameObject = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(LayoutIgnorer));
            frameObject.transform.SetParent(achievementObject.transform, false);

            RectTransform frameRectTransform = frameObject.GetComponent<RectTransform>();
            frameRectTransform.anchorMin = Vector2.zero;
            frameRectTransform.anchorMax = Vector2.one;
            frameRectTransform.offsetMin = Vector2.zero;
            frameRectTransform.offsetMax = Vector2.zero;

            Image frameImage = frameObject.GetComponent<Image>();
            frameImage.type = Image.Type.Sliced;
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;
            this.frameImage = frameImage;

            GameObject achievementIconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            achievementIconObject.transform.SetParent(achievementObject.transform, false);

            LayoutElement iconLayout = achievementIconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 96f;
            iconLayout.preferredHeight = 96f;
            iconLayout.minWidth = 96f;
            iconLayout.minHeight = 96f;

            Image iconImage = achievementIconObject.GetComponent<Image>();
            iconImage.preserveAspect = true;
            this.iconImage = iconImage;

            GameObject iconFrameObject = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            iconFrameObject.transform.SetParent(achievementIconObject.transform, false);

            RectTransform iconFrameRectTransform = iconFrameObject.GetComponent<RectTransform>();
            iconFrameRectTransform.anchorMin = Vector2.zero;
            iconFrameRectTransform.anchorMax = Vector2.one;
            iconFrameRectTransform.offsetMin = Vector2.zero;
            iconFrameRectTransform.offsetMax = Vector2.zero;

            Image iconFrameImage = iconFrameObject.GetComponent<Image>();
            iconFrameImage.sprite = AssetLoader.GetCachedSprite("achievement_frame_image");
            iconFrameImage.type = Image.Type.Simple;
            iconFrameImage.raycastTarget = false;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textObject.transform.SetParent(achievementObject.transform, false);

            LayoutElement textLayout = textObject.GetComponent<LayoutElement>();
            textLayout.flexibleWidth = 1f;

            VerticalLayoutGroup textVerticalLayout = textObject.GetComponent<VerticalLayoutGroup>();
            textVerticalLayout.childControlWidth = true;
            textVerticalLayout.childControlHeight = true;
            textVerticalLayout.childForceExpandWidth = true;
            textVerticalLayout.spacing = 4f;

            GameObject titleObj = CreateText("Title", textObject.transform, 20, Color.white, fontAsset);
            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            this.titleTmp = titleTmp;

            GameObject descObj = CreateText("Description", textObject.transform, 16, new Color(1f, 1f, 1f, 0.7f), fontAsset);
            TextMeshProUGUI descTmp = descObj.GetComponent<TextMeshProUGUI>();
            this.descTmp = descTmp;

            LayoutElement descLayout = descObj.GetComponent<LayoutElement>();
            descLayout.preferredHeight = 45f;
            descLayout.flexibleHeight = 0f;
        }

        private GameObject CreateText(string name, Transform parent, int fontSize, Color color, TMP_FontAsset font)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textObj.transform.SetParent(parent, false);

            var textTmp = textObj.GetComponent<TextMeshProUGUI>();
            textTmp.transform.SetParent(parent, false);
            textTmp.fontSize = fontSize;
            textTmp.color = color;
            textTmp.font = font;
            textTmp.enableWordWrapping = true;
            textTmp.overflowMode = TextOverflowModes.Ellipsis;
            textTmp.maxVisibleLines = 2;
            return textObj;
        }

        private IEnumerator DebugShowCoroutine(AchievementDefinition achievement)
        {
            yield return AnimatePopup(achievement.Difficulty, achievement.Icon, achievement.Title, achievement.Description);
        }

        private IEnumerator AnimatePopup(AchievementDifficulty difficulty, Sprite icon, string title, string desc)
        {
            frameImage.sprite = GetAchievementFrame(difficulty);
            iconImage.sprite = icon;
            titleTmp.text = title;
            descTmp.text = desc;
            yield return AnimationHelper.SlideTo(shownPos, popupRoot);
            yield return new WaitForSeconds(ShowDuration);
            yield return AnimationHelper.SlideTo(hiddenPos, popupRoot);
        }

        

        private static bool NeedPopup(AlertType type, RegisteredAchievement achievement)
        {
            switch (type)
            {
                case AlertType.PopUp:
                    return true;

                case AlertType.Alert:

                    if (!CUCoreUtils.TryGetCamera(out _))
                        break;

                    if (achievement != null)
                        AlertShow(achievement);

                    return false;

                case AlertType.Hidden:
                    return false;
            }

            return true;
        }
        private static void BuildOnPlayerPrefs()
        {
            DisplayType playerPrefDisplayType = (DisplayType)PlayerPrefs.GetInt("Achievements_AchievementDisplay");
            //PopupStyle playerPrefPopupStyle = (PopupStyle)PlayerPrefs.GetInt("Achievements_AchievementPopupStyle");

            bool needsRebuild = Instance != null && (displayType != playerPrefDisplayType);
            displayType = playerPrefDisplayType;
            //popupStyle = playerPrefPopupStyle;

            EnsureCreated();

            if (needsRebuild)
                Instance.RebuildUI();
        }

        private static void AlertShow(RegisteredAchievement achievement)
        {
            string alertMessage = string.Format("Достижение \"{0}\" разблокировано!", achievement.Definition.Title); // TODO: Localization
            CUCoreUtils.ShowAlert(alertMessage);
        }

        private void BuildUI()
        {
            GameObject popupCanvasObject = new GameObject("AchievementPopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(AudioSource));
            popupCanvasObject.transform.SetParent(transform, false);
            this.popupCanvasObject = popupCanvasObject;

            uiAudioSource = popupCanvasObject.GetComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f;
            uiAudioSource.pitch = 1f;

            Canvas popupCanvas = popupCanvasObject.GetComponent<Canvas>();
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupCanvas.sortingOrder = 5000;

            CanvasScaler popupCanvasScaler = popupCanvasObject.GetComponent<CanvasScaler>();
            popupCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            popupCanvasScaler.referenceResolution = new Vector2(1920, 1080);

            GameObject popupRootObject = new GameObject("PopupRoot", typeof(RectTransform));
            popupRootObject.transform.SetParent(popupCanvasObject.transform, false);

            popupRoot = popupRootObject.GetComponent<RectTransform>();
            popupRoot.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            ConfigureAnchorsForDisplayType();

            popupRoot.anchoredPosition = hiddenPos;

            BuildAchievementObject();
        }

        private void RebuildUI()
        {
            if (popupCanvasObject != null) Destroy(popupCanvasObject);
            BuildUI();
        }
        private static AudioClip GetUnlockSound()
        {
            int playerPrefUnlockSoundIndex = PlayerPrefs.GetInt("Achievements_AchievementUnlockSound", 0);
            AudioClip audio = AssetLoader.GetCachedAudioClip($"achievement_sound_unlock_{playerPrefUnlockSoundIndex}");
            if (audio == null)
                return AssetLoader.GetCachedAudioClip("achievement_sound_unlock_0");
            return audio;
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

        private Sprite GetAchievementFrame(AchievementDifficulty rank)
        {
            switch (rank)
            {
                case AchievementDifficulty.Normal:
                    return AssetLoader.GetCachedSprite("achievement_frame_normal");
                case AchievementDifficulty.Difficult:
                    return AssetLoader.GetCachedSprite("achievement_frame_difficult");
                case AchievementDifficulty.Hard:
                    return AssetLoader.GetCachedSprite("achievement_frame_hard");
            }
            return AssetLoader.GetCachedSprite("achievement_frame_normal");
        }
    }
}
