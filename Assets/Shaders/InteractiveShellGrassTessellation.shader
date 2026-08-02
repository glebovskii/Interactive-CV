Shader "Custom/URP/InteractiveShellGrassTessellation"
{
    Properties
    {
        [Header(Appearance)]
        _ShellColor("Shell Color", Color) = (0.15, 0.65, 0.12, 1)
        _Density("Grass Density", Range(1, 500)) = 100
        _Thickness("Strand Thickness", Range(0.001, 1)) = 0.25
        _NoiseMin("Minimum Strand Height", Range(0, 1)) = 0.2
        _NoiseMax("Maximum Strand Height", Range(0, 1)) = 1

        [Header(Shell)]
        _ShellIndex("Shell Index", Float) = 0
        _ShellCount("Shell Count", Float) = 16
        _ShellLength("Shell Length", Range(0, 2)) = 0.25
        _ShellHeightAttenuation("Shell Height Attenuation", Range(0.01, 8)) = 1
        _Curvature("Curvature", Range(0.01, 8)) = 2
        _ShellDirection("Base Shell Direction", Vector) = (0, -1, 0, 0)
        _DisplacementStrength("Base Direction Strength", Range(0, 2)) = 0.05
        [Toggle] _KeepBaseSolid("Keep Base Shell Solid", Float) = 1

        [Header(Simple Lighting)]
        _OcclusionAttenuation("Occlusion Attenuation", Range(0.01, 8)) = 1
        _OcclusionBias("Occlusion Bias", Range(0, 1)) = 0.25
        _MainLightColorInfluence("Main Light Color Influence", Range(0, 1)) = 1

        [Header(Interaction Map)]
        [NoScaleOffset] _InteractionMap("Interaction Map", 2D) = "black" {}
        _TrackBendStrength("Track Bend Strength", Range(0, 2)) = 0.25
        _TrackFlattenStrength("Track Flatten Strength", Range(0, 2)) = 0.05
        [Toggle] _BendOppositeMovement("Bend Opposite Movement", Float) = 0

        [Header(Current Player Bend)]
        _PlayerBendRadius("Player Bend Radius", Range(0.01, 10)) = 1
        _PlayerBendStrength("Player Bend Strength", Range(0, 2)) = 0.25
        _PlayerFlattenStrength("Player Flatten Strength", Range(0, 2)) = 0.1

        [Header(Tessellation)]
        _TessellationMin("Minimum Tessellation", Range(1, 8)) = 1
        _TessellationMax("Maximum Tessellation", Range(1, 32)) = 8
        _PlayerTessellationRadius("Player Tessellation Radius", Range(0.01, 20)) = 3
        _TrailTessellationDistance("Trail Tessellation Distance", Range(0.01, 50)) = 12
        _TessellationCameraDistance("Tessellation Camera Distance", Range(1, 100)) = 30
        _TrailTessellationInfluence("Trail Tessellation Influence", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
        }

        Pass
        {
            Name "ForwardUnlit"

            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 4.6
            #pragma require tessellation

            // Hull/domain shaders are not available on GLES/WebGL.
            #pragma exclude_renderers gles gles3 metal

            #pragma vertex TessellationVertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Fragment

            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_InteractionMap);
            SAMPLER(sampler_InteractionMap);

            // Set globally from C#.
            float4 _PlayerPositionWS;
            float _PlayerPositionValid;

            CBUFFER_START(UnityPerMaterial)
                float4 _ShellColor;

                float _Density;
                float _Thickness;
                float _NoiseMin;
                float _NoiseMax;

                float _ShellIndex;
                float _ShellCount;
                float _ShellLength;
                float _ShellHeightAttenuation;
                float _Curvature;
                float4 _ShellDirection;
                float _DisplacementStrength;
                float _KeepBaseSolid;

                float _OcclusionAttenuation;
                float _OcclusionBias;
                float _MainLightColorInfluence;

                float _TrackBendStrength;
                float _TrackFlattenStrength;
                float _BendOppositeMovement;

                float _PlayerBendRadius;
                float _PlayerBendStrength;
                float _PlayerFlattenStrength;

                float _TessellationMin;
                float _TessellationMax;
                float _PlayerTessellationRadius;
                float _TrailTessellationDistance;
                float _TessellationCameraDistance;
                float _TrailTessellationInfluence;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct ControlPoint
            {
                float3 positionOS : INTERNALTESSPOS;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            float Hashed(uint n)
            {
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;

                return float(n & 0x7FFFFFFFU) / float(0x7FFFFFFF);
            }

            float RadialFalloff(float distanceValue, float radius)
            {
                radius = max(radius, 0.0001);

                return 1.0 - smoothstep(radius * 0.7, radius, distanceValue);
            }

            float4 SampleInteraction(float2 uv)
            {
                return SAMPLE_TEXTURE2D_LOD(
                    _InteractionMap,
                    sampler_InteractionMap,
                    saturate(uv),
                    0);
            }

            float GetPlayerDistance(float3 positionWS)
            {
                return distance(positionWS.xz, _PlayerPositionWS.xz);
            }

            float GetPlayerInfluence(float3 positionWS, float radius)
            {
                if (_PlayerPositionValid < 0.5)
                    return 0.0;

                return RadialFalloff(GetPlayerDistance(positionWS), radius);
            }

            float GetCameraTessellationFade(float3 positionWS)
            {
                float maxDistance = max(_TessellationCameraDistance, 0.001);
                float cameraDistance = distance(positionWS, _WorldSpaceCameraPos);

                return 1.0 - smoothstep(
                    maxDistance * 0.8,
                    maxDistance,
                    cameraDistance);
            }

            float GetTrailDistanceFade(float3 positionWS)
            {
                if (_PlayerPositionValid < 0.5)
                    return 1.0;

                float maxDistance = max(_TrailTessellationDistance, 0.001);
                float playerDistance = GetPlayerDistance(positionWS);

                return 1.0 - smoothstep(
                    maxDistance * 0.8,
                    maxDistance,
                    playerDistance);
            }

            float GetTessellationLevel(float3 positionOS, float2 uv)
            {
                return 8.0;
                float3 positionWS = TransformObjectToWorld(positionOS);

                float interactionMask = saturate(SampleInteraction(uv).r);

                float playerInfluence = GetPlayerInfluence(
                    positionWS,
                    _PlayerTessellationRadius);

                float trailInfluence =
                    interactionMask *
                    GetTrailDistanceFade(positionWS) *
                    _TrailTessellationInfluence;

                float cameraFade = GetCameraTessellationFade(positionWS);

                float influence = max(playerInfluence, trailInfluence);
                influence *= cameraFade;

                float minimumLevel = max(_TessellationMin, 1.0);
                float maximumLevel = max(_TessellationMax, minimumLevel);

                return lerp(minimumLevel, maximumLevel, saturate(influence));
            }

            ControlPoint TessellationVertex(Attributes input)
            {
                ControlPoint output;

                output.positionOS = input.positionOS;
                output.normalOS = input.normalOS;
                output.uv = input.uv;

                return output;
            }

            TessellationFactors PatchConstantFunction(
                InputPatch<ControlPoint, 3> patch)
            {
                TessellationFactors factors;

                float t0 = GetTessellationLevel(
                    patch[0].positionOS,
                    patch[0].uv);

                float t1 = GetTessellationLevel(
                    patch[1].positionOS,
                    patch[1].uv);

                float t2 = GetTessellationLevel(
                    patch[2].positionOS,
                    patch[2].uv);

                float3 position12 =
                    (patch[1].positionOS + patch[2].positionOS) * 0.5;

                float3 position20 =
                    (patch[2].positionOS + patch[0].positionOS) * 0.5;

                float3 position01 =
                    (patch[0].positionOS + patch[1].positionOS) * 0.5;

                float2 uv12 = (patch[1].uv + patch[2].uv) * 0.5;
                float2 uv20 = (patch[2].uv + patch[0].uv) * 0.5;
                float2 uv01 = (patch[0].uv + patch[1].uv) * 0.5;

                float edge12 = GetTessellationLevel(position12, uv12);
                float edge20 = GetTessellationLevel(position20, uv20);
                float edge01 = GetTessellationLevel(position01, uv01);

                // Edge 0 is opposite control point 0, so it connects 1 and 2.
                factors.edge[0] = max(edge12, max(t1, t2));
                factors.edge[1] = max(edge20, max(t2, t0));
                factors.edge[2] = max(edge01, max(t0, t1));

                float3 centerPosition =
                    (patch[0].positionOS +
                     patch[1].positionOS +
                     patch[2].positionOS) / 3.0;

                float2 centerUV =
                    (patch[0].uv +
                     patch[1].uv +
                     patch[2].uv) / 3.0;

                float centerLevel = GetTessellationLevel(
                    centerPosition,
                    centerUV);

                factors.inside = max(
                    centerLevel,
                    (factors.edge[0] +
                     factors.edge[1] +
                     factors.edge[2]) / 3.0);

                return factors;
            }

            [domain("tri")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [outputcontrolpoints(3)]
            [patchconstantfunc("PatchConstantFunction")]
            ControlPoint Hull(
                InputPatch<ControlPoint, 3> patch,
                uint controlPointID : SV_OutputControlPointID)
            {
                return patch[controlPointID];
            }

            float3 SafeNormalize3(float3 value)
            {
                float lengthSquared = dot(value, value);

                if (lengthSquared < 0.000001)
                    return 0.0;

                return value * rsqrt(lengthSquared);
            }

            float2 SafeNormalize2(float2 value)
            {
                float lengthSquared = dot(value, value);

                if (lengthSquared < 0.000001)
                    return 0.0;

                return value * rsqrt(lengthSquared);
            }

            void ApplyShellDisplacement(
                inout float3 positionOS,
                float3 normalOS,
                float2 uv)
            {
                float shellCount = max(_ShellCount, 1.0);

                float rawShellHeight = saturate(
                    _ShellIndex / shellCount);

                float shellHeight = pow(
                    rawShellHeight,
                    max(_ShellHeightAttenuation, 0.001));

                float shellCurve = pow(
                    shellHeight,
                    max(_Curvature, 0.001));

                float3 basePositionWS =
                    TransformObjectToWorld(positionOS);

                // Normal shell extrusion.
                positionOS +=
                    normalOS *
                    _ShellLength *
                    shellHeight;

                // Constant base direction, such as wind or gravity.
                positionOS +=
                    _ShellDirection.xyz *
                    shellCurve *
                    _DisplacementStrength;

                float4 interaction = SampleInteraction(uv);
                float interactionStrength = 0;//saturate(interaction.r);

                // Interaction map layout:
                // R = strength
                // G = signed world X direction
                // B = signed world Z direction
                float2 trackDirectionXZ = SafeNormalize2(interaction.gb);

                float directionSign = lerp(
                    1.0,
                    -1.0,
                    saturate(_BendOppositeMovement));

                trackDirectionXZ *= directionSign;

                float3 trackDirectionWS = float3(
                    trackDirectionXZ.x,
                    0.0,
                    trackDirectionXZ.y);

                float3 trackDirectionOS = TransformWorldToObjectDir(
                    trackDirectionWS,
                    true);

                // Bend along the stored movement direction.
                positionOS +=
                    trackDirectionOS *
                    interactionStrength *
                    shellCurve *
                    _TrackBendStrength;

                // Compress toward the ground inside old tracks.
                positionOS -=
                    normalOS *
                    interactionStrength *
                    shellCurve *
                    _TrackFlattenStrength;

                float playerInfluence = GetPlayerInfluence(
                    basePositionWS,
                    _PlayerBendRadius);

                float2 awayFromPlayerXZ =
                    basePositionWS.xz -
                    _PlayerPositionWS.xz;

                awayFromPlayerXZ = SafeNormalize2(awayFromPlayerXZ);

                float3 awayFromPlayerWS = float3(
                    awayFromPlayerXZ.x,
                    0.0,
                    awayFromPlayerXZ.y);

                float3 awayFromPlayerOS = TransformWorldToObjectDir(
                    awayFromPlayerWS,
                    true);

                // Radial bending around the current player.
                positionOS +=
                    awayFromPlayerOS *
                    playerInfluence *
                    shellCurve *
                    _PlayerBendStrength;

                positionOS -=
                    normalOS *
                    playerInfluence *
                    shellCurve *
                    _PlayerFlattenStrength;
            }

            [domain("tri")]
            Varyings Domain(
                TessellationFactors factors,
                OutputPatch<ControlPoint, 3> patch,
                float3 barycentricCoordinates : SV_DomainLocation)
            {
                Varyings output;

                float3 positionOS =
                    patch[0].positionOS * barycentricCoordinates.x +
                    patch[1].positionOS * barycentricCoordinates.y +
                    patch[2].positionOS * barycentricCoordinates.z;

                float3 normalOS =
                    patch[0].normalOS * barycentricCoordinates.x +
                    patch[1].normalOS * barycentricCoordinates.y +
                    patch[2].normalOS * barycentricCoordinates.z;

                float2 uv =
                    patch[0].uv * barycentricCoordinates.x +
                    patch[1].uv * barycentricCoordinates.y +
                    patch[2].uv * barycentricCoordinates.z;

                normalOS = SafeNormalize3(normalOS);

                ApplyShellDisplacement(
                    positionOS,
                    normalOS,
                    uv);

                float3 positionWS =
                    TransformObjectToWorld(positionOS);

                output.positionHCS =
                    TransformWorldToHClip(positionWS);

                output.uv = uv;
                output.normalWS =
                    TransformObjectToWorldNormal(normalOS);

                output.fogFactor =
                    ComputeFogFactor(output.positionHCS.z);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float2 tiledUV = input.uv * _Density;
                float2 localUV = frac(tiledUV) * 2.0 - 1.0;

                float localDistanceFromCenter = length(localUV);

                uint2 tileID = (uint2)floor(tiledUV);
                uint seed =
                    tileID.x +
                    100U * tileID.y +
                    1000U;

                float randomHeight = lerp(
                    _NoiseMin,
                    _NoiseMax,
                    Hashed(seed));

                float shellHeight = saturate(
                    _ShellIndex / max(_ShellCount, 1.0));

                float strandRadius =
                    _Thickness *
                    max(randomHeight - shellHeight, 0.0);

                bool outsideStrand = localDistanceFromCenter > strandRadius;

                if (_KeepBaseSolid > 0.5)
                {
                    if (outsideStrand && _ShellIndex > 0.5)
                        discard;
                }
                else if (outsideStrand)
                {
                    discard;
                }

                float3 normalWS = SafeNormalize3(input.normalWS);

                // Uses the URP main light direction/color, but does not
                // request or evaluate shadow attenuation.
                Light mainLight = GetMainLight();

                float halfLambert =
                    saturate(dot(normalWS, mainLight.direction)) *
                    0.5 +
                    0.5;

                halfLambert *= halfLambert;

                float ambientOcclusion = pow(
                    shellHeight,
                    max(_OcclusionAttenuation, 0.001));

                ambientOcclusion = saturate(
                    ambientOcclusion +
                    _OcclusionBias);

                float3 lightColor = lerp(
                    float3(1.0, 1.0, 1.0),
                    mainLight.color,
                    _MainLightColorInfluence);

                float3 finalColor =
                    _ShellColor.rgb *
                    halfLambert *
                    ambientOcclusion *
                    lightColor;

                finalColor = MixFog(
                    finalColor,
                    input.fogFactor);

                return half4(finalColor, _ShellColor.a);
            }

            ENDHLSL
        }
    }

    Fallback Off
}