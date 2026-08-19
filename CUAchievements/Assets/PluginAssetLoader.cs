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
            public SpriteEntry(string path, string cacheKey, float ppu = 100f, float border = 0f)
            {
                Path = path;
                CacheKey = cacheKey;
                PixelsPerUnit = ppu;
                Border = border;
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

                new SpriteEntry("Assets.Images.achievement_default_sprite.png", "achievement_default_sprite"),
                new SpriteEntry("Assets.Images.achievement_default_locked.png", "achievement_default_locked"),
                new SpriteEntry("Assets.Images.achievement_default_hidden.png", "achievement_default_hidden"),

                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v1.png", "achievement_popup_frame_v1"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v1alt.png", "achievement_popup_frame_v1alt"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v2.png", "achievement_popup_frame_v2"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v2alt.png", "achievement_popup_frame_v2alt"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v3.png", "achievement_popup_frame_v3"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v3alt.png", "achievement_popup_frame_v3alt"),

                new SpriteEntry("Assets.Images.UI.achievement_frame_image.png", "achievement_frame_image"),
                new SpriteEntry("Assets.Images.UI.achievement_frame_normal.png", "achievement_frame_normal", border: 6f)
            };

            foreach (var entry in spriteEntries)
            {
                var sprite = AssetLoader.LoadEmbeddedSprite(entry.Path, entry.PixelsPerUnit);
                if (entry.Border > 0f)
                    sprite = SpriteSlicer.MakeSliced(sprite, entry.Border);
                LoadedSprites[entry.CacheKey] = sprite;
            }


            var audioEntries = new AudioEntry[]
            {
                new AudioEntry("Assets.Audio.Unlock.unlock0.wav", "achievement_sound_unlock_0"),
            };

            foreach (var entry in audioEntries)
            {
                var clip = AssetLoader.LoadEmbeddedAudio(entry.Path);
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
