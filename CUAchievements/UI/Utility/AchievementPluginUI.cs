using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CUAchievements.UI.Utility
{
    internal class AchievementPluginUI : MonoBehaviour, IPointerClickHandler/*, IPointerEnterHandler, IPointerExitHandler*/
    {
        private GameObject pluginContainer;
        private Action<GameObject> onClick;

        //private float hoverScale = 1.01f;
        //private float lerpSpeed = 12f;

        //private Vector3 targetScale = Vector3.one;
        //private RectTransform selfRect;

        public void Initialize(GameObject pluginContainer, Action<GameObject> onClick)
        {
            this.pluginContainer = pluginContainer;
            this.onClick = onClick;
            //selfRect = GetComponent<RectTransform>();
            //targetScale = selfRect.localScale;
        }

        //private void Update() => selfRect.localScale = Vector3.Lerp(selfRect.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);
        public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(pluginContainer);
        //public void OnPointerEnter(PointerEventData eventData) => targetScale = Vector3.one * hoverScale;
        //public void OnPointerExit(PointerEventData eventData) => targetScale = Vector3.one;
    }
}
