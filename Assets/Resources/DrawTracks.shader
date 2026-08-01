Shader "Hidden/DrawTracks"
{
    Properties
    {
        _MainTex("Previous Interaction Map", 2D) = "black" {}
        _Coordinate("UV And Direction", Vector) = (0, 0, 0, 0)
        _Size("Brush Radius In Pixels", Float) = 24
        _BrushStrength("Brush Strength", Range(0, 1)) = 1
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
                float4 _Coordinate;
                float _Size;
                float _BrushStrength;
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
                float4 previous = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float2 pixelOffset = (input.uv - _Coordinate.xy) * _MainTex_TexelSize.zw;
                float distancePixels = length(pixelOffset);

                float radius = max(_Size, 1.0);
                float brush = 1.0 - smoothstep(radius * 0.65, radius, distancePixels);
                float stampStrength = saturate(brush * _BrushStrength);

                float2 newDirection = _Coordinate.zw;
                float newDirectionLength = length(newDirection);

                // Do not modify the texture when no valid movement direction
                // was supplied.
                if (newDirectionLength < 0.00001 || stampStrength <= 0.0)
                    return previous;

                newDirection /= newDirectionLength;

                float oldMask = saturate(previous.r);
                float newMask = max(oldMask, stampStrength);

                // Strongly overwrite direction near the center of the newest
                // stamp while preserving previous direction around its edge.
                float directionBlend = stampStrength / max(newMask, 0.00001);
                float2 blendedDirection = lerp(previous.gb, newDirection, directionBlend);

                float blendedLength = length(blendedDirection);

                if (blendedLength > 0.00001)
                    blendedDirection /= blendedLength;
                else
                    blendedDirection = newDirection;

                // Do not saturate G/B. They must preserve negative values.
                return float4(
                    newMask,
                    blendedDirection.x,
                    blendedDirection.y,
                    max(previous.a, stampStrength));
            }
            ENDHLSL
        }
    }
}