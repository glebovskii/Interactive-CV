Shader "Hidden/DrawTracksBatch"
{
    Properties
    {
        _MainTex ("Previous Interaction Map", 2D) = "black" {}
        _Size ("Brush Radius (Pixels)", Float) = 24
        _BrushStrength ("Brush Strength", Range(0, 1)) = 1
        _BrushFalloff ("Brush Falloff", Float) = 1
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            #define MAX_BRUSH_POINTS 32

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Coordinates[MAX_BRUSH_POINTS];
            int _CoordinateCount;
            float _Size;
            float _BrushStrength;
            float _BrushFalloff;

            float4 frag(v2f_img input) : SV_Target
            {
                float value = tex2D(_MainTex, input.uv).r;
                float radius = max(_Size, 0.001);
                float falloff = max(_BrushFalloff, 0.001);

                [loop]
                for (int i = 0; i < _CoordinateCount; i++)
                {
                    float2 deltaPixels = (input.uv - _Coordinates[i].xy) / _MainTex_TexelSize.xy;
                    float normalizedDistance = saturate(1.0 - length(deltaPixels) / radius);
                    float brush = pow(normalizedDistance, falloff) * _BrushStrength;
                    value = max(value, brush);
                }

                return float4(value, value, value, 1.0);
            }
            ENDCG
        }
    }
}
