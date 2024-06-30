Shader "Hidden/GoblinzMechanics/BSC"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Hue ("Hue", Range(-360, 360)) = 0
        _Brightness ("Brightness", Range(-1, 1)) = 0
        _Contrast("Contrast", Range(0, 2)) = 1
        _Saturation("Saturation", Range(0, 2)) = 1
    }

    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };
    struct Varyings 
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };
    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    TEXTURE2D_X(_MainTex);
    float4 _MainTex_ST;
    float _Hue;
    float _Brightness;
    float _Contrast;
    float _Saturation;
    
    Varyings Vert(Attributes input)
    {
        Varyings output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

        return output;
    }

    inline float3 ApplyHue(float3 aColor, float aHue)
    {
        float angle = aHue * 3.14159 * 2.0;
        float3 k = float3(0.57735, 0.57735, 0.57735);
        float3 p = cos(angle) * aColor + sin(angle) * k * dot(k, aColor);
        return p; 
    }

    float3 ApplySaturation(float3 col, float sat)
    {
        float3 grayscale = dot(col, float3(0.2126, 0.7152, 0.0722));
        return lerp(grayscale, col, sat);
    }

    inline float4 ApplyHSBEffect(float4 startColor)
    {
        float4 outputColor = startColor;
        outputColor.rgb *= _Brightness;
        outputColor.rgb = ApplyHue(outputColor.rgb, _Hue / 360.0);
        outputColor.rgb = ApplySaturation(outputColor.rgb, _Saturation);
        outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast)+0.5f;
        float3 intensity = dot(outputColor.rgb, float3(0.299, 0.587, 0.114));
        return outputColor;
    }

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        
        float2 uv = ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord);

        float4 startColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, uv);
        float4 hsbColor = ApplyHSBEffect(startColor);
        return hsbColor;
    }
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "HBS"
            Tags { "LightMode" = "ForwardLit" }

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
            #pragma fragment CustomPostProcess
            #pragma vertex Vert
            ENDHLSL
        }
    }

    Fallback Off
}