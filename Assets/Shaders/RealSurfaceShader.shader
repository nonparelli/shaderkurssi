Shader "Custom/RealSurfaceShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color",Color)=(1,1,1,1)
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [NoScaleOffset] [Normal] _NormalMap("Normal Map",2D)="bump"{}
        [NoScaleOffset] _RoughnessMap("Roughness Map",2D)="white"{}
        [NoScaleOffset] _OcclusionMap("Amibent Occlusion Map",2D)="white"{}
        [NoScaleOffset] _MetallicMap("Metallic Map",2D)="black"{}
        [NoScaleOffset] _ParallaxMap("Parallax Map",2D)="white"{}
        _ParallaxStrength("Parallax Strength", Range(0,1))=0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"
        }
        Pass
        {
            Name "StandardSurf"
            Tags{"LightMode"="UniversalForward"}
            
            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
            
            HLSLPROGRAM

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            #pragma vertex Vertex
            #pragma fragment Fragment

            // UPR keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            
            #include "RealSurfaceShaderProgram.hlsl"  
            
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