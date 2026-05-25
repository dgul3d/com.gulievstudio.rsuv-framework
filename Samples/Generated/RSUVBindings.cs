using UnityEngine;

namespace RSUVFramework
{
    public static class RSUVBindings
    {
        public static readonly RSUVFieldKey<int> AtlasIndex = new RSUVFieldKey<int>("AtlasIndex");
        public static readonly RSUVFieldKey<bool> IsFlickering = new RSUVFieldKey<bool>("IsFlickering");
        public static readonly RSUVFieldKey<Color> QuantizedColor = new RSUVFieldKey<Color>("QuantizedColor");
        public static readonly RSUVFieldKey<int> RotationSpeed = new RSUVFieldKey<int>("RotationSpeed");

        public static void SetAtlasIndex(this RSUVRendererValueWriter writer, int value)
        {
            writer.SetInt(AtlasIndex, value);
        }

        public static void SetIsFlickering(this RSUVRendererValueWriter writer, bool value)
        {
            writer.SetBool(IsFlickering, value);
        }

        public static void SetQuantizedColor(this RSUVRendererValueWriter writer, Color value)
        {
            writer.SetColor(QuantizedColor, value);
        }

        public static void SetRotationSpeed(this RSUVRendererValueWriter writer, int value)
        {
            writer.SetInt(RotationSpeed, value);
        }

    }
}
