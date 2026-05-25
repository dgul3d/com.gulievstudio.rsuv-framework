#ifndef RSUV_BINDINGS_INCLUDED
#define RSUV_BINDINGS_INCLUDED

#include "Packages/com.gulievstudio.rsuv-framework/ShaderLibrary/RSUVCore.hlsl"

static const uint AlternateRSUVSchema_AtlasIndex_OFFSET = 0u;
static const uint AlternateRSUVSchema_AtlasIndex_BITS = 2u;
static const int AlternateRSUVSchema_AtlasIndex_MIN_INT = 0;
static const int AlternateRSUVSchema_AtlasIndex_MAX_INT = 3;

uint AlternateRSUVSchema_GetAtlasIndexRawFromData(uint data)
{
    return RSUV_GetBits(data, AlternateRSUVSchema_AtlasIndex_OFFSET, AlternateRSUVSchema_AtlasIndex_BITS);
}

uint AlternateRSUVSchema_GetAtlasIndexRaw()
{
    return AlternateRSUVSchema_GetAtlasIndexRawFromData(RSUV_GetData());
}

int AlternateRSUVSchema_GetAtlasIndexFromData(uint data)
{
    return RSUV_DecodeInt(data, AlternateRSUVSchema_AtlasIndex_OFFSET, AlternateRSUVSchema_AtlasIndex_BITS, AlternateRSUVSchema_AtlasIndex_MIN_INT);
}

int AlternateRSUVSchema_GetAtlasIndex() //CUSTOM NODE READY
{
    return AlternateRSUVSchema_GetAtlasIndexFromData(RSUV_GetData());
}

void AlternateRSUVSchema_GetAtlasIndex_float(out float Value)
{
    Value = (float)AlternateRSUVSchema_GetAtlasIndex();
}

void AlternateRSUVSchema_GetAtlasIndex_half(out half Value)
{
    Value = (half)AlternateRSUVSchema_GetAtlasIndex();
}

static const uint AlternateRSUVSchema_IsFlickering_OFFSET = 2u;
static const uint AlternateRSUVSchema_IsFlickering_BITS = 1u;

uint AlternateRSUVSchema_GetIsFlickeringRawFromData(uint data)
{
    return RSUV_GetBits(data, AlternateRSUVSchema_IsFlickering_OFFSET, AlternateRSUVSchema_IsFlickering_BITS);
}

uint AlternateRSUVSchema_GetIsFlickeringRaw()
{
    return AlternateRSUVSchema_GetIsFlickeringRawFromData(RSUV_GetData());
}

bool AlternateRSUVSchema_GetIsFlickeringFromData(uint data)
{
    return RSUV_DecodeBool(data, AlternateRSUVSchema_IsFlickering_OFFSET);
}

bool AlternateRSUVSchema_GetIsFlickering() //CUSTOM NODE READY
{
    return AlternateRSUVSchema_GetIsFlickeringFromData(RSUV_GetData());
}

float AlternateRSUVSchema_GetIsFlickeringAsFloat()
{
    return AlternateRSUVSchema_GetIsFlickering() ? 1.0f : 0.0f;
}

void AlternateRSUVSchema_GetIsFlickering_float(out float Value)
{
    Value = AlternateRSUVSchema_GetIsFlickeringAsFloat();
}

void AlternateRSUVSchema_GetIsFlickering_half(out half Value)
{
    Value = AlternateRSUVSchema_GetIsFlickering() ? (half)1.0h : (half)0.0h;
}


static const uint RSUVSchema_RotationSpeed_OFFSET = 0u;
static const uint RSUVSchema_RotationSpeed_BITS = 3u;
static const int RSUVSchema_RotationSpeed_MIN_INT = -3;
static const int RSUVSchema_RotationSpeed_MAX_INT = 3;

uint RSUVSchema_GetRotationSpeedRawFromData(uint data)
{
    return RSUV_GetBits(data, RSUVSchema_RotationSpeed_OFFSET, RSUVSchema_RotationSpeed_BITS);
}

uint RSUVSchema_GetRotationSpeedRaw()
{
    return RSUVSchema_GetRotationSpeedRawFromData(RSUV_GetData());
}

int RSUVSchema_GetRotationSpeedFromData(uint data)
{
    return RSUV_DecodeInt(data, RSUVSchema_RotationSpeed_OFFSET, RSUVSchema_RotationSpeed_BITS, RSUVSchema_RotationSpeed_MIN_INT);
}

