using CUAchievements.UI;
using CUCoreLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
            public SpriteEntry(string path, string cacheKey, float ppu = 100f)
            {
                Path = path;
                CacheKey = cacheKey;
                PixelsPerUnit = ppu;
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

        internal static readonly Dictionary<string, Sprite> LoadedSprites = new Dictionary<string, Sprite>();
        internal static readonly Dictionary<string, AudioClip> LoadedAudioClips = new Dictionary<string, AudioClip>();
        internal static void LoadAssets()
        {
            var spriteEntries = new SpriteEntry[]
            {
                new SpriteEntry("Assets.Images.UI.achievement_button.png", "achievement_button"),
                new SpriteEntry("Assets.Images.UI.achievement_button_overlay.png", "achievement_button_overlay"),
                new SpriteEntry("Assets.Images.achievement_default_sprite.png", "achievement_default_sprite"),
                new SpriteEntry("Assets.Images.achievement_default_locked.png", "achievement_default_locked"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v1.png", "achievement_popup_frame_v1"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v1alt.png", "achievement_popup_frame_v1alt"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v2.png", "achievement_popup_frame_v2"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v2alt.png", "achievement_popup_frame_v2alt"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v3.png", "achievement_popup_frame_v3"),
                new SpriteEntry("Assets.Images.PopUp.achievement_popup_frame_v3alt.png", "achievement_popup_frame_v3alt"),
                new SpriteEntry("Assets.Images.UI.achievement_frame_image.png", "achievement_frame_image"),
                new SpriteEntry("Assets.Images.UI.achievement_frame_common.png", "achievement_frame_common")
            };

            foreach (var entry in spriteEntries)
            {
                var sprite = AssetLoader.LoadEmbeddedSprite(entry.Path, entry.PixelsPerUnit);
                LoadedSprites[entry.CacheKey] = sprite;
            }

            if (LoadedSprites.TryGetValue("achievement_frame_common", out var commonFrame))
            {
                var sliced = SpriteSlicer.MakeSliced(commonFrame, 6f);
                LoadedSprites["achievement_frame_common"] = sliced;
            }

            var audioEntries = new AudioEntry[]
            {
                new AudioEntry("Assets.Audio.achievement_sound_unlocked.mp3", "achievement_sound_unlocked"),
            };

            foreach (var entry in audioEntries)
            {
                var clip = AssetLoader.LoadEmbeddedAudio(entry.Path);
                LoadedAudioClips[entry.CacheKey] = clip;
            }

            CacheAssets();
        }

        internal static void CacheAssets()
        {
            foreach (var embeddedSprite in LoadedSprites)
                AssetLoader.CacheSprite(embeddedSprite.Key, embeddedSprite.Value);
            foreach (var embeddedAudio in LoadedAudioClips)
                AssetLoader.CacheAudioClip(embeddedAudio.Key, embeddedAudio.Value);
        }
    }
}
