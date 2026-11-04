using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CUAchievements.UI.Menu
{
    internal sealed class TextLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        private TMP_Text text;
        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int index = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
            if (index < 0) return;

            string url = text.textInfo.linkInfo[index].GetLinkID();
            if (!url.StartsWith("https://")) return;

            PlayerCamera.PlayUISound("miniclick", 1f);
            Application.OpenURL(url);
        }
    }
}
