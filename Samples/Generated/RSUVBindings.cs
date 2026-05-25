using UnityEngine;

namespace RSUVFramework
{
    public static class RSUVBindings
    {
        public static readonly RSUVFieldKey<int> AlternateRSUVSchema_AtlasIndex = new RSUVFieldKey<int>("AtlasIndex");
        public static readonly RSUVFieldKey<bool> AlternateRSUVSchema_IsFlickering = new RSUVFieldKey<bool>("IsFlickering");
        public static readonly RSUVFieldKey<int> RSUVSchema_RotationSpeed = new RSUVFieldKey<int>("RotationSpeed");
        public static readonly RSUVFieldKey<Color> RSUVSchema_QuantizedColor = new RSUVFieldKey<Color>("QuantizedColor");
        public static readonly RSUVFieldKey<bool> RSUVSchema_IsFlickering = new RSUVFieldKey<bool>("IsFlickering");
        public static readonly RSUVFieldKey<int> RSUVSchema_AtlasIndex = new RSUVFieldKey<int>("AtlasIndex");

        public static void SetAlternateRSUVSchema_AtlasIndex(this RSUVRendererValueWriter writer, int value)
        {
            writer.SetInt(AlternateRSUVSchema_AtlasIndex, value);
        }

        public static void SetAlternateRSUVSchema_IsFlickering(this RSUVRendererValueWriter writer, bool value)
        {
            writer.SetBool(AlternateRSUVSchema_IsFlickering, value);
        }

        public static void SetRSUVSchema_RotationSpeed(this RSUVRendererValueWriter writer, int value)
        {
            writer.SetInt(RSUVSchema_RotationSpeed, value);
        }

        public static void SetRSUVSchema_QuantizedColor(this RSUVRendererValueWriter writer, Color value)
        {
            writer.SetColor(RSUVSchema_QuantizedColor, value);
        }

        public static void SetRSUVSchema_IsFlickering(this RSUVRendererValueWriter writer, bool value)
        {
            writer.SetBool(RSUVSchema_IsFlickering, value);
        }

        public static void SetRSUVSchema_AtlasIndex(this RSUVRendererValueWriter writer, int value)
        {
            writer.SetInt(RSUVSchema_AtlasIndex, value);
        }

    }
}
