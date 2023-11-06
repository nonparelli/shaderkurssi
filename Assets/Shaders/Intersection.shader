Shader "Custom/Intersection"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _IntersectionColor("Intersection Color", Color) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "IntersectionUnlit"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }

            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex Vertex
            #pragma fragment Fragment
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _IntersectionColor;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings Vertex(const Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.positionWS = TransformObjectToWorld(input.positionOS);
                return output;
            }

            float4 Fragment(Varyings input) : SV_TARGET
            {
                /*
                    Fragment Stagessa ensimmäiseksi hommataan screen space UV. Tämä hoituu GetNormalizedScreenSpaceUV(positionHCS) funktiolla.

                    Sen jälkeen tarvitaan linear eye depth depth tekstuurista. Se saadaan funktioilla SampleSceneDepth(screenUV) ja LinearEyeDepth(SceneDepth, _ZBufferParams).

                    Sitten tarvitaan vielä objectin linear eye depth. Se saadaan myös samalla funktiolla, mutta eri overloadilla: LinearEyeDepth(positionWS, UNITY_MATRIX_V).

                    Seuraavaksi lasketaan lerp-arvo. Se saadaan kaavalla pow(1 - saturate(depthTexture - depthObject), 15).
                    
                    Sitten voidaan lerpata kahden värin välillä arvoa käyttäen. lerp(colObject, colIntersection, lerpValue);
                */
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionHCS);
                float sceneDepth = SampleSceneDepth(screenUV);
                float depthTexture = LinearEyeDepth(sceneDepth,_ZBufferParams);
                float depthObject = LinearEyeDepth(input.positionWS,UNITY_MATRIX_V);
                float lerpValue = pow(1-saturate(depthTexture-depthObject),15);
                
                return lerp(_Color,_IntersectionColor,lerpValue);
            }
            ENDHLSL
        }
Pass
        {
            Name "Depth"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            Cull Back
            ZTest LEqual
            ZWrite On
            ColorMask R

            HLSLPROGRAM
    
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #include "Common\DepthOnly.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Normals"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }

            Cull Back
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
    
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag

            #include "Common\DepthNormalsOnly.hlsl"
    
            ENDHLSL
        }
    }
}