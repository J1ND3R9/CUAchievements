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
        private const float AlphaDuration = 0.3f;
        private const float FlickerInterval = 0.08f;
        private const float NoiseCell = 3f;
        private const float IdleAlpha = 0.18f;
        private const float HoverAlpha = 0.32f;
        private const float RedStep = 0.33f;

        private static readonly HashSet<string> RevealedIds = [];
        private static readonly List<RevealCover> Revealed = [];
        private static readonly List<RevealCover> Clicked = [];
        private static readonly System.Random Rng = new();
        private static bool OneClickReveal = false;

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
        private int timeClicked = 0;
        private float redAmount = 0f;
        private UITooltip tooltip;
        public bool IsFullyRevealed => revealing && progress >= 1f;

        public void Initialize(string fullId, UITooltip uiTooltip, RectMask2D rectMask, RawImage noiseImage)
        {
            id = fullId;
            mask = rectMask;
            noise = noiseImage;
            tooltip = uiTooltip;
            rect = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            group.ignoreParentGroups = true;
            group.alpha = 1f;

            SetNoiseAlpha();
            Reroll();
        }

        private void OnDestroy() => Revealed.Remove(this);
        public static void ResetAll()
        {
            RevealedIds.Clear();
            for (int i = Revealed.Count - 1; i >= 0; i--)
                Revealed[i].Restore();
            ResetClicks();
        }
        public void Reveal()
        {
            if (!Click()) return;
            if (revealing) return;

            PlayerCamera.PlayUISound("click");

            revealing = true;
            RevealedIds.Add(id);
            group.blocksRaycasts = false;
            Revealed.Add(this);
            tooltip.tipName = "";
        }
        public void Restore()
        {
            if (!revealing) return;

            revealing = false;
            progress = 0f;
            hovered = false;
            group.alpha = 1f;
            group.blocksRaycasts = true;
            gameObject.SetActive(true);

            ResetClicks();
            ResolveStage();

            RevealedIds.Remove(id);
            SetNoiseAlpha();
            Reroll();
        }

        public static void ResetClicks()
        {
            foreach (RevealCover cover in Clicked)
            {
                cover.redAmount = 0f;
                cover.timeClicked = 0;
                cover.SetNoiseAlpha();
                cover.ResolveStage();
            }

            OneClickReveal = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            SetNoiseAlpha();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            SetNoiseAlpha();
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
                progress += deltaTime / AlphaDuration;

                group.alpha = 1f - progress;

                if (progress >= 1f)
                    gameObject.SetActive(false);
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

        private void SetNoiseAlpha()
        {
            float a = hovered ? HoverAlpha : IdleAlpha;
            noise.color = new Color(1f, 1f - redAmount, 1f - redAmount, a);
        }

        private bool Click()
        {
            if (OneClickReveal) return true;


            Clicked.Add(this);
            timeClicked++;
            redAmount += RedStep;
            SetNoiseAlpha();

            if (timeClicked >= 4)
            {
                OneClickReveal = true;
                return true;
            }

            ResolveStage();

            return false;
        }

        private void ResolveStage()
        {
            switch (timeClicked)
            {
                case 1:
                    PlayerCamera.PlayUISound("selfharmtap", 1f);
                    tooltip.tipName = LocaleRegistry.Get("achievement_reveal_first", "Are you sure?");
                    return;
                case 2:
                    PlayerCamera.PlayUISound("selfharmtap", 0.75f);
                    tooltip.tipName = LocaleRegistry.Get("achievement_reveal_second", "Are you ABSOLUTELY sure?");
                    return;
                case 3:
                    PlayerCamera.PlayUISound("selfharmtap", 0.5f);
                    tooltip.tipName = LocaleRegistry.Get("achievement_reveal_third", "This decision cannot be changed!");
                    return;
                default:
                    tooltip.tipName = LocaleRegistry.Get("achievement_reveal", "Reveal");
                    return;
            }
        }
    }

    internal static class RevealCoverBuilder
    {
        private const float Inset = 6f;

        public static RevealCover Build(Transform card, string fullId, UITooltip tooltip, TMP_FontAsset font)
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

            Image tint = UIUtility.Create("Tint", root.transform, typeof(Image)).GetComponent<Image>();
            tint.transform.Stretch();
            tint.color = new Color(1f, 0f, 0f, 0f);
            tint.raycastTarget = false;

            GameObject plate = UIUtility.CreatePanel("Plate", root.transform);
            RectTransform plateRect = (RectTransform)plate.transform;
            plateRect.anchorMin = plateRect.anchorMax = plateRect.pivot = new Vector2(0.5f, 0.5f);
            plateRect.sizeDelta = new Vector2(150f, 44f);
            plate.GetComponent<Image>().raycastTarget = false;

            TextMeshProUGUI mark = UIUtility.CreateText(plate.transform, "Label", "???", font, 28f,
                TextAlignmentOptions.Midline);
            mark.transform.Stretch();
            mark.raycastTarget = false;

            RevealCover cover = root.GetComponent<RevealCover>();
            cover.Initialize(fullId, tooltip, root.GetComponent<RectMask2D>(), noise);

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