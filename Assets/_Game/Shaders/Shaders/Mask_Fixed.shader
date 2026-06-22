Shader "Custom/URP_StencilMaskWriter"
{
    Properties
    {
        _StencilRef("Stencil Ref", Range(0,255)) = 2
        _ShowMask("Show Mask (Debug)", Float) = 1
        _MaskColor("Mask Color (Debug)", Color) = (0,1,0,0.25)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry-50" }

        Pass
        {
            Name "StencilMaskWriter"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            ZTest Always      // ✅ IMPORTANT: always write stencil
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
                Fail Replace
                ZFail Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            float _ShowMask;
            float4 _MaskColor;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 c = (half4)_MaskColor;
                c.a *= (half)_ShowMask; // debug visible
                return c;
            }
            ENDHLSL
        }
        Pass
        {
            Name "StencilMaskDepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite Off
            ZTest Always
            ColorMask 0
            Cull Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
                Fail Replace
                ZFail Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragDepth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 fragDepth (Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

    }
}
