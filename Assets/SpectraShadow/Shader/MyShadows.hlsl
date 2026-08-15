#ifndef UNIVERSAL_SHADOWS_INCLUDED
#define UNIVERSAL_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.deprecated.hlsl"

#define MAX_SHADOW_CASCADES 4


    #if defined(_ADDITIONAL_LIGHT_SHADOWS)
        #define ADDITIONAL_LIGHT_CALCULATE_SHADOWS
    #endif


    #define SAMPLE_SHADOWMASK(uv) half4(1, 1, 1, 1);

#define REQUIRES_WORLD_SPACE_POS_INTERPOLATOR

#if defined(LIGHTMAP_ON) || defined(LIGHTMAP_SHADOW_MIXING) || defined(SHADOWS_SHADOWMASK)
#define CALCULATE_BAKED_SHADOWS
#endif

TEXTURE2D_X(_ScreenSpaceShadowmapTexture);

TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
TEXTURE2D_SHADOW(_AdditionalLightsShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);

// GLES3 causes a performance regression in some devices when using CBUFFER.
#ifndef SHADER_API_GLES3
CBUFFER_START(LightShadows)
#endif

// Last cascade is initialized with a no-op matrix. It always transforms
// shadow coord to half3(0, 0, NEAR_PLANE). We use this trick to avoid
// branching since ComputeCascadeIndex can return cascade index = MAX_SHADOW_CASCADES
float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
float4      _CascadeShadowSplitSpheres0;
float4      _CascadeShadowSplitSpheres1;
float4      _CascadeShadowSplitSpheres2;
float4      _CascadeShadowSplitSpheres3;
float4      _CascadeShadowSplitSphereRadii;

float4      _MainLightShadowOffset0; // xy: offset0, zw: offset1
float4      _MainLightShadowOffset1; // xy: offset2, zw: offset3
float4      _MainLightShadowParams;   // (x: shadowStrength, y: >= 1.0 if soft shadows, 0.0 otherwise, z: main light fade scale, w: main light fade bias)
float4      _MainLightShadowmapSize;  // (xy: 1/width and 1/height, zw: width and height)

float4      _AdditionalShadowOffset0; // xy: offset0, zw: offset1
float4      _AdditionalShadowOffset1; // xy: offset2, zw: offset3
float4      _AdditionalShadowFadeParams; // x: additional light fade scale, y: additional light fade bias, z: 0.0, w: 0.0)
float4      _AdditionalShadowmapSize; // (xy: 1/width and 1/height, zw: width and height)

#if !USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
// Point lights can use 6 shadow slices. Some mobile GPUs performance decrease drastically with uniform
// blocks bigger than 8kb while others have a 64kb max uniform block size. This number ensures size of buffer
// AdditionalLightShadows stays reasonable. It also avoids shader compilation errors on SHADER_API_GLES30
// devices where max number of uniforms per shader GL_MAX_FRAGMENT_UNIFORM_VECTORS is low (224)
float4      _AdditionalShadowParams[MAX_VISIBLE_LIGHTS];         // Per-light data
float4x4    _AdditionalLightsWorldToShadow[MAX_VISIBLE_LIGHTS];  // Per-shadow-slice-data
#endif

#ifndef SHADER_API_GLES3
CBUFFER_END
#endif


float4 _ShadowBias; // x: depth bias, y: normal bias

#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0

// Should match: UnityEngine.Rendering.Universal + 1
#define SOFT_SHADOW_QUALITY_OFF    half(0.0)
#define SOFT_SHADOW_QUALITY_LOW    half(1.0)
#define SOFT_SHADOW_QUALITY_MEDIUM half(2.0)
#define SOFT_SHADOW_QUALITY_HIGH   half(3.0)

struct ShadowSamplingData
{
    half4 shadowOffset0;
    half4 shadowOffset1;
    float4 shadowmapSize;
    half softShadowQuality;
};