int RSUVSchema_GetRotationSpeed() //CUSTOM NODE READY
{
    return RSUVSchema_GetRotationSpeedFromData(RSUV_GetData());
}

void RSUVSchema_GetRotationSpeed_float(out float Value)
{
    Value = (float)RSUVSchema_GetRotationSpeed();
}

void RSUVSchema_GetRotationSpeed_half(out half Value)
{
    Value = (half)RSUVSchema_GetRotationSpeed();
}

static const uint RSUVSchema_QuantizedColor_OFFSET = 3u;
static const uint RSUVSchema_QuantizedColor_BITS = 12u;

uint RSUVSchema_GetQuantizedColorRawFromData(uint data)
{
    return RSUV_GetBits(data, RSUVSchema_QuantizedColor_OFFSET, RSUVSchema_QuantizedColor_BITS);
}

uint RSUVSchema_GetQuantizedColorRaw()
{
    return RSUVSchema_GetQuantizedColorRawFromData(RSUV_GetData());
}

float4 RSUVSchema_GetQuantizedColorFromData(uint data)
{
    return RSUV_DecodeColor(data, RSUVSchema_QuantizedColor_OFFSET, RSUVSchema_QuantizedColor_BITS);
}

float4 RSUVSchema_GetQuantizedColor() //CUSTOM NODE READY
{
    return RSUVSchema_GetQuantizedColorFromData(RSUV_GetData());
}

void RSUVSchema_GetQuantizedColor_float(out float4 Value)
{
    Value = RSUVSchema_GetQuantizedColor();
}

void RSUVSchema_GetQuantizedColor_half(out half4 Value)
{
    Value = (half4)RSUVSchema_GetQuantizedColor();
}

static const uint RSUVSchema_IsFlickering_OFFSET = 15u;
static const uint RSUVSchema_IsFlickering_BITS = 1u;

uint RSUVSchema_GetIsFlickeringRawFromData(uint data)
{
    return RSUV_GetBits(data, RSUVSchema_IsFlickering_OFFSET, RSUVSchema_IsFlickering_BITS);
}

uint RSUVSchema_GetIsFlickeringRaw()
{
    return RSUVSchema_GetIsFlickeringRawFromData(RSUV_GetData());
}

bool RSUVSchema_GetIsFlickeringFromData(uint data)
{
    return RSUV_DecodeBool(data, RSUVSchema_IsFlickering_OFFSET);
}

bool RSUVSchema_GetIsFlickering() //CUSTOM NODE READY
{
    return RSUVSchema_GetIsFlickeringFromData(RSUV_GetData());
}

float RSUVSchema_GetIsFlickeringAsFloat()
{
    return RSUVSchema_GetIsFlickering() ? 1.0f : 0.0f;
}

void RSUVSchema_GetIsFlickering_float(out float Value)
{
    Value = RSUVSchema_GetIsFlickeringAsFloat();
}

void RSUVSchema_GetIsFlickering_half(out half Value)
{
    Value = RSUVSchema_GetIsFlickering() ? (half)1.0h : (half)0.0h;
}

static const uint RSUVSchema_AtlasIndex_OFFSET = 16u;
static const uint RSUVSchema_AtlasIndex_BITS = 2u;
static const int RSUVSchema_AtlasIndex_MIN_INT = 0;
static const int RSUVSchema_AtlasIndex_MAX_INT = 3;

uint RSUVSchema_GetAtlasIndexRawFromData(uint data)
{
    return RSUV_GetBits(data, RSUVSchema_AtlasIndex_OFFSET, RSUVSchema_AtlasIndex_BITS);
}

uint RSUVSchema_GetAtlasIndexRaw()
{
    return RSUVSchema_GetAtlasIndexRawFromData(RSUV_GetData());
}

int RSUVSchema_GetAtlasIndexFromData(uint data)
{
    return RSUV_DecodeInt(data, RSUVSchema_AtlasIndex_OFFSET, RSUVSchema_AtlasIndex_BITS, RSUVSchema_AtlasIndex_MIN_INT);
}

int RSUVSchema_GetAtlasIndex() //CUSTOM NODE READY
{
    return RSUVSchema_GetAtlasIndexFromData(RSUV_GetData());
}

void RSUVSchema_GetAtlasIndex_float(out float Value)
{
    Value = (float)RSUVSchema_GetAtlasIndex();
}

void RSUVSchema_GetAtlasIndex_half(out half Value)
{
    Value = (half)RSUVSchema_GetAtlasIndex();
}

#endif // RSUV_BINDINGS_INCLUDED
