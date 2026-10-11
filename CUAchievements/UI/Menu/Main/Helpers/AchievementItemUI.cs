using CUAchievements.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CUAchievements.UI
{
    internal class AchievementItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private RegisteredAchievement achievement;
        private Action<RegisteredAchievement> onClick;

        public void Initialize(RegisteredAchievement achievement, Action<RegisteredAchievement> onClick)
        {
            this.achievement = achievement;
            this.onClick = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(achievement);
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
