Shader "XGame/SurfaceAlpha"
{
    Properties
    {
        [Header(Main Settings)]
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Tint Color", Color) = (1,1,1,1)

        [Header(Lighting Settings)]
        _LightIntensity("Light Intensity", Range(0, 2)) = 0.5
        _ShininessPinky("Shininess", Range(1, 100)) = 20

        [Header(Specular Settings)]
        _Speculars("Specular Color", Color) = (1,1,1,1)
        _SpecularStrength("Specular Strength", Range(0, 1)) = 1
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 200
            Cull Back

            // --------- PASS 1: Depth Prepass with alpha clip ---------
            Pass
            {
                Tags { "LightMode" = "Always" }
                ZWrite On
                ColorMask 0

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _Color;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                    clip(col.a - 0.01);
                    return 0;
                }
                ENDCG
            }

            // --------- PASS 2: Transparent rendering with lighting ---------
            CGPROGRAM
            #pragma surface surf CustomAlphaLighting alpha:fade
            #pragma target 3.0

            sampler2D _MainTex;
            fixed4 _Color;
            half _LightIntensity;
            half _ShininessPinky;
            fixed4 _Speculars;
            half _SpecularStrength;

            struct Input
            {
                float2 uv_MainTex;
            };

            inline half4 LightingCustomAlphaLighting(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
            {
                half NdotL = max(0, dot(s.Normal, lightDir));
                half hLambert = NdotL * _LightIntensity + 0.5;

                half3 halfDir = normalize(lightDir + viewDir);
                half NdotH = max(dot(s.Normal, halfDir), 0.0);
                half spec = pow(NdotH, _ShininessPinky);
                half3 specular = _Speculars.rgb * spec * _SpecularStrength;

                half4 c;
                c.rgb = s.Albedo * _LightColor0.rgb * (hLambert * atten * 2) + specular;
                c.a = s.Alpha;
                return c;
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }

            ENDCG
        }

            Fallback "Legacy Shaders/Transparent/Diffuse"
}