ShadowSamplingData GetMainLightShadowSamplingData()
{
    ShadowSamplingData shadowSamplingData;

    // shadowOffsets are used in SampleShadowmapFiltered for low quality soft shadows.
    shadowSamplingData.shadowOffset0 = _MainLightShadowOffset0;
    shadowSamplingData.shadowOffset1 = _MainLightShadowOffset1;

    // shadowmapSize is used in SampleShadowmapFiltered otherwise
    shadowSamplingData.shadowmapSize = _MainLightShadowmapSize;
    shadowSamplingData.softShadowQuality = _MainLightShadowParams.y;

    return shadowSamplingData;
}

ShadowSamplingData GetAdditionalLightShadowSamplingData(int index)
{
    ShadowSamplingData shadowSamplingData = (ShadowSamplingData)0;

        // shadowOffsets are used in SampleShadowmapFiltered for low quality soft shadows.
        shadowSamplingData.shadowOffset0 = _AdditionalShadowOffset0;
        shadowSamplingData.shadowOffset1 = _AdditionalShadowOffset1;

        // shadowmapSize is used in SampleShadowmapFiltered otherwise.
        shadowSamplingData.shadowmapSize = _AdditionalShadowmapSize;
        shadowSamplingData.softShadowQuality = _AdditionalShadowParams[index].y;

    return shadowSamplingData;
}

// ShadowParams
// x: ShadowStrength
// y: 1.0 if shadow is soft, 0.0 otherwise
half4 GetMainLightShadowParams()
{
    return _MainLightShadowParams;
}


// ShadowParams
// x: ShadowStrength
// y: >= 1.0 if shadow is soft, 0.0 otherwise. Higher value for higher quality. (1.0 == low, 2.0 == medium, 3.0 == high)
// z: 1.0 if cast by a point light (6 shadow slices), 0.0 if cast by a spot light (1 shadow slice)
// w: first shadow slice index for this light, there can be 6 in case of point lights. (-1 for non-shadow-casting-lights)
half4 GetAdditionalLightShadowParams(int lightIndex)
{
            return _AdditionalShadowParams[lightIndex];
    
}

half4 SampleScreenSpaceShadowmap(float4 shadowCoord, float3 offset, float3 color)
{
    shadowCoord.xy /= max(0.00001, shadowCoord.w); // Prevent division by zero.

    // The stereo transform has to happen after the manual perspective divide
    shadowCoord.xy = UnityStereoTransformScreenSpaceTex(shadowCoord.xy);

    half4 attenuation = half4(
        SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_PointClamp, shadowCoord.xy+offset.x).x*color.r,
        SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_PointClamp, shadowCoord.xy+offset.y).y*color.g,
        SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_PointClamp, shadowCoord.xy+offset.z).z*color.b,
        SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_PointClamp, shadowCoord.xy).w
        );

    return attenuation;
}

real4 SampleShadowmapFilteredLowQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData, float3 offset, float3 color)
{
    // 4-tap hardware comparison

    half paramX=offset.x;
    half paramY=offset.y;
    half paramZ=offset.z;
    real3 shadowColor = color;

    real4 attenuation4[4];
    real4 attenuation4Result;

    real4 acc[4];
    [loop]
    for(int i = 0; i< 4; i++)
    {
        acc[i].r = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(samplingData.shadowOffset0.xy , 0)+ paramX)*shadowColor.r);
        acc[i].g = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(samplingData.shadowOffset0.xy , 0)+ paramY)*shadowColor.g);
        acc[i].b = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(samplingData.shadowOffset0.xy , 0)+ paramZ)*shadowColor.b);
        acc[i].a = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(samplingData.shadowOffset0.xy, 0)));
    }

    attenuation4Result = (acc[0] + acc[1] + acc[2] + acc[3]);///4.0;
    // attenuation4Result.rgb*=(1-dot(attenuation4Result.a, real(0.25)));
    return real4(dot(attenuation4Result.r, real(0.25)),dot(attenuation4Result.g, real(0.25)),dot(attenuation4Result.b, real(0.25)), dot(attenuation4Result.a, real(0.25)));
}

real4 SampleShadowmapFilteredMediumQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData, float3 offset, float3 color)
{
    real fetchesWeights[9];
    real2 fetchesUV[9];
    SampleShadow_ComputeSamples_Tent_5x5(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    half paramX=offset.x;//*samplingData.shadowmapSize.zw;
    half paramY=offset.y;//*samplingData.shadowmapSize.zw;
    half paramZ=offset.z;//*samplingData.shadowmapSize.zw;
    real3 shadowColor = color;
//    return SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[0].xy-paramX, shadowCoord.z))*float3(1,0,0);

    real4 acc[9];
    [loop]
    for(int i = 0; i< 9; i++)
    {
        acc[i].r = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy+paramX, shadowCoord.z))*color;
        acc[i].g = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy+paramY, shadowCoord.z))*color;
        acc[i].b = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy+paramZ, shadowCoord.z))*color;
        acc[i].a = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy, shadowCoord.z));
    }

    real4 result = (acc[0]+acc[1]+acc[2]+acc[3]+acc[4]+acc[5]+acc[6]+acc[7]+acc[8]);

    return result;
}

real4 SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData, float3 offset, float3 color)
{
    real fetchesWeights[16];
    real2 fetchesUV[16];

    half paramX=offset.x;
    half paramY=offset.y;
    half paramZ=offset.z;

    SampleShadow_ComputeSamples_Tent_7x7(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    real4 acc[16];

    [loop]
    for(int i = 0; i< 16; i++)
    {
        acc[i].r = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy + offset.x, shadowCoord.z)).r*color.r;
        acc[i].g = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy + offset.y, shadowCoord.z)).r*color.g;
        acc[i].b = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy + offset.z, shadowCoord.z)).r*color.b;
        acc[i].a = fetchesWeights[i] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[i].xy, shadowCoord.z));
    }

    real4 result = (acc[0]+acc[1]+acc[2]+acc[3]+acc[4]+acc[5]+acc[6]+acc[7]+acc[8]+acc[9]+acc[10]+acc[11]+acc[12]+acc[13]+acc[14]+acc[15]);
    
    return result;
}

real4 SampleShadowmapFiltered(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData, float3 offset, float3 color)
{
    // real attenuation = real(1.0);
    real4 attenuation = real4(1.0, 0.0, 0.0, 1.0);

    if (samplingData.softShadowQuality == SOFT_SHADOW_QUALITY_LOW)
    {
        attenuation = SampleShadowmapFilteredLowQuality(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
    }
    else if(samplingData.softShadowQuality == SOFT_SHADOW_QUALITY_MEDIUM)
    {
        attenuation = SampleShadowmapFilteredMediumQuality(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
    }
    else // SOFT_SHADOW_QUALITY_HIGH
    {
        attenuation = SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
    }
    return attenuation;
}

half4 SampleShadowmap(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData, half4 shadowParams, float3 offset, float3 color, bool isPerspectiveProjection = true)
{
    // Compiler will optimize this branch away as long as isPerspectiveProjection is known at compile time
    if (isPerspectiveProjection)
        shadowCoord.xyz /= shadowCoord.w;

    half4 attenuation;
    real shadowStrength = shadowParams.x;

    // Quality levels are only for platforms requiring strict static branches
    #if defined(_SHADOWS_SOFT_LOW)
        attenuation = SampleShadowmapFilteredLowQuality(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
    #elif defined(_SHADOWS_SOFT_MEDIUM)
        attenuation = SampleShadowmapFilteredMediumQuality(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
    #elif defined(_SHADOWS_SOFT_HIGH)
        attenuation = SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
    #elif defined(_SHADOWS_SOFT)
        if (shadowParams.y > SOFT_SHADOW_QUALITY_OFF)
        {
            attenuation = SampleShadowmapFiltered(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, offset, color);
        }
        else
        {
            attenuation.r = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz+offset.r).r*color.r);
            attenuation.g = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz+offset.g).r*color.g);
            attenuation.b = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz+offset.b).r*color.b);
            attenuation.a = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz));
        }
    #else
            attenuation.r = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz+offset.r).r*color.r);
            attenuation.g = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz+offset.g).r*color.g);
            attenuation.b = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz+offset.b).r*color.b);
            attenuation.a = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz));
    #endif

    attenuation.a = LerpWhiteTo(attenuation.a, shadowStrength);
    // if (!isPerspectiveProjection)
    //     attenuation.rgb-=attenuation.a;
    // Shadow coords that fall out of the light frustum volume must always return attenuation 1.0
    // TODO: We could use branch here to save some perf on some platforms.
    // return attenuation;
    return BEYOND_SHADOW_FAR(shadowCoord) ? half4(1.0, 1.0, 1.0, 1.0) : attenuation;
    // return attenuation;
}

