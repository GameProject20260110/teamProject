Shader "Custom/CircleHole"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0
        _EdgeSoftness ("Edge Softness", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Center;
            float _Radius;
            float _EdgeSoftness;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.uv - _Center.xy;
                uv.x *= _ScreenParams.x / _ScreenParams.y;
                float dist = length(uv);
                float alpha = smoothstep(
                    _Radius - _EdgeSoftness,
                    _Radius + _EdgeSoftness,
                    dist
                );
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}