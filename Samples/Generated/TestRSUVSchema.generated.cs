using UnityEngine;

namespace RSUVFramework.Generated
{
    public static class TestRSUVSchemaApi
    {
        public static readonly RSUVFieldKey<Color> MyCol = new RSUVFieldKey<Color>("MyCol");

        public static void SetMyCol(this RSUVRendererValueWriter writer, Color value)
        {
            writer.SetColor(MyCol, value);
        }

    }
}