half ComputeCascadeIndex(float3 positionWS)
{
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);

    return half(4.0) - dot(weights, half4(4, 3, 2, 1));
}

float4 TransformWorldToShadowCoord(float3 positionWS)
{
    half cascadeIndex = half(0.0);

    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));

    return float4(shadowCoord.xyz, 0);
}

half4 MainLightRealtimeShadow(float4 shadowCoord, float3 offset, float3 color)
{
        return half(1.0);
}

// returns 0.0 if position is in light's shadow
// returns 1.0 if position is in light
half4 AdditionalLightRealtimeShadow(int lightIndex, float3 positionWS, half3 lightDirection, float3 offset, float3 color)
{
        ShadowSamplingData shadowSamplingData = GetAdditionalLightShadowSamplingData(lightIndex);

        half4 shadowParams = GetAdditionalLightShadowParams(lightIndex);

        int shadowSliceIndex = shadowParams.w;
        if (shadowSliceIndex < 0)
            return half4(1,1,1,1);


            float4 shadowCoord = mul(_AdditionalLightsWorldToShadow[shadowSliceIndex], float4(positionWS, 1.0));

        return SampleShadowmap(TEXTURE2D_ARGS(_AdditionalLightsShadowmapTexture, sampler_LinearClampCompare), shadowCoord, shadowSamplingData, shadowParams, offset, color, true);
    
}

half GetMainLightShadowFade(float3 positionWS)
{
    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

    float fade = saturate(distanceCamToPixel2 * float(_MainLightShadowParams.z) + float(_MainLightShadowParams.w));
    return half(fade);
}

half GetAdditionalLightShadowFade(float3 positionWS)
{
        float3 camToPixel = positionWS - _WorldSpaceCameraPos;
        float distanceCamToPixel2 = dot(camToPixel, camToPixel);

        float fade = saturate(distanceCamToPixel2 * float(_AdditionalShadowFadeParams.x) + float(_AdditionalShadowFadeParams.y));
        return half(fade);
    
}

half MixRealtimeAndBakedShadows(half realtimeShadow, half bakedShadow, half shadowFade)
{
    return lerp(realtimeShadow, bakedShadow, shadowFade);
}

half BakedShadow(half4 shadowMask, half4 occlusionProbeChannels, float3 offset, float3 color)
{
    // Here occlusionProbeChannels used as mask selector to select shadows in shadowMask
    // If occlusionProbeChannels all components are zero we use default baked shadow value 1.0
    // This code is optimized for mobile platforms:
    // half bakedShadow = any(occlusionProbeChannels) ? dot(shadowMask, occlusionProbeChannels) : 1.0h;
    half bakedShadow = half(1.0) + dot(shadowMask - half(1.0), occlusionProbeChannels);
    half shadowStrength = 1.0h - bakedShadow;

    half3 chroma;
    chroma.r = bakedShadow + shadowStrength * offset.x*color.r;
    chroma.g = bakedShadow + shadowStrength * offset.y*color.g;
    chroma.b = bakedShadow + shadowStrength * offset.z*color.b;

return chroma;
    // return bakedShadow;
}

