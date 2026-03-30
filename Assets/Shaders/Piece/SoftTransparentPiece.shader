Shader "LegoBuilder/Soft Transparent Piece"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range(0, 1)) = 0.38
        _AmbientStrength("Ambient Strength", Range(0, 1)) = 0.65
        _MinLight("Minimum Light", Range(0, 1)) = 0.3
        _Smoothness("Smoothness", Range(0, 1)) = 0.22
        _SpecularStrength("Specular Strength", Range(0, 1)) = 0.08
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
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Alpha;
                half _AmbientStrength;
                half _MinLight;
                half _Smoothness;
                half _SpecularStrength;
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
                half3 normalWS : TEXCOORD1;
                half3 viewDirWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.viewDirWS = SafeNormalize(GetWorldSpaceViewDir(positionInputs.positionWS));
                output.shadowCoord = GetShadowCoord(positionInputs);

                return output;
            }

            half3 AccumulateLight(half3 baseColor, half3 normalWS, half3 viewDirWS, Light lightData, half specularPower)
            {
                half3 lightDir = SafeNormalize(lightData.direction);
                half ndotl = saturate(dot(normalWS, lightDir));
                half diffuseTerm = max(_MinLight, ndotl) * lightData.distanceAttenuation * lightData.shadowAttenuation;

                half3 halfVector = SafeNormalize(lightDir + viewDirWS);
                half ndoth = saturate(dot(normalWS, halfVector));
                half specularTerm = pow(ndoth, specularPower) * _SpecularStrength * ndotl;
                specularTerm *= lightData.distanceAttenuation * lightData.shadowAttenuation;

                half3 diffuse = baseColor * lightData.color * diffuseTerm;
                half3 specular = lightData.color * specularTerm;
                return diffuse + specular;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 baseColor = _BaseColor.rgb;
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = SafeNormalize(input.viewDirWS);
                half specularPower = lerp(8.0h, 48.0h, saturate(_Smoothness));

                half3 color = baseColor * _AmbientStrength;

                Light mainLight = GetMainLight(input.shadowCoord);
                color += AccumulateLight(baseColor, normalWS, viewDirWS, mainLight, specularPower);

                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < additionalLightsCount; i++)
                {
                    Light additionalLight = GetAdditionalLight(i, input.positionWS);
                    color += AccumulateLight(baseColor, normalWS, viewDirWS, additionalLight, specularPower);
                }
                #endif

                return half4(saturate(color), saturate(_Alpha));
            }
            ENDHLSL
        }
    }
}
