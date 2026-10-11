using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CUAchievements.UI
{
    internal class AchievementPluginUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject pluginContainer;
        private Action<GameObject> onClick;

        public void Initialize(GameObject pluginContainer, Action<GameObject> onClick)
        {
            this.pluginContainer = pluginContainer;
            this.onClick = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(pluginContainer);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // hover
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // unhover
        }
    }
}
