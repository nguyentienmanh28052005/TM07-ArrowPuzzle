Shader "Custom/Stencil/MaskWriter"
{
    Properties
    {
        _StencilRef ("Stencil Ref", Float) = 1
        _WriteMask ("Stencil Write Mask", Float) = 255

        // Optional debug (turn on to see the mask area)
        [Toggle] _DebugColor ("Debug Color (render mask)", Float) = 0
        _Color ("Debug Color", Color) = (0,1,0,0.25)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry-10" }
        LOD 100

        Pass
        {
            Name "StencilMask"

            // Write stencil
            Stencil
            {
                Ref [_StencilRef]
                WriteMask [_WriteMask]
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _DebugColor;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // When debug off, we don't want to draw any color.
                // We'll use ColorMask 0 via conditional technique:
                // Return transparent if debug off; still cheap and OK.
                return (_DebugColor > 0.5) ? _Color : fixed4(0,0,0,0);
            }
            ENDCG

            // If debug is off, you can also force no color writes by enabling this line.
            // But ShaderLab doesn't allow toggling ColorMask dynamically.
            // If you never need debug, uncomment next line and remove debug props.
            // ColorMask 0
        }
    }
}