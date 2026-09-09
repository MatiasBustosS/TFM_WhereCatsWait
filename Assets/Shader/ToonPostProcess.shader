Shader "Custom/ToonPostProcess"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _ColorLevels ("Color Quantization Levels", Range(2, 128)) = 5
        _EdgeThreshold ("Edge Threshold", Range(0.001, 0.1)) = 0.01
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off ZTest Always

        Pass
        {
            Name "ToonPostProcessPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float4 _BlitTexture_TexelSize;
            
            float _ColorLevels;
            float _EdgeThreshold;
            float4 _EdgeColor;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float DetectEdges(float2 uv, float2 texelSize)
            {
                float depthC = SampleSceneDepth(uv);
                float depthL = SampleSceneDepth(uv + float2(-texelSize.x, 0));
                float depthR = SampleSceneDepth(uv + float2(texelSize.x, 0));
                float depthU = SampleSceneDepth(uv + float2(0, texelSize.y));
                float depthD = SampleSceneDepth(uv + float2(0, -texelSize.y));

                float diff = abs(depthC - depthL) + abs(depthC - depthR) + abs(depthC - depthU) + abs(depthC - depthD);
                return diff > _EdgeThreshold ? 1.0 : 0.0;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, input.uv);

                col.rgb = floor(col.rgb * _ColorLevels) / _ColorLevels;

                float2 texelSize = _BlitTexture_TexelSize.xy;
                float edge = DetectEdges(input.uv, texelSize);

                return lerp(col, _EdgeColor, edge);
            }
            ENDHLSL
        }
    }
}