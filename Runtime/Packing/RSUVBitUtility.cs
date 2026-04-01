using System;
using UnityEngine;

namespace RSUVFramework
{
    public static class RSUVBitUtility
    {
        public static uint EncodeBits(uint data, uint rawValue, int bitOffset, int bitCount)
        {
            ValidateBitRange(bitOffset, bitCount);
            uint bitMask = GetBitMask(bitCount);
            uint shiftedMask = bitCount >= 32 ? uint.MaxValue : (bitMask << bitOffset);
            uint clearedData = data & ~shiftedMask;
            uint shiftedValue = bitCount >= 32 ? (rawValue & bitMask) : ((rawValue & bitMask) << bitOffset);
            return clearedData | shiftedValue;
        }

        public static uint DecodeBits(uint data, int bitOffset, int bitCount)
        {
            ValidateBitRange(bitOffset, bitCount);
            uint bitMask = GetBitMask(bitCount);
            return bitCount >= 32 ? data : ((data >> bitOffset) & bitMask);
        }

        public static uint QuantizeNormalized(float value, int bitCount)
        {
            uint maxRawValue = GetBitMask(bitCount);
            return (uint)Mathf.RoundToInt(Mathf.Clamp01(value) * maxRawValue);
        }

        public static uint QuantizeRange(float value, float minimumValue, float maximumValue, int bitCount)
        {
            if (maximumValue <= minimumValue)
            {
                return 0u;
            }

            float normalizedValue = Mathf.InverseLerp(minimumValue, maximumValue, value);
            return QuantizeNormalized(normalizedValue, bitCount);
        }

        public static uint QuantizeInteger(int value, int minimumValue, int maximumValue)
        {
            int clampedValue = Mathf.Clamp(value, minimumValue, maximumValue);
            return (uint)(clampedValue - minimumValue);
        }

        public static uint QuantizeColor(Color value, int bitCount)
        {
            int channelBitCount = GetColorChannelBitCount(bitCount);
            uint rawValue = QuantizeNormalized(value.r, channelBitCount);

            rawValue |= QuantizeNormalized(value.g, channelBitCount) << channelBitCount;
            rawValue |= QuantizeNormalized(value.b, channelBitCount) << (channelBitCount * 2);
            rawValue |= QuantizeNormalized(value.a, channelBitCount) << (channelBitCount * 3);

            return rawValue;
        }

        public static int GetColorChannelBitCount(int bitCount)
        {
            if (bitCount < 4 || (bitCount % 4) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), "Color fields must use a bit count divisible by 4 and at least 4 bits.");
            }

            return bitCount / 4;
        }

        public static uint GetBitMask(int bitCount)
        {
            if (bitCount >= 32)
            {
                return uint.MaxValue;
            }

            return (1u << bitCount) - 1u;
        }

        private static void ValidateBitRange(int bitOffset, int bitCount)
        {
            if (bitCount <= 0 || bitCount > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            }

            if (bitOffset < 0 || bitOffset > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bitOffset));
            }

            if ((bitOffset + bitCount) > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bitOffset));
            }
        }
    }
}