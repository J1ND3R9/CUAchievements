using CUAchievements.Options;
using CUAchievements.Registry;
using CUAchievements.UI.Menu;
using CUAchievements.UI.Utility;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    // TODO: Рефактор
    internal sealed class AchievementPopupManager : MonoBehaviour
    {
        public static AchievementPopupManager Instance { get; private set; }
        private static float ShowDuration = 5f;
        private static AchievementDisplayType DisplayType = AchievementDisplayType.BottomToUp;
        private static AudioSource audioSource;
        private static bool isPrecentageShow = false;

        private const float PanelWidth = 460f;
        private const float PanelHeight = 120f;
        private const float Margin = 20f;

        private GameObject PopupRoot;

        private Vector2 HiddenPos = Vector2.zero;
        private Vector2 ShownPos = Vector2.zero;

        private Image iconImage;
        private Image frameImage;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descText;

        private GameObject textSection;
        private GameObject progressRow;

        private GameObject canvasObject;

        private bool isShowing;
        private readonly Queue<AchievementCardModel> AchievementsQueue = new();

        private Coroutine debugShowCoroutine;
        public static void Initialize()
        {
            if (Instance != null) return;
            GameObject popupInstance = new("AchievementPopupManager");
            DontDestroyOnLoad(popupInstance);
            Instance = popupInstance.AddComponent<AchievementPopupManager>();
            Instance.Build();
        }

        public static void Show(RegisteredAchievement achievement, bool precentageShow = false)
        {
            AchievementCardModel model = AchievementCardModel.From(achievement);
            AchievementAlertType alertType = AchievementConfig.AlertType.Value;

            if (alertType != AchievementAlertType.Popup)
            {
                if (alertType == AchievementAlertType.Hidden) return;
                if (alertType == AchievementAlertType.Default && CUCoreUtils.TryGetCamera(out _))
                {
                    ShowAlert(model);
                    return;
                }
            }

            BuildOnConfig(precentageShow);

            Instance.AchievementsQueue.Enqueue(model);

            if (!precentageShow)
            {
                float volume = AchievementConfig.UnlockVolume.Value;
                AudioClip unlockSound = AssetLoader.GetCachedAudioClip("achievement_sound_unlock_0");
                audioSource.PlayOneShot(unlockSound, volume);
            }

            if (!Instance.isShowing)
            {
                Instance.StartCoroutine(Instance.ProcessQueue());
            }
        }

        public static void ShowAlert(AchievementCardModel model)
        {
            string alertLocale = LocaleRegistry.Get("achievement_alert_unlocked", "Achievement \"{0}\" unlocked!");
            string alertMessage = string.Format(alertLocale, model.Title);
            CUCoreUtils.ShowAlert(alertMessage);
        }

        private void Build()
        {
            canvasObject = new GameObject("PopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(AudioSource));
            canvasObject.transform.SetParent(transform, false);

            audioSource = canvasObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.pitch = 1f;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

            BuildPopupContainer(canvasObject.transform);
            BuildAchievement();
        }

        private void BuildPopupContainer(Transform canvas)
        {
            PopupRoot = UIUtility.Create("Popup", canvas);

            RectTransform popupRect = PopupRoot.GetComponent<RectTransform>();
            popupRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            DisplayTypeAnchors(popupRect);

            popupRect.anchoredPosition = HiddenPos;
        }

        private void DisplayTypeAnchors(RectTransform rect)
        {
            DisplayType = AchievementConfig.DisplayType.Value;

            Vector2 minMax = new(0.5f, 0f);
            Vector2 hiddenPos = new(0f, -(-PanelHeight + Margin));
            Vector2 shownPos = new(0f, Margin);

            switch (DisplayType)
            {
                case AchievementDisplayType.BottomToUp:
                    minMax = new(0.5f, 0f);
                    hiddenPos = new(0f, -(PanelHeight + Margin));
                    shownPos = new(0f, Margin);
                    break;
                case AchievementDisplayType.LeftToRight:
                    minMax = new(0f, 0f);
                    hiddenPos = new(-(PanelWidth + Margin), Margin);
                    shownPos = new(Margin, Margin);
                    break;
                case AchievementDisplayType.UpToBottom:
                    minMax = new(0.5f, 1f);
                    hiddenPos = new(0f, PanelHeight + Margin);
                    shownPos = new(0f, -Margin);
                    break;
                case AchievementDisplayType.SteamLike:
                    minMax = new(1f, 0f);
                    hiddenPos = new(0f, -(PanelHeight + Margin));
                    shownPos = new(0f, Margin);
                    break;
            }

            ConfigurePopupPosition(rect,
                        minMax: minMax,
                        hiddenPos: hiddenPos,
                        shownPos: shownPos
                        );
        }

        private void ConfigurePopupPosition(RectTransform rect, Vector2 minMax, Vector2 hiddenPos, Vector2 shownPos)
        {
            rect.anchorMin = rect.anchorMax = minMax;
            rect.pivot = minMax;
            HiddenPos = hiddenPos;
            ShownPos = shownPos;
        }

        private void BuildAchievement()
        {
            GameObject achObject = UIUtility.Create("Achievement", PopupRoot.transform, typeof(HorizontalLayoutGroup), typeof(CanvasGroup));
            achObject.GetComponent<RectTransform>().Stretch();
            achObject.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 16f, controlH: false, align: TextAnchor.MiddleLeft, padding: new RectOffset(8, 8, 8, 8));

            GameObject bgObject = UIUtility.Create("Background", achObject.transform, typeof(Image), typeof(LayoutIgnorer));
            bgObject.GetComponent<RectTransform>().Stretch();

            Image bgImage = bgObject.GetComponent<Image>();
            bgImage.color = Color.black;
            bgImage.raycastTarget = false;

            GameObject frameObject = UIUtility.Create("Frame", achObject.transform, typeof(Image), typeof(LayoutIgnorer));
            frameObject.GetComponent<RectTransform>().Stretch();

            frameImage = frameObject.GetComponent<Image>();
            frameImage.type = Image.Type.Sliced;
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;

            GameObject iconObject = UIUtility.Create("Icon", achObject.transform, typeof(Image), typeof(LayoutElement));

            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 96f;
            iconLayout.preferredHeight = 96f;
            iconLayout.minWidth = 96f;
            iconLayout.minHeight = 96f;

            iconImage = iconObject.GetComponent<Image>();
            iconImage.preserveAspect = true;

            GameObject iconFrameObject = UIUtility.Create("IconFrame", iconObject.transform, typeof(Image));
            iconFrameObject.transform.SetParent(iconObject.transform, false);

            iconFrameObject.GetComponent<RectTransform>().Stretch();

            Image iconFrameImage = iconFrameObject.GetComponent<Image>();
            iconFrameImage.sprite = AssetLoader.GetCachedSprite("achievement_frame_image");
            iconFrameImage.type = Image.Type.Simple;
            iconFrameImage.raycastTarget = false;

            textSection = UIUtility.Create("Text", achObject.transform, typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textSection.GetComponent<LayoutElement>().flexibleWidth = 1f;
            textSection.GetComponent<VerticalLayoutGroup>().Configure(spacing: 4f, expandW: true);

            titleText = UIUtility.CreateText(textSection.transform, "Title", "Title", GameFonts.Retro, 20f);
            titleText.enableWordWrapping = true;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.maxVisibleLines = 2;

            descText = UIUtility.CreateText(textSection.transform, "Description", "Desc", GameFonts.Retro, 16f);
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            descText.maxVisibleLines = 2;
            descText.color = new Color(1f, 1f, 1f, 0.7f);

            BuildProgressRow("Progress", 0);

            LayoutElement descLayout = descText.gameObject.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 45f;
        }

        private static void BuildProgressRow(string text, int filledSquares)
        {
            if (Instance == null) return;
            if (Instance.progressRow != null) Destroy(Instance.progressRow);
            if (isPrecentageShow)
            {
                Instance.progressRow = UIUtility.Create("PopupProgress", Instance.textSection.transform, typeof(HorizontalLayoutGroup));
                Instance.progressRow.GetComponent<HorizontalLayoutGroup>().Configure(spacing: 10f, align: TextAnchor.MiddleLeft);
                UIUtility.CreateText(Instance.progressRow.transform, "ProgressText", text, GameFonts.Retro, 16f);
                UIUtility.CreateSquareProgress(Instance.progressRow.transform, 14f, filledSquares);
            }
        }

        private static void BuildOnConfig(bool precentageShow)
        {
            AchievementDisplayType displayType = AchievementConfig.DisplayType.Value;
            bool needsRebuild = Instance != null && displayType != DisplayType && precentageShow != isPrecentageShow;
            DisplayType = displayType;
            ShowDuration = AchievementConfig.PopUpTime.Value;
            isPrecentageShow = precentageShow;

            Initialize();

            if (needsRebuild)
            {
                Instance.Rebuild();
            }
        }

        private void Rebuild()
        {
            if (canvasObject != null) Destroy(canvasObject);
            Build();
        }

        private IEnumerator ProcessQueue()
        {
            isShowing = true;
            while (AchievementsQueue.Count > 0)
            {
                AchievementCardModel model = AchievementsQueue.Dequeue();
                yield return AnimatePopup(model);
            }
            isShowing = false;
        }

        private IEnumerator AnimatePopup(AchievementCardModel model)
        {
            RectTransform popupRect = PopupRoot.GetComponent<RectTransform>();
            frameImage.sprite = model.Frame;
            iconImage.sprite = model.Icon;
            titleText.text = model.Title;
            descText.text = model.ShortDescription;

            if (model.Counter != null)
                BuildProgressRow(model.Counter.Text, model.Counter.FilledSquares);
            else if (progressRow != null)
                Destroy(progressRow);

            yield return AnimationUtility.SlideTo(ShownPos, popupRect);
            yield return new WaitForSecondsRealtime(ShowDuration);
            yield return AnimationUtility.SlideTo(HiddenPos, popupRect);
        }

        internal static void DebugShow(bool precentageShow = false)
        {
            BuildOnConfig(precentageShow);
            if (Instance == null) return;
            if (Instance.debugShowCoroutine != null)
                Instance.StopCoroutine(Instance.debugShowCoroutine);
            Instance.debugShowCoroutine = Instance.StartCoroutine(Instance.DebugShowCoroutine(AchievementCardModel.Debug()));
        }

        internal IEnumerator DebugShowCoroutine(AchievementCardModel model)
        {
            yield return AnimatePopup(model);
        }
    }
}
