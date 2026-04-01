#ifndef GULIEVSTUDIO_RSUV_CORE_INCLUDED
#define GULIEVSTUDIO_RSUV_CORE_INCLUDED

uint RSUV_GetData()
{
    return unity_RendererUserValue;
}

uint RSUV_GetBitMask(uint bitCount)
{
    return bitCount >= 32u ? 0xffffffffu : ((1u << bitCount) - 1u);
}

uint RSUV_GetBits(uint data, uint bitOffset, uint bitCount)
{
    uint bitMask = RSUV_GetBitMask(bitCount);
    return bitCount >= 32u ? data : ((data >> bitOffset) & bitMask);
}

bool RSUV_DecodeBool(uint data, uint bitOffset)
{
    return RSUV_GetBits(data, bitOffset, 1u) != 0u;
}

float RSUV_DecodeNormalized(uint data, uint bitOffset, uint bitCount)
{
    uint rawValue = RSUV_GetBits(data, bitOffset, bitCount);
    uint maxRawValue = RSUV_GetBitMask(bitCount);
    return maxRawValue == 0u ? 0.0f : ((float)rawValue / (float)maxRawValue);
}

int RSUV_DecodeInt(uint data, uint bitOffset, uint bitCount, int minimumValue)
{
    return (int)RSUV_GetBits(data, bitOffset, bitCount) + minimumValue;
}

float RSUV_DecodeFloat(uint data, uint bitOffset, uint bitCount, float minimumValue, float maximumValue)
{
    return lerp(minimumValue, maximumValue, RSUV_DecodeNormalized(data, bitOffset, bitCount));
}

float4 RSUV_DecodeColor(uint data, uint bitOffset, uint bitCount)
{
    uint rawValue = RSUV_GetBits(data, bitOffset, bitCount);
    uint channelBitCount = bitCount / 4u;

    return float4(
        RSUV_DecodeNormalized(rawValue, 0u, channelBitCount),
        RSUV_DecodeNormalized(rawValue, channelBitCount, channelBitCount),
        RSUV_DecodeNormalized(rawValue, channelBitCount * 2u, channelBitCount),
        RSUV_DecodeNormalized(rawValue, channelBitCount * 3u, channelBitCount));
}

#endif