using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    public class AchievementButtonHoverVisual : MonoBehaviour
    {
        private RectTransform clickRect;
        private Image overlay;
        private TextMeshProUGUI label;
        private Action onClicked;
        private const float FadeSpeed = 16f;

        public void Initialize(RectTransform clickRect, Image overlay, TextMeshProUGUI label, Action onClicked)
        {
            this.clickRect = clickRect;
            this.overlay = overlay;
            this.label = label; 
            this.onClicked = onClicked;
        }
        
        private void Update()
        {
            bool hovering = clickRect != null && RectTransformUtility.RectangleContainsScreenPoint(clickRect, Input.mousePosition, null);
            float target = hovering ? 1f : 0f;

            Color oc = overlay.color;
            oc.a = Mathf.MoveTowards(oc.a, target, FadeSpeed * Time.unscaledDeltaTime);
            overlay.color = oc;

            Color lc = label.color;
            lc.a = Mathf.MoveTowards(lc.a, target, FadeSpeed * Time.unscaledDeltaTime);
            label.color = lc;

            if (hovering && Input.GetMouseButtonDown(0))
            {
                onClicked?.Invoke();
            }
        }
    }
}
