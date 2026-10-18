using CUAchievements.Registry;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CUAchievements.UI.Utility
{
    internal class AchievementItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private RegisteredAchievement achievement;
        private Action<RegisteredAchievement> onClick;
        private float hoverScale = 1.01f;
        private float lerpSpeed = 12f;

        private Vector3 targetScale = Vector3.one;
        private RectTransform selfRect;

        public void Initialize(RegisteredAchievement achievement, Action<RegisteredAchievement> onClick)
        {
            this.achievement = achievement;
            this.onClick = onClick;
            selfRect = GetComponent<RectTransform>();
            targetScale = selfRect.localScale;
        }
        private void Update() => selfRect.localScale = Vector3.Lerp(selfRect.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);
        public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(achievement);
        public void OnPointerEnter(PointerEventData eventData) => targetScale = Vector3.one * hoverScale;
        public void OnPointerExit(PointerEventData eventData) => targetScale = Vector3.one;
    }
}
