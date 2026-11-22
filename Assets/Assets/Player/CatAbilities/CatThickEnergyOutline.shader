Shader "Custom/URP/CatEarThickEnergyOutline"
{
    Properties
    {
        [HDR] _GlowColor ("Neon Blue (HDR)", Color) = (0.02, 0.55, 1.0, 1.0)
        _OutlinePixels ("Outline Thickness (pixels)", Range(1, 16)) = 6
        _Emission ("Glow Strength", Range(1, 20)) = 8
        _Flicker ("Energy Flicker", Range(0, 1)) = 0.18
        _NoiseScale ("Energy Pattern Scale", Range(2, 80)) = 24
        _PulseSpeed ("Pulse Speed", Range(0, 8)) = 1.5
        _Opacity ("Opacity", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ThickEnergyOutline"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                float _OutlinePixels;
                float _Emission;
                float _Flicker;
                float _NoiseScale;
                float _PulseSpeed;
                float _Opacity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            float hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 posCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Project the vertex normal into screen space, then expand by a pixel-based width.
                float4 normalPointCS = TransformObjectToHClip(IN.positionOS.xyz + normalize(IN.normalOS) * 0.1);
                float2 p0 = posCS.xy / max(abs(posCS.w), 0.0001);
                float2 p1 = normalPointCS.xy / max(abs(normalPointCS.w), 0.0001);
                float2 dir = p1 - p0;
                dir = (dot(dir, dir) > 0.000001) ? normalize(dir) : float2(0, 1);

                posCS.xy += dir * (_OutlinePixels * 2.0 / _ScreenParams.xy) * posCS.w;
                OUT.positionHCS = posCS;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float n = hash31(floor(IN.positionWS * _NoiseScale + _Time.y * _PulseSpeed));
                float pulse = 1.0 - _Flicker * 0.5 + _Flicker * n;
                float3 col = _GlowColor.rgb * (_Emission * pulse);
                return half4(col, _Opacity * pulse);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
