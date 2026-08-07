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
        private const float FadeSpeed = 0.5f;

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

            Color overlayColor = overlay.color;
            overlayColor.a = Mathf.MoveTowards(overlayColor.a, target, FadeSpeed * Time.unscaledDeltaTime);
            overlay.color = overlayColor;

            Color labelColor = label.color;
            labelColor.a = Mathf.MoveTowards(labelColor.a, target, FadeSpeed * Time.unscaledDeltaTime);
            label.color = labelColor;

            if (hovering && Input.GetMouseButtonDown(0))
            {
                onClicked?.Invoke();
            }
        }
    }
}
