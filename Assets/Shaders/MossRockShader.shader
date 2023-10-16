Shader "Custom/MossRockShader"
{
    Properties
    {
        _MainTex("Main texture", 2D) = "white" {}
   
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"
        }
        HLSLINCLUDE
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformWorldToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv=input.uv;
                return output;
            }
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            
            
            CBUFFER_START(UnityPerMaterial)
            float _Shininess;
            float4 _MainTex_ST;
            float4 _Color;
            CBUFFER_END
            
            float4 frag(Varyings input):SV_TARGET
            {
                float2 tiling = _MainTex_ST.xy;
                float2 offset = _MainTex_ST.zw+_Time*float2(5,0.1);
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv*tiling + offset);
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
            // PITÄÄ OLLA RELATIVE PATH TIEDOSTOON!!!
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
    FallBack "Diffuse"
}