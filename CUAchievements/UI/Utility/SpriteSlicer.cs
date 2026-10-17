using UnityEngine;

namespace CUAchievements.UI
{
    internal static class SpriteSlicer
    {
        internal static Sprite MakeSliced(Sprite source , float border)
        {
            return MakeSliced(source, new Vector4(border, border, border, border));
        }

        internal static Sprite MakeSliced(Sprite source, Vector4 border)
        {
            if (source == null) return null;
            return Sprite.Create(
                source.texture,
                source.rect,
                new Vector2(0.5f, 0.5f),
                source.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect,
                border
                );
        }
    }
}
