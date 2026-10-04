using CUAchievements.UI;
using CUCoreLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CUAchievements.Assets
{

    internal class PluginAssetLoader
    {
        private readonly struct SpriteEntry
        {
            public string Path { get; }
            public string CacheKey { get; }
            public float PixelsPerUnit { get; }
            public float Border { get; }
            public FilterMode? Filter { get; }
            public SpriteEntry(string path, string cacheKey, float ppu = 100f, float border = 0f, FilterMode? filter = null)
            {
                Path = path;
                CacheKey = cacheKey;
                PixelsPerUnit = ppu;
                Border = border;
                Filter = filter;
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

        private static readonly Dictionary<string, Sprite> LoadedSprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, AudioClip> LoadedAudioClips = new Dictionary<string, AudioClip>();
        internal static void LoadAssets()
        {
            var spriteEntries = new SpriteEntry[]
            {
                new SpriteEntry("Assets.Images.UI.achievement_button.png", "achievement_button"),
                new SpriteEntry("Assets.Images.UI.achievement_button_overlay.png", "achievement_button_overlay"),


                new SpriteEntry("Assets.Images.achievement_default_sprite.png", "achievement_default_sprite", filter: FilterMode.Bilinear),
                new SpriteEntry("Assets.Images.achievement_default_locked.png", "achievement_default_locked", filter: FilterMode.Bilinear),
                new SpriteEntry("Assets.Images.achievement_default_hidden.png", "achievement_default_hidden", filter: FilterMode.Bilinear),
                new SpriteEntry("Assets.Images.achievement_debug_icon.png", "achievement_debug_icon", filter: FilterMode.Bilinear),

                new SpriteEntry("Assets.Images.UI.achievement_frame_image.png", "achievement_frame_image"),
                new SpriteEntry("Assets.Images.UI.achievement_frame_normal.png", "achievement_frame_normal", border: 6f)
            };

            foreach (var entry in spriteEntries)
            {
                Sprite sprite = AssetLoader.LoadEmbeddedSprite(entry.Path, entry.PixelsPerUnit);
                if (entry.Border > 0f)
                    sprite = SpriteSlicer.MakeSliced(sprite, entry.Border);

                if (entry.Filter.HasValue)
                    sprite.texture.filterMode = entry.Filter.Value;

                LoadedSprites[entry.CacheKey] = sprite;
            }


            var audioEntries = new AudioEntry[]
            {
                new AudioEntry("Assets.Audio.Unlock.unlock0.wav", "achievement_sound_unlock_0"),
            };

            foreach (var entry in audioEntries)
            {
                AudioClip clip = AssetLoader.LoadEmbeddedAudio(entry.Path);
                LoadedAudioClips[entry.CacheKey] = clip;
            }

            CacheAssets();
        }

        private static void CacheAssets()
        {
            foreach (var embeddedSprite in LoadedSprites)
                AssetLoader.CacheSprite(embeddedSprite.Key, embeddedSprite.Value);
            foreach (var embeddedAudio in LoadedAudioClips)
                AssetLoader.CacheAudioClip(embeddedAudio.Key, embeddedAudio.Value);
        }
    }
}
