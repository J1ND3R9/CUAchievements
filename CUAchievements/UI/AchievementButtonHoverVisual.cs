using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal class AchievementButtonHoverVisual : MonoBehaviour
    {
        internal RectTransform clickRect;
        internal Image overlay;
        internal TextMeshProUGUI label;
        internal Action onClicked;
        internal const float FadeSpeed = 0.5f;
        internal static readonly List<RaycastResult> raycastResultsBuffer = new List<RaycastResult>();

        internal void Initialize(RectTransform clickRect, Image overlay, TextMeshProUGUI label, Action onClicked)
        {
            this.clickRect = clickRect;
            this.overlay = overlay;
            this.label = label; 
            this.onClicked = onClicked;
        }

        internal void Update()
        {
            bool hovering = clickRect != null && RectTransformUtility.RectangleContainsScreenPoint(clickRect, Input.mousePosition, null) && IsTopmostAtCursor(clickRect);
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
        internal bool IsTopmostAtCursor(RectTransform myRect)
        {
            if (EventSystem.current == null) return true;
            if (GameObject.Find("Panel")) return false;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            raycastResultsBuffer.Clear();
            EventSystem.current.RaycastAll(pointerData, raycastResultsBuffer);

            foreach (var result in raycastResultsBuffer)
            {
                Transform hit = result.gameObject.transform;

                if (hit == myRect || hit.IsChildOf(myRect) || myRect.IsChildOf(hit))
                    continue;

                if (IsDecorative(result.gameObject))
                    continue;

                return false;
            }
            return true;
        }

        internal static bool IsDecorative(GameObject go)
        {
            return go.GetComponent<Selectable>() == null
                && go.GetComponent<IPointerClickHandler>() == null
                && go.GetComponent<IPointerDownHandler>() == null
                && go.GetComponentInChildren<Toggle>() == null
                && go.GetComponent<UITooltip>() == null
                && go.GetComponent<Slider>() == null;
        }
    }
}
