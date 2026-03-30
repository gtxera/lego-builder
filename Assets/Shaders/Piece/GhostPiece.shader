Shader "LegoBuilder/Ghost Piece"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range(0, 1)) = 0.18
        _SourceTransparent("Source Transparent", Range(0, 1)) = 0
        [HDR]_GlowColor("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity("Glow Intensity", Range(0, 4)) = 1.35
        _GlowWidth("Glow Width", Range(0.5, 8)) = 3.2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _GlowColor;
                float _Alpha;
                float _SourceTransparent;
                float _GlowIntensity;
                float _GlowWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = positionInputs.positionCS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.viewDirWS = normalize(GetWorldSpaceViewDir(positionInputs.positionWS));

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half ndotv = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                half edgeFactor = pow(saturate(1.0h - ndotv), _GlowWidth);
                half glowMask = saturate(_SourceTransparent) * saturate(edgeFactor * _GlowIntensity);
                half3 glow = _GlowColor.rgb * glowMask;
                half3 finalColor = _BaseColor.rgb + glow;

                return half4(finalColor, saturate(_Alpha));
            }
            ENDHLSL
        }
    }
}