half4 MainLightShadow(float4 shadowCoord, float3 positionWS, half4 shadowMask, half4 occlusionProbeChannels, float3 offset, float3 color)
{
    half4 realtimeShadow = MainLightRealtimeShadow(shadowCoord, offset, color);

    half bakedShadow = 0;

    half shadowFade = 0;

    return half4(realtimeShadow.rgb, MixRealtimeAndBakedShadows(realtimeShadow.a, bakedShadow, shadowFade));
}

half4 AdditionalLightShadow(int lightIndex, float3 positionWS, half3 lightDirection, half4 shadowMask, half4 occlusionProbeChannels, float3 offset, float3 color)
{
    half4 realtimeShadow = AdditionalLightRealtimeShadow(lightIndex, positionWS, lightDirection, offset, color);

    half bakedShadow = half(1.0);

    //UNCOMMENT FOR ACTUAL CORRECT SHADOW
//#ifdef ADDITIONAL_LIGHT_CALCULATE_SHADOWS
//    half shadowFade = GetAdditionalLightShadowFade(positionWS);
//#else
    half shadowFade = half(1.0);
//#endif
// TODO: Fix return
    return half4(realtimeShadow.rgb, MixRealtimeAndBakedShadows(realtimeShadow.a, bakedShadow, shadowFade));
}

float4 GetShadowCoord(VertexPositionInputs vertexInput)
{
    return TransformWorldToShadowCoord(vertexInput.positionWS);
}

float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * _ShadowBias.y;

    // normal bias is negative since we want to apply an inset normal offset
    positionWS = lightDirection * _ShadowBias.xxx + positionWS;
    positionWS = normalWS * scale.xxx + positionWS;
    return positionWS;
}

///////////////////////////////////////////////////////////////////////////////
// Deprecated                                                                 /
///////////////////////////////////////////////////////////////////////////////

// Renamed -> _MainLightShadowParams
#define _MainLightShadowData _MainLightShadowParams

// Deprecated: Use GetMainLightShadowFade or GetAdditionalLightShadowFade instead.
float GetShadowFade(float3 positionWS)
{
    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

    float fade = saturate(distanceCamToPixel2 * float(_MainLightShadowParams.z) + float(_MainLightShadowParams.w));
    return fade * fade;
}

// Deprecated: Use GetShadowFade instead.
float ApplyShadowFade(float shadowAttenuation, float3 positionWS)
{
    float fade = GetShadowFade(positionWS);
    return shadowAttenuation + (1 - shadowAttenuation) * fade * fade;
}

// Deprecated: Use GetMainLightShadowParams instead.
half GetMainLightShadowStrength()
{
    return _MainLightShadowData.x;
}

// Deprecated: Use GetAdditionalLightShadowParams instead.
half GetAdditionalLightShadowStrenth(int lightIndex)
{
        return half(1.0);
}

// Deprecated: Use SampleShadowmap that takes shadowParams instead of strength.
real4 SampleShadowmap(float4 shadowCoord, TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), ShadowSamplingData samplingData, half shadowStrength, half3 offset, half3 color, bool isPerspectiveProjection = true)
{
    half4 shadowParams = half4(shadowStrength, 1.0, 0.0, 0.0);
    return SampleShadowmap(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, shadowParams, offset, color, isPerspectiveProjection);
}

// Deprecated: Use AdditionalLightRealtimeShadow(int lightIndex, float3 positionWS, half3 lightDirection) in Shadows.hlsl instead, as it supports Point Light shadows
half4 AdditionalLightRealtimeShadow(int lightIndex, float3 positionWS, float3 offset, float3 color)
{
    return AdditionalLightRealtimeShadow(lightIndex, positionWS, half3(1, 0, 0), offset, color);
}

#endif
