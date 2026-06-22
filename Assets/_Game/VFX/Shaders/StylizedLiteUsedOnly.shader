Shader "Custom/StylizedLiteUsedOnly"
{
    Properties
    {
        [Toggle] _MobileMode ("Mobile Mode", Float) = 1

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Render Face", Float) = 1
        [Toggle] _ZWrite ("Depth Write", Float) = 1

        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}

        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.1,0.1,0.1,1)

        [KeywordEnum(Default)] _RampType ("Ramp Type", Float) = 0
        _RampThreshold ("Threshold", Range(0,1)) = 0.7
        _RampSmoothing ("Smoothing", Range(0.001,1)) = 0.1

        [Toggle] _EnableSpecular ("Specular", Float) = 1
        [KeywordEnum(Stylized)] _SpecType ("Type", Float) = 0
        [HDR]_SpecColor ("Specular Color", Color) = (1,1,1,1)
        _SpecSize ("Size", Range(0.001,1)) = 0.005
        _SpecSmoothness ("Smoothing", Range(0.001,1)) = 1

        _IndirectDiffuseStrength ("Indirect Diffuse Strength", Range(0,2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Cull [_Cull]
        ZWrite [_ZWrite]

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _MobileMode;
            float4 _Color;
            float4 _HighlightColor;
            float4 _ShadowColor;

            float _RampThreshold;
            float _RampSmoothing;

            float _EnableSpecular;
            float4 _SpecColor2;
            float _SpecSize;
            float _SpecSmoothness;

            float _IndirectDiffuseStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir     : TEXCOORD3;
                float3 ambientData : TEXCOORD4;
                float4 color  : COLOR;
                SHADOW_COORDS(5)
                UNITY_FOG_COORDS(6)
            };

            float Ramp(float ndl01)
            {
                return smoothstep(
                    _RampThreshold - _RampSmoothing * 0.5,
                    _RampThreshold + _RampSmoothing * 0.5,
                    ndl01
                );
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = UnityWorldSpaceViewDir(o.worldPos);
                o.color = v.color;

                if (_MobileMode > 0.5)
                    o.ambientData = ShadeSH9(float4(normalize(o.worldNormal), 1.0));
                else
                    o.ambientData = 0;

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(i.viewDir);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 H = normalize(L + V);

                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color * i.color;

                float shadowAtten = SHADOW_ATTENUATION(i);

                float ndlRaw = dot(N, L);
                float ndl01 = saturate(ndlRaw * 0.5 + 0.5);
                float ramp = Ramp(ndl01) * shadowAtten;

                fixed3 toonTint = lerp(_ShadowColor.rgb, _HighlightColor.rgb, ramp);
                fixed3 color = albedo.rgb * toonTint * _LightColor0.rgb;

                float3 ambient = (_MobileMode > 0.5)
                    ? i.ambientData
                    : ShadeSH9(float4(N, 1.0));

                color += ambient * albedo.rgb * _IndirectDiffuseStrength;

                if (_EnableSpecular > 0.5)
                {
                    float ndh = saturate(dot(N, H));

                    float edge0 = 1.0 - _SpecSize - _SpecSmoothness * 0.5;
                    float edge1 = 1.0 - _SpecSize + _SpecSmoothness * 0.5;
                    float specularTerm = smoothstep(edge0, edge1, ndh);

                    color += specularTerm * _SpecColor2.rgb * _LightColor0.rgb * shadowAtten;
                }

                fixed4 finalCol = fixed4(color, 1.0);
                UNITY_APPLY_FOG(i.fogCoord, finalCol);
                return finalCol;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}