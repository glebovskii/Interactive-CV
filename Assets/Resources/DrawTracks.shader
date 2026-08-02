Shader "Hidden/DrawTracks"
{
    Properties
    {
        _MainTex("Previous Interaction Map", 2D) = "black" {}
        _Coordinate("Brush UV", Vector) = (0, 0, 0, 0)
        _Size("Brush Radius In Pixels", Float) = 24
        _BrushStrength("Brush Strength", Range(0, 1)) = 1
        _BrushFalloff("Brush Falloff", Range(0.1, 8)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "DrawTracks"

            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                // XY contains the brush position in texture UV space.
                // ZW previously contained movement direction, but is unused now.
                float4 _Coordinate;

                float _Size;
                float _BrushStrength;
                float _BrushFalloff;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;

                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float4 previous = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv);

                // Convert the UV distance from the brush center into pixels.
                float2 pixelOffset =
                    (input.uv - _Coordinate.xy) *
                    _MainTex_TexelSize.zw;

                float distancePixels = length(pixelOffset);
                float radius = max(_Size, 1.0);

                // 1 at the brush center, gradually falling to 0 at its edge.
                float normalizedDistance = saturate(distancePixels / radius);
                float brush = 1.0 - normalizedDistance;

                // Controls the shape of the radial falloff.
                //
                // Falloff < 1: broad and soft
                // Falloff = 1: linear
                // Falloff > 1: concentrated near the center
                brush = pow(brush, max(_BrushFalloff, 0.0001));

                float stampStrength = saturate(brush * _BrushStrength);

                float oldMask = saturate(previous.r);

                // Preserve the strongest value already written.
                float newMask = max(oldMask, stampStrength);

                /*
                Direction storage is disabled for now.

                float2 newDirection = _Coordinate.zw;
                float newDirectionLength = length(newDirection);

                if (newDirectionLength > 0.00001)
                    newDirection /= newDirectionLength;

                float directionBlend =
                    stampStrength / max(newMask, 0.00001);

                float2 blendedDirection = lerp(
                    previous.gb,
                    newDirection,
                    directionBlend);
                */

                // R = interaction strength.
                // G/B = unused.
                // A = duplicate interaction strength.
                return float4(newMask, 0.0, 0.0, newMask);
            }
            ENDHLSL
        }
    }
}