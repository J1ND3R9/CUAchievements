using CUAchievements.UI;
using CUCoreLib.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace CUAchievements.Assets
{
    internal static class TextureExtension
    {
        public static void ApplyAlphaIsTransparency(this Texture2D texture, int dilationPixels = 2)
        {
            if (!texture.isReadable)
            {
                return;
            }

            int width = texture.width;
            int height = texture.height;
            Color[] originalPixels = texture.GetPixels();
            Color[] processedPixels = (Color[])originalPixels.Clone();

            for (int pass = 0; pass < dilationPixels; pass++)
            {
                Color[] currentPixels = (Color[])processedPixels.Clone();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        if (Mathf.Approximately(processedPixels[index].a, 0f))
                        {
                            Color? neighborColor = GetNeighborColor(currentPixels, x, y, width, height);
                            if (neighborColor.HasValue)
                            {
                                processedPixels[index] = new Color(
                                    neighborColor.Value.r,
                                    neighborColor.Value.g,
                                    neighborColor.Value.b,
                                    0f
                                );
                            }
                        }
                    }
                }
            }

            texture.SetPixels(processedPixels);
            texture.Apply(true, true);
        }

        private static Color? GetNeighborColor(Color[] pixels, int x, int y, int width, int height)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        Color neighbor = pixels[ny * width + nx];
                        if (neighbor.a > 0f)
                        {
                            return neighbor;
                        }
                    }
                }
            }
            return null;
        }
    }
    internal class PluginAssetLoader
    {
        private readonly struct SpriteEntry
        {
            public string Path { get; }
            public string CacheKey { get; }
            public float PixelsPerUnit { get; }
            public float Border { get; }
            public FilterMode? Filter { get; }
            public bool AplhaIsTransparency { get; }
            public SpriteEntry(string path, string cacheKey, float ppu = 100f, float border = 0f, FilterMode? filter = null, bool alphaIsTransparency = false)
            {
                Path = path;
                CacheKey = cacheKey;
                PixelsPerUnit = ppu;
                Border = border;
                Filter = filter;
                AplhaIsTransparency = alphaIsTransparency;
            }
        }
        private readonly struct AudioEntry
        {
            public string Path { get; }
            public string CacheKey { get; }

            public AudioEntry(string path, string cacheKey)
            {
                Path = path;
                CacheKey = cacheKey;
            }
        }

        private static readonly Dictionary<string, Sprite> LoadedSprites = [];
        private static readonly Dictionary<string, AudioClip> LoadedAudioClips = [];
        internal static void LoadAssets()
        {
            SpriteEntry[] spriteEntries =
            [
                new("Assets.Images.UI.achievement_button.png", "achievement_button", filter: FilterMode.Bilinear, alphaIsTransparency: true),
                new("Assets.Images.UI.achievement_button_overlay.png", "achievement_button_overlay", filter: FilterMode.Bilinear, alphaIsTransparency: true),
                new("Assets.Images.UI.achievement_button_glow.png", "achievement_button_glow", filter: FilterMode.Bilinear, alphaIsTransparency: true),
                new("Assets.Images.UI.achievement_pause_button.png", "achievement_pause_button"),
                new("Assets.Images.UI.statusarrow.png", "statusarrow"),


                new("Assets.Images.achievement_default_sprite.png", "achievement_default_sprite", filter: FilterMode.Bilinear),
                new("Assets.Images.achievement_default_locked.png", "achievement_default_locked", filter: FilterMode.Bilinear),
                new("Assets.Images.achievement_default_hidden.png", "achievement_default_hidden", filter: FilterMode.Bilinear),
                new("Assets.Images.achievement_preview.png", "achievement_preview", filter: FilterMode.Bilinear),
#if DEBUG
                new("Assets.Images.achievement_debug_icon.png", "achievement_debug_icon", filter: FilterMode.Bilinear),
#endif

                new("Assets.Images.UI.achievement_frame_image.png", "achievement_frame_image"),
                new("Assets.Images.UI.achievement_frame_normal.png", "achievement_frame_normal", border: 6f)
            ];

            foreach (SpriteEntry entry in spriteEntries)
            {
                Sprite sprite = AssetLoader.LoadEmbeddedSprite(entry.Path, entry.PixelsPerUnit);
                if (entry.Border > 0f)
                    sprite = SpriteSlicer.MakeSliced(sprite, entry.Border);

                if (entry.Filter.HasValue)
                    sprite.texture.filterMode = entry.Filter.Value;
                if (entry.AplhaIsTransparency)
                    sprite.texture.ApplyAlphaIsTransparency();
                LoadedSprites[entry.CacheKey] = sprite;
            }


            AudioEntry[] audioEntries =
            [
                new AudioEntry("Assets.Audio.Unlock.unlock0.wav", "achievement_sound_unlock_0"),
            ];

            foreach (AudioEntry entry in audioEntries)
            {
                AudioClip clip = AssetLoader.LoadEmbeddedAudio(entry.Path);
                LoadedAudioClips[entry.CacheKey] = clip;
            }

            CacheAssets();
        }

        private static void CacheAssets()
        {
            foreach (KeyValuePair<string, Sprite> embeddedSprite in LoadedSprites)
                AssetLoader.CacheSprite(embeddedSprite.Key, embeddedSprite.Value);
            foreach (KeyValuePair<string, AudioClip> embeddedAudio in LoadedAudioClips)
                AssetLoader.CacheAudioClip(embeddedAudio.Key, embeddedAudio.Value);
        }
    }
}
