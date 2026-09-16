using System.Collections;
using UnityEngine;

namespace CUAchievements.UI
{
    internal static class AnimationUtility
    {
        public static IEnumerator SlideTo(Vector2 target, RectTransform rect)
        {
            Vector2 start = rect.anchoredPosition;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                rect.anchoredPosition = Vector2.Lerp(start, target, eased);
                yield return null;
            }

            rect.anchoredPosition = target;
        }
    }
}
