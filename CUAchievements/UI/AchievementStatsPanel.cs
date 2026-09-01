using System.Collections;
using TMPro;
using UnityEngine;

namespace CUAchievements.UI
{
    internal sealed class AchievementStatsPanel : MonoBehaviour
    {
        private RectTransform statsPanelRect;
        private TextMeshProUGUI totalUnlockedText;
        private bool isVisible = false;
        private Vector2 hiddenPos = new Vector2(600f, 0f);
        private Vector2 shownPos = new Vector2(-20f, 0f);

        public static AchievementStatsPanel Create(Transform parent, TMP_FontAsset font)   
        {
            Transform panelTransform = parent.Find("Panel");
            if (panelTransform == null) return null;

            GameObject statsPanel = Instantiate(panelTransform, parent).gameObject;
            statsPanel.name = "StatsPanel";

            AchievementStatsPanel panelComponent = statsPanel.AddComponent<AchievementStatsPanel>();

            panelComponent.Initialize(font);
            return panelComponent;
        }

        private void Initialize(TMP_FontAsset font)
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Scroll")
                {
                    var content = child.Find("Content");
                    if (content != null)
                    {
                        foreach (Transform child2 in content.transform)
                            Destroy(child2.gameObject);
                    }
                    continue;
                }
                if (child.name != "Description")
                    Destroy(child.gameObject);
            }

            GameObject statsScroll = transform.Find("Scroll").gameObject;
            RectTransform statsScrollRect = statsScroll.GetComponent<RectTransform>();
            statsScrollRect.sizeDelta = new Vector2(370f, 700f);

            TextMeshProUGUI description = GetComponentInChildren<TextMeshProUGUI>();
            if (description != null) description.text = "Statistic"; // TODO: Localization

            RectTransform statsRect = GetComponent<RectTransform>();
            statsRect.anchorMax = new Vector2(1f, 0.5f);
            statsRect.anchorMin = new Vector2(1f, 0.5f);
            statsRect.pivot = new Vector2(1f, 0.5f);
            statsRect.sizeDelta = new Vector2(380f, 800f);
            statsRect.anchoredPosition = hiddenPos;

            statsPanelRect = statsRect;
        }

        public void Refresh()
        {
            
        }

        public void SetVisible()
        {
            isVisible = !isVisible;

            StopAllCoroutines();
            StartCoroutine(Animate(isVisible ? shownPos : hiddenPos));
        }

        private IEnumerator Animate(Vector2 target)
        {
            yield return AnimationHelper.SlideTo(target, statsPanelRect);
        }
    }
}
