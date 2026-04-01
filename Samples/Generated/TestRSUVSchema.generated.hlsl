#ifndef RSUVSCHEMA_GENERATED_INCLUDED
#define RSUVSCHEMA_GENERATED_INCLUDED

#include "Packages/com.gulievstudio.rsuv-framework/ShaderLibrary/RSUVCore.hlsl"

static const uint RSUVSchema_MyCol_OFFSET = 0u;
static const uint RSUVSchema_MyCol_BITS = 8u;

uint RSUVSchema_GetMyColRawFromData(uint data)
{
    return RSUV_GetBits(data, RSUVSchema_MyCol_OFFSET, RSUVSchema_MyCol_BITS);
}

uint RSUVSchema_GetMyColRaw()
{
    return RSUVSchema_GetMyColRawFromData(RSUV_GetData());
}

float4 RSUVSchema_GetMyColFromData(uint data)
{
    return RSUV_DecodeColor(data, RSUVSchema_MyCol_OFFSET, RSUVSchema_MyCol_BITS);
}

float4 RSUVSchema_GetMyCol()
{
    return RSUVSchema_GetMyColFromData(RSUV_GetData());
}

void RSUVSchema_GetMyCol_float(out float4 Value)
{
    Value = RSUVSchema_GetMyCol();
}

void RSUVSchema_GetMyCol_half(out half4 Value)
{
    Value = (half4)RSUVSchema_GetMyCol();
}

#endif // RSUVSCHEMA_GENERATED_INCLUDED
