Shader "Custom/FrostedGlassBoard"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Glass Fog Base)]
        _FrostColor ("Frost Color", Color) = (0.9, 0.95, 1.0, 1.0)
        _BaseAlpha ("Base Fog Alpha", Range(0,1)) = 0.18
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _NoiseTiling ("Noise Tiling (X,Y)", Vector) = (2, 2, 0, 0)
        _ScrollSpeed ("Noise Scroll Speed (X,Y)", Vector) = (0.008, 0.004, 0, 0)

        [Header(Glass Sheen)]
        _SheenColor ("Sheen Color", Color) = (1, 1, 1, 1)
        _SheenAngleDeg ("Sheen Angle (deg)", Range(0, 180)) = 35
        _SheenWidth ("Sheen Width", Range(0.02, 0.5)) = 0.12
        _SheenSpeed ("Sheen Speed", Range(0, 1)) = 0.12
        _SheenStrength ("Sheen Strength", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "GlassFrost"
            Tags { "LightMode"="Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _FrostColor;
                half _BaseAlpha;
                float4 _NoiseTiling;
                float4 _ScrollSpeed;

                half4 _SheenColor;
                half _SheenAngleDeg;
                half _SheenWidth;
                half _SheenSpeed;
                half _SheenStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 fogUV = IN.uv * _NoiseTiling.xy + _Time.y * _ScrollSpeed.xy;
                half fogNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, fogUV).r;
                half fog = lerp(0.7, 1.0, fogNoise);

                float angleRad = radians(_SheenAngleDeg);
                float2 sheenDir = float2(cos(angleRad), sin(angleRad));
                float coord = dot(IN.uv - 0.5, sheenDir);
                float sweepPos = frac(_Time.y * _SheenSpeed) * 3.0 - 1.0;
                half sheen = smoothstep(_SheenWidth, 0.0, abs(coord - sweepPos)) * _SheenStrength;

                half alpha = saturate(_BaseAlpha * fog + sheen) * _FrostColor.a * IN.color.a;
                half3 color = (_FrostColor.rgb + sheen * _SheenColor.rgb) * IN.color.rgb;

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}