using CUAchievements.Registry;
using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.SidePanels
{
    internal sealed class DetailsPanel : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public GameObject Root;
        public RectTransform Content;
        //public event Action Opened;
        //public event Action Closed;

        private const float PanelWidth = 380f;
        private const float PanelHeight = 800f;
        private const float ScrollWidth = PanelWidth - 10f;
        private const float ScrollHeight = PanelHeight - 100f;

        private Vector2 HiddenPos = new(-600f, 0f);
        private Vector2 ShownPos = new(20f, 0f);

        private Coroutine slideAnim;

        private Image icon;
        private Image frame;

        private TextMeshProUGUI aTitle;
        private TextMeshProUGUI aDescription;
        private TextMeshProUGUI aModSource;
        private TextMeshProUGUI aScope;
        private TextMeshProUGUI aUnlocked;
        private TextMeshProUGUI aUnlockedAtDate;
        private TextMeshProUGUI aUnlockedAtTime;

        private GameObject dataObject;
        private GameObject squaresObject;

        //private string currentShowedId = string.Empty;

        public static DetailsPanel Create(Transform parent, Transform panel)
        {
            GameObject detailsPanel = Instantiate(panel, parent).gameObject;
            detailsPanel.name = "Details";

            DetailsPanel panelComponent = detailsPanel.AddComponent<DetailsPanel>();

            panelComponent.Initialize();
            return panelComponent;
        }

        private void Initialize()
        {
            Root = transform.gameObject;

            ScrollRect scroll = Root.GetComponentInChildren<ScrollRect>(true);
            scroll.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            scroll.HideScrollbar();

            Content = scroll?.content;
            if (Content != null)
            {
                Root.transform.ClearPanel();
                Root.transform.ClearButtons();
                VerticalLayoutGroup vertical = Content.SetupScrollContent(spacing: 16f);
                vertical.name = "Details";
            }

            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(ScrollWidth, ScrollHeight);

            //var description = transform.GetComponentInChildren<TextMeshProUGUI>();
            //description.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollWidth, 50f);
            //description.text = LocaleRegistry.Get("achievements_details_panel_title", "Details");

            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            rect.anchoredPosition = HiddenPos;

            CreateHeader(parent: Content);
            UIUtility.CreateSeparator(parent: Content);

            aDescription = UIUtility.CreateText(Content, "Description", "Description", GameFonts.Retro, 16f);
            UIUtility.CreateSeparator(parent: Content);

            CreateScope(parent: Content);
            UIUtility.CreateSeparator(parent: Content);

            CreateBottom(parent: Content);
        }

        private void CreateHeader(Transform parent)
        {
            GameObject header = UIUtility.Create("Header", parent, typeof(VerticalLayoutGroup));
            header.GetComponent<VerticalLayoutGroup>().Configure(spacing: 8f, align: TextAnchor.MiddleCenter);

            GameObject icon = UIUtility.Create("Icon", header.transform, typeof(Image), typeof(LayoutElement));

            LayoutElement iconLayout = icon.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = iconLayout.minWidth = 152f;
            iconLayout.preferredHeight = iconLayout.minHeight = 152f;

            this.icon = icon.GetComponent<Image>();

            GameObject frame = UIUtility.Create("Frame", icon.transform, typeof(Image));
            frame.transform.Stretch();

            Image frameImage = frame.GetComponent<Image>();
            frameImage.type = Image.Type.Sliced;

            this.frame = frameImage;

            aTitle = UIUtility.CreateText(header.transform, "Title", "Title", GameFonts.Retro, 20f, TextAlignmentOptions.Center);
            aTitle.gameObject.AddComponent<LayoutElement>().minHeight = 32f;

            aModSource = UIUtility.CreateText(header.transform, "Source", "Source", GameFonts.Retro, 16f, TextAlignmentOptions.Center);
            aModSource.alpha = 0.75f;
        }

        private void CreateScope(Transform parent)
        {
            dataObject = UIUtility.Create("ProgressData", parent, typeof(VerticalLayoutGroup));
            dataObject.GetComponent<VerticalLayoutGroup>().Configure(spacing: 8f, align: TextAnchor.MiddleCenter);
            aScope = UIUtility.CreateText(dataObject.transform, "Scope", "Scope", GameFonts.Retro, 16f, TextAlignmentOptions.Center);
        }

        private void CreateProgressRow(AchievementCardModel card)
        {
            if (dataObject == null) return;
            if (squaresObject != null)
            {
                Destroy(squaresObject);
            }

            squaresObject = UIUtility.Create("SquareProgress", dataObject.transform, typeof(VerticalLayoutGroup));
            squaresObject.GetComponent<VerticalLayoutGroup>().Configure(spacing: 4f, align: TextAnchor.MiddleCenter);

            string lastProgress = "";
            if (card.Scope == AchievementScope.Run && card.Source.ProgressData.LastRunProgress > 0 && card.Source.ProgressData.Progress != card.Source.Definition.MaxProgress)
            {
                lastProgress = LocaleRegistry.Get("achievement_last_progress", "LAST RUN PROGRESS:") + "\n";
            }

            UIUtility.CreateText(squaresObject.transform, "ProgressText", $"{lastProgress}{card.Counter.Text}", GameFonts.Retro, 18f, TextAlignmentOptions.Center);
            UIUtility.CreateSquareProgress(squaresObject.transform, 24f, card.Counter.FilledSquares);
        }

        private void CreateBottom(Transform parent)
        {
            GameObject bottom = UIUtility.Create("Bottom", parent, typeof(VerticalLayoutGroup));
            aUnlocked = UIUtility.CreateText(bottom.transform, "Unlocked", "Unlocked", GameFonts.Retro, 18f);
            (aUnlockedAtDate, aUnlockedAtTime) = UIUtility.CreateStatisticRow(bottom.transform, "UnlockedAt", "-", "-");
        }

        private void Refresh(AchievementCardModel card)
        {
            if (card == null) return;

            icon.sprite = card.Icon;
            frame.sprite = card.Frame;

            aTitle.text = card.Title;
            aDescription.text = card.Description;
            aModSource.text = AchievementRegistry.TryGetPluginName(card.Source.PluginGuid, out string pluginName) ? pluginName : card.Source.PluginGuid;
            aScope.text = card.Scope == AchievementScope.Run ?
                LocaleRegistry.Get("achievements_scope_run", "MUST BE COMPLETED DURING A SINGLE RUN — PROGRESS RESETS ON DEATH") :
                card.Counter != null ?
                LocaleRegistry.Get("achievements_scope_lifetime", "PROGRESS IS SAVED BETWEEN RUNS") :
                "------";

            if (card.Counter != null)
            {
                CreateProgressRow(card);
            }
            else
            {
                Destroy(squaresObject);
            }

            if (card.Source.IsUnlocked)
            {
                string unlockedLocale = LocaleRegistry.Get("achievements_earned", "EARNED");
                aUnlocked.text = unlockedLocale;
                aUnlocked.color = Color.green;

                aUnlockedAtDate.text = card.Source.UnlockedAt?.ToLocalTime().ToShortDateString();
                aUnlockedAtTime.text = card.Source.UnlockedAt?.ToLocalTime().ToShortTimeString();
            }
            else
            {
                string lockedLocale = LocaleRegistry.Get("achievements_locked", "LOCKED");
                aUnlocked.text = lockedLocale;
                aUnlocked.color = Color.red;

                aUnlockedAtDate.text = "";
                aUnlockedAtTime.text = "";
            }
        }

        private void SetVisible(bool show, RegisteredAchievement achievement = null)
        {
            IsOpen = show;
            if (show)
                OnOpen(achievement);
            else
                OnClose();
        }
        public void Open(RegisteredAchievement achievement) => SetVisible(true, achievement);
        public void Close() => SetVisible(false);
        private void OnOpen(RegisteredAchievement achievement)
        {
            //if (currentShowedId == achievement.FullId)
            //{
            //    Close();
            //    currentShowedId = string.Empty;
            //    return;
            //}
            //currentShowedId = achievement.FullId;
            Refresh(AchievementCardModel.From(achievement));
            PlayAnim(ShownPos);
        }

        private void OnClose() => PlayAnim(HiddenPos);

        private void PlayAnim(Vector2 target)
        {
            if (slideAnim != null) StopCoroutine(slideAnim);
            slideAnim = StartCoroutine(Animate(target, (RectTransform)Root.transform));
        }
        private IEnumerator Animate(Vector2 target, RectTransform rect)
        {
            yield return AnimationUtility.SlideTo(target, rect);
            slideAnim = null;
        }
    }
}
