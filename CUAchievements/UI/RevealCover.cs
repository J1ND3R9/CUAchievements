using CUAchievements.UI.Utility;
using CUCoreLib.Registries;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    // I struggled for a couple of hours, and then I just vibecoded it. Sorry about that.
    internal sealed class RevealCover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float WipeDuration = 0.3f;
        private const float FlickerInterval = 0.08f;
        private const float NoiseCell = 3f;
        private const float IdleAlpha = 0.18f;
        private const float HoverAlpha = 0.32f;

        private static readonly HashSet<string> RevealedIds = [];
        private static readonly System.Random Rng = new();

        public static bool IsRevealed(string fullId) => RevealedIds.Contains(fullId);

        private RectTransform rect;
        private RectMask2D mask;
        private RawImage noise;
        private CanvasGroup group;
        private string id;
        private Vector2 offset;
        private bool hovered;
        private bool revealing;
        private float timer;
        private float progress;

        public void Initialize(string fullId, RectMask2D rectMask, RawImage noiseImage)
        {
            id = fullId;
            mask = rectMask;
            noise = noiseImage;
            rect = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            group.ignoreParentGroups = true;
            group.alpha = 1f;

            SetNoiseAlpha(IdleAlpha);
            Reroll();
        }

        public void Reveal()
        {
            if (revealing) return;
            revealing = true;
            RevealedIds.Add(id);
            group.blocksRaycasts = false;

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            SetNoiseAlpha(HoverAlpha);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            SetNoiseAlpha(IdleAlpha);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (noise != null) UpdateUv();
        }

        private void Update()
        {
            if (!revealing && !hovered) return;

            float deltaTime = Time.unscaledDeltaTime;

            if (revealing)
            {
                progress += deltaTime / WipeDuration;

                mask.padding = new Vector4(rect.rect.width * Mathf.Clamp01(progress), 0f, 0f, 0f);

                if (progress >= 1f)
                    Destroy(gameObject);
                return;
            }

            timer -= deltaTime;
            if (timer <= 0f)
            {
                timer = FlickerInterval;
                Reroll();
            }
        }

        private void Reroll()
        {
            offset = new Vector2((float)Rng.NextDouble(), (float)Rng.NextDouble());
            UpdateUv();
        }

        private void UpdateUv()
        {
            Vector2 size = rect.rect.size;
            float tile = NoiseTexture.Size * NoiseCell;
            noise.uvRect = new Rect(offset.x, offset.y, size.x / tile, size.y / tile);
        }

        private void SetNoiseAlpha(float alpha)
        {
            noise.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    internal static class RevealCoverBuilder
    {
        private const float Inset = 6f;

        public static RevealCover Build(Transform card, string fullId, TMP_FontAsset font)
        {
            GameObject root = UIUtility.Create("Cover", card, typeof(RectMask2D), typeof(CanvasGroup), typeof(LayoutIgnorer), typeof(RevealCover));
            root.transform.Stretch(new Vector2(Inset, Inset), new Vector2(-Inset, -Inset));

            Image back = UIUtility.Create("Back", root.transform, typeof(Image)).GetComponent<Image>();
            back.transform.Stretch();
            back.color = new Color(0.055f, 0.055f, 0.055f, 1f);

            RawImage noise = UIUtility.Create("Noise", root.transform, typeof(RawImage)).GetComponent<RawImage>();
            noise.transform.Stretch();
            noise.texture = NoiseTexture.Get();
            noise.raycastTarget = false;

            GameObject plate = UIUtility.CreatePanel("Plate", root.transform);
            RectTransform plateRect = (RectTransform)plate.transform;
            plateRect.anchorMin = plateRect.anchorMax = plateRect.pivot = new Vector2(0.5f, 0.5f);
            plateRect.sizeDelta = new Vector2(150f, 44f);
            plate.GetComponent<Image>().raycastTarget = false;

            TextMeshProUGUI mark = UIUtility.CreateText(plate.transform, "Label", "???", font, 28f,
                TextAlignmentOptions.Midline);
            mark.transform.Stretch();
            mark.raycastTarget = false;

            UITooltip tooltip = root.AddComponent<UITooltip>();
            tooltip.skipLocale = true;
            tooltip.tipName = LocaleRegistry.Get("achievement_reveal", "Reveal");

            RevealCover cover = root.GetComponent<RevealCover>();
            cover.Initialize(fullId, root.GetComponent<RectMask2D>(), noise);
            return cover;
        }
    }

    internal static class NoiseTexture
    {
        public const int Size = 64;

        private static Texture2D texture;

        public static Texture2D Get()
        {
            if (texture != null) return texture;

            texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };

            System.Random rng = new(1234);
            Color32[] pixels = new Color32[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
            {
                bool on = rng.NextDouble() < 0.3;
                pixels[i] = on ? new Color32(255, 255, 255, (byte)(120 + rng.Next(136))) : new Color32(0, 0, 0, 0);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}