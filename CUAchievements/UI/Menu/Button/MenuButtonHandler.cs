using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CUAchievements.UI.Menu.CustomButton
{
    internal class MenuButtonHandler : MonoBehaviour
    {
        private RectTransform clickRect;
        private Image overlay;
        private TextMeshProUGUI label;
        private Action onClicked;
        private const float FadeSpeed = 0.5f;

        private static readonly List<MenuButtonHandler> Active = [];
        private static readonly List<RaycastResult> raycastResultsBuffer = [];

        public void Initialize(RectTransform clickRect, Image overlay, TextMeshProUGUI label, Action onClicked)
        {
            this.clickRect = clickRect;
            this.overlay = overlay;
            this.label = label;
            this.onClicked = onClicked;
        }

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        private void Update()
        {
            if (clickRect == null) return;

            bool hovering = IsPointerOver(out bool topmost) && topmost;

            float target = (hovering && !MainMenuWindow.IsOpen) ? 1f : 0f;
            float step = FadeSpeed * Time.unscaledDeltaTime;

            Color oc = overlay.color;
            oc.a = Mathf.MoveTowards(oc.a, target, step);
            overlay.color = oc;

            //var origc = original.color;
            //origc.a = Mathf.MoveTowards(origc.a, hovering ? 0f : 1f, step);
            //original.color = origc;

            Color lc = label.color;
            lc.a = Mathf.MoveTowards(lc.a, target, step);
            label.color = lc;

            if (hovering && Input.GetMouseButtonDown(0))
            {
                PlayerCamera.PlayUISound("click", 1f);
                onClicked?.Invoke();
            }
        }

        private bool IsPointerOver(out bool topmost)
        {
            topmost = true;

            if (EventSystem.current == null)
                return false;

            Canvas canvas = clickRect.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

            if (!RectTransformUtility.RectangleContainsScreenPoint(clickRect, Input.mousePosition, camera))
                return false;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            raycastResultsBuffer.Clear();
            EventSystem.current.RaycastAll(pointerData, raycastResultsBuffer);

            foreach (RaycastResult result in raycastResultsBuffer)
            {
                Transform hit = result.gameObject.transform;

                if (hit == clickRect || hit.IsChildOf(clickRect) || clickRect.IsChildOf(hit))
                    continue;

                if (IsDecorative(result.gameObject))
                    continue;

                topmost = false;
                return true;
            }

            return true;
        }
        public static bool IsPointerOverAny()
        {
            for (int i = 0; i < Active.Count; i++)
                if (Active[i].ContainsPointer())
                    return true;
            return false;
        }
        private bool ContainsPointer(float inset = 0f)
        {
            if (clickRect == null) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, Input.mousePosition, null, out Vector2 local))
                return false;

            Rect r = clickRect.rect;
            r.xMin += inset;
            r.xMax -= inset;
            r.yMin += inset;
            r.yMax -= inset;

            return r.Contains(local);
        }

        private static bool IsDecorative(GameObject go)
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