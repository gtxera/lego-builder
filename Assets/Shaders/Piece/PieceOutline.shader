Shader "LegoBuilder/Piece Outline"
{
    Properties
    {
        [HDR]_EmissionColor("Emission Color", Color) = (0.48, 0.82, 1, 1)
        _EmissionIntensity("Emission Intensity", Range(0, 8)) = 2
        _OutlineWidth("Outline Width", Range(0.001, 0.1)) = 0.035
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry+1"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _EmissionColor;
                float _EmissionIntensity;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
                positionWS += normalWS * _OutlineWidth;

                output.positionHCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 emissiveColor = _EmissionColor * _EmissionIntensity;
                emissiveColor.a = _EmissionColor.a;
                return emissiveColor;
            }
            ENDHLSL
        }
    }
}
