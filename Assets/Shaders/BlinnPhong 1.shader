Shader "Custom/BlinnPhong1"
{
    Properties
    {
        _Color("Color", Color) = (0.1, 0.4, 0, 1)
        _MainTex("Main texture", 2D) = "white" {}
        _NormalMap("Normal Map",2D)="bump"{}
        _Shininess("Shininess", Range(1, 512)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"
        }
        HLSLINCLUDE
        struct Attributes // Vertex stage input
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv : TEXCOORD0;
            // We need these later but they're vertex stage inputs, we need to store them for frag stage later?
        };

        struct Varyings //vertex to frag / frag stage input / vertex output
        {
            float4 positionHCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 tangentWS : TEXCOORD2;
            float3 bitagentWS : TEXCOORD3;
            float2 uv :TEXCOORD4;
        };

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        Varyings vert(Attributes input)
        {
            Varyings output;
            const VertexPositionInputs position_inputs = GetVertexPositionInputs(input.positionOS);
            const VertexNormalInputs normal_inputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

            output.positionHCS = position_inputs.positionCS;
            output.normalWS = normal_inputs.normalWS;
            output.tangentWS = normal_inputs.tangentWS;
            output.bitagentWS = normal_inputs.bitangentWS;
            output.positionWS = position_inputs.positionWS; 
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
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _NormalMap_ST;
                 float4 _MainTex_ST;
                float _Shininess;
            CBUFFER_END

            float4 BlinnPhong(const Varyings input,float4 color)
            {
                const Light mainLight = GetMainLight();

                const float3 ambientLight = 0.1 * mainLight.color;

                const float3 diffuseLight = saturate(dot(input.normalWS, mainLight.direction)) * mainLight.color;
                
                const float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                
                const float3 halfwayDir = normalize(mainLight.direction + viewDir);

                const float3 specularLight = pow(saturate(dot(input.normalWS, halfwayDir)), _Shininess) * mainLight.
                    color;
                return float4(
                    (ambientLight + diffuseLight + specularLight * 10 /* <-- vaihtoehtoinen 10 */) * color.rgb, 1);
            }

            float4 frag(Varyings input) : SV_TARGET
            {
                const float4 texColor = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,TRANSFORM_TEX(input.uv,_MainTex));
                const float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,input.uv));
                const float3x3 tangentToWorld = float3x3(input.tangentWS,input.bitagentWS,input.normalWS);
                
                const float3 normalWS = TransformTangentToWorld(normalTS,tangentToWorld,true);
                
                input.normalWS=normalWS;
                
                return BlinnPhong(input,texColor);
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
}