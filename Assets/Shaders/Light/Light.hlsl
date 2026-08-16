void AddAdditionalLight_float(in float Smoothness, in float3 WorldPos, in float3 WorldNormal, in float3 WorldView,
                              in float MainDiffuse, in float3 MainSpecular, in float3 MainColor, in float2 ScreenPos,
                              out float Diffuse, out float3 Specular, out float3 Color)
{
    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor * (MainDiffuse + MainSpecular);

    #ifndef SHADERGRAPH_PREVIEW

    uint pixelLightCount = GetAdditionalLightsCount();

    #if defined(_ADDITIONAL_LIGHTS)

    #if USE_CLUSTER_LIGHT_LOOP
    UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);
        light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPos, light.direction);
        float atten = light.distanceAttenuation * light.shadowAttenuation;
    
        float NdotL = saturate(dot(WorldNormal, light.direction));//*0.5 + 0.5);  //HALF LAMBERT
        float thisDiffuse = NdotL * atten;
        float3 thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness);
    
        Diffuse += thisDiffuse;
        Specular += thisSpecular;
    
    #if defined(_LIGHT_COOKIES)
        float3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPos);
        light.color *= cookieColor;
    #endif
    
        Color += light.color * (thisDiffuse + thisSpecular);
    }
    #endif
    LIGHT_LOOP_BEGIN(pixelLightCount)

    #if !USE_FORWARD_PLUS
    lightIndex = GetPerObjectLightIndex(lightIndex);
    #endif
    Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);

    light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPos, light.direction);
    float atten = light.distanceAttenuation * light.shadowAttenuation;
    
    float NdotL = saturate(dot(WorldNormal, light.direction));
    float thisDiffuse = NdotL * atten;
    float3 thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness);
    
    Diffuse += thisDiffuse;
    Specular += thisSpecular;
    
    #if defined(_LIGHT_COOKIES)
        float3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPos);
        light.color *= cookieColor;
    #endif
    
    Color += light.color * (thisDiffuse + thisSpecular);

    LIGHT_LOOP_END

    #endif

    float total = Diffuse + dot(Specular, float3(0.333, 0.333, 0.333));
    Color = total <= 0 ? MainColor : Color / total;

    #endif
}

void AddAdditionalLightColorized_float(in float Smoothness, in float3 WorldPos, in float3 WorldNormal,
                                       in float3 WorldView,
                                       in float MainDiffuse, in float3 MainSpecular, in float3 MainColor,
                                       in float2 ScreenPos,
                                       out float Diffuse, out float3 Specular, out float3 Color, out float Attenuation)
{
    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor * (MainDiffuse + MainSpecular);
    Attenuation = 0;

    #ifndef SHADERGRAPH_PREVIEW

    uint pixelLightCount = GetAdditionalLightsCount();

    #if defined(_ADDITIONAL_LIGHTS)

    #if USE_CLUSTER_LIGHT_LOOP
    UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);
        light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPos, light.direction);
        float atten = light.distanceAttenuation * light.shadowAttenuation;
    
        float NdotL = saturate(dot(WorldNormal, light.direction));//*0.5 + 0.5);  //HALF LAMBERT
        float thisDiffuse = NdotL * atten;
        float3 thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness);
    
        Diffuse += thisDiffuse;
        Specular += thisSpecular;
    
    #if defined(_LIGHT_COOKIES)
        float3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPos);
        light.color *= cookieColor;
    #endif
    
        Color += light.color * (thisDiffuse + thisSpecular);
    }
    #endif
    LIGHT_LOOP_BEGIN(pixelLightCount)

    #if !USE_FORWARD_PLUS
    lightIndex = GetPerObjectLightIndex(lightIndex);
    #endif
    Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);

    light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPos, light.direction);
    float atten = light.distanceAttenuation * light.shadowAttenuation;
    
    float NdotL = saturate(dot(WorldNormal, light.direction));
    float thisDiffuse = NdotL * atten;
    float3 thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness);
    
    Diffuse += thisDiffuse;
    Specular += thisSpecular;
    
    #if defined(_LIGHT_COOKIES)
        float3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPos);
        light.color *= cookieColor;
    #endif
    
    Color += light.color * (thisDiffuse + thisSpecular);
    Attenuation += atten;
    LIGHT_LOOP_END

    #endif

    float total = Diffuse + dot(Specular, float3(0.333, 0.333, 0.333));
    Color = total <= 0 ? MainColor : Color / total;

    #endif
}

void AddAdditionalLightBasic_float(in float3 WorldPos, in float3 WorldNormal, in float3 WorldView,
                                   in float MainDiffuse, in float3 MainColor, in float2 ScreenPos,
                                   out float Diffuse, out float3 Color)
{
    Diffuse = MainDiffuse;
    Color = MainColor * MainDiffuse;

    #ifndef SHADERGRAPH_PREVIEW

    uint pixelLightCount = GetAdditionalLightsCount();


    LIGHT_LOOP_BEGIN(pixelLightCount)
    // Convert the pixel light index to the light data index
    #if !USE_CLUSTER_LIGHT_LOOP
    lightIndex = GetPerObjectLightIndex(lightIndex);
    #endif
    Light light = GetAdditionalPerObjectLight(lightIndex, WorldPos);
    float NdotL = saturate(dot(WorldNormal, light.direction));
    float thisDiffuse = light.distanceAttenuation * NdotL;
    Diffuse += thisDiffuse;
    Color += light.color * thisDiffuse;
    LIGHT_LOOP_END
    float total = Diffuse;
    Color = total != 0 ? MainColor : Color / total;
    #endif
}

void CalculateSSAO_float(in float2 ScreenPos, out float DirectAO, out float IndirectAO)
{
    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(SHADERGRAPH_PREVIEW) && !defined(_SURFACE_TYPE_TRANSPARENT)
        float ssao = saturate(SampleAmbientOcclusion(ScreenPos) + (1.0 - _AmbientOcclusionParam.x));
        IndirectAO = ssao;
        DirectAO = lerp(1.0, ssao, _AmbientOcclusionParam.w);
    #else
    DirectAO = 1.0;
    IndirectAO = 1.0;
    #endif
}

void SampleReflectionProbes_float(in float3 positionWS, in float3 reflectVector, in float2 ScreenPos,
                                  in float roughness, in float occlusion, out float3 reflection)
{
    reflection = 0;
    
}


void AddAdditionalLightURP_float(in float Smoothness, in float3 WorldPosition, in float3 WorldNormal, in float3 WorldView,
                                 in float MainDiffuse, in float3 MainSpecular, in float3 MainColor, in float2 ScreenPos,
                                 in float Reflectance,
                                 out float Diffuse, out float3 Specular, out float3 Color)
{
    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor * (MainDiffuse + MainSpecular);

    #ifndef SHADERGRAPH_PREVIEW

    uint pixelLightCount = GetAdditionalLightsCount();
    half Roughness = pow(1 - Smoothness, 2);
    half Roughness2 = Roughness * Roughness;
    half Roughness2Minus1 = Roughness2 - 1;
    half normalizationTerm = (Roughness * half(4.0) + half(2.0));


    LIGHT_LOOP_BEGIN(pixelLightCount)
    // Convert the pixel light index to the light data index
    #if !USE_CLUSTER_LIGHT_LOOP
    lightIndex = GetPerObjectLightIndex(lightIndex);
    #endif
    // Call the URP additional light algorithm. This will not calculate shadows, since we don't pass a shadow mask value
    Light light = GetAdditionalPerObjectLight(lightIndex, WorldPosition);
    // Manually set the shadow attenuation by calculating realtime shadows
    light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPosition, light.direction);
    #if defined(_LIGHT_COOKIES)
            float3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPosition);
            light.color *= cookieColor;
    #endif
    float NdotL = saturate(dot(WorldNormal, light.direction));
    float atten = light.distanceAttenuation * light.shadowAttenuation;
    float thisDiffuse = NdotL * atten;
    //DirectBRDFSpecular

    float3 lightDirectionWSFloat3 = float3(light.direction);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(WorldView));
    float NoH = saturate(dot(float3(WorldNormal), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    float d = NoH * NoH * Roughness2Minus1 + 1.00001f;
    half LoH2 = LoH * LoH;
    half spec = Roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);
    #if REAL_IS_HALF
            spec = spec - HALF_MIN;
            spec = clamp(spec, 0.0, 1000.0);
    #endif
    float3 thisSpecular = spec * Reflectance * NdotL * atten;

    Diffuse += thisDiffuse;
    Specular += thisSpecular;

    Color += light.color * (thisDiffuse + thisSpecular);
    LIGHT_LOOP_END
    float total = Diffuse + dot(Specular, float3(0.333, 0.333, 0.333));
    Color = total <= 0 ? MainColor : Color / total;
    #endif
}
