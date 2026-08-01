Shader "Hidden/DrawTracks"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Coordinate("Coordinate", Vector) =(0,0,0,0)
        _DrawColor("Draw Color", Color) = (1,0,0,0)
        _Size("Brush size", float)=500
        _BrushStrength("Brush Strength", Range(0,1)) = 0.5
        _RestoreTime("Restore Time", float)=0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Size;
            half _BrushStrength;
            fixed4 _Coordinate, _DrawColor;
            half _RestoreTime;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed draw = pow(saturate(1 - distance(i.uv, _Coordinate.xy)), 5000 / _Size);
                fixed4 drawColor = _DrawColor * (draw * _BrushStrength);
                return saturate(col + drawColor);
            }
            ENDCG
        }
    }
}