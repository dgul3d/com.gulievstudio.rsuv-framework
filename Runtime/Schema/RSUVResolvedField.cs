using UnityEngine;

namespace RSUVFramework
{
    public sealed class RSUVResolvedField
    {
        public RSUVResolvedField(
            string name,
            string identifier,
            RSUVFieldType fieldType,
            int bitOffset,
            int bitCount,
            uint maxRawValue,
            int minimumIntegerValue,
            int maximumIntegerValue,
            float minimumFloatValue,
            float maximumFloatValue,
            int defaultIntegerValue,
            float defaultFloatValue,
            Color defaultColorValue,
            bool defaultBooleanValue)
        {
            Name = name;
            Identifier = identifier;
            FieldType = fieldType;
            BitOffset = bitOffset;
            BitCount = bitCount;
            MaxRawValue = maxRawValue;
            MinimumIntegerValue = minimumIntegerValue;
            MaximumIntegerValue = maximumIntegerValue;
            MinimumFloatValue = minimumFloatValue;
            MaximumFloatValue = maximumFloatValue;
            DefaultIntegerValue = defaultIntegerValue;
            DefaultFloatValue = defaultFloatValue;
            DefaultColorValue = defaultColorValue;
            DefaultBooleanValue = defaultBooleanValue;
        }

        public string Name { get; }

        public string Identifier { get; }

        public RSUVFieldType FieldType { get; }

        public int BitOffset { get; }

        public int BitCount { get; }

        public uint MaxRawValue { get; }

        public int MinimumIntegerValue { get; }

        public int MaximumIntegerValue { get; }

        public float MinimumFloatValue { get; }

        public float MaximumFloatValue { get; }

        public int DefaultIntegerValue { get; }

        public float DefaultFloatValue { get; }

        public Color DefaultColorValue { get; }

        public bool DefaultBooleanValue { get; }

        public int ColorChannelBitCount => BitCount / 4;
    }
}