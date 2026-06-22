Shader "XGame/SurfaceBlendLitUnlitSmooth"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Depth", Range(0, 5)) = 1.0
        _LightIntensity("Light Intensity", Float) = 0.5
        _Shininess("Shininess", Range(1, 100)) = 20
        _SpecularColor("Specular Color", Color) = (0.0588, 0.0588, 0.0588, 1)
        _LitToggle("Lighting Blend (0 = Unlit, 1 = Full Lit)", Range(0,1)) = 1
        _Saturation("Saturation", Range(0, 2)) = 1

        _AOMap("Ambient Occlusion Map", 2D) = "white" {}
        _AOStrength("AO Strength", Range(0,1)) = 1
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
            LOD 200
            Cull Back

            CGPROGRAM
            #pragma surface surf CustomHalfLambert addshadow
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _AOMap;

            fixed4 _Color;
            half   _BumpScale;
            half   _LightIntensity;
            half   _Shininess;
            fixed4 _SpecularColor;
            half   _Saturation;
            float  _LitToggle;
            half   _AOStrength;

            struct Input {
                float2 uv_MainTex;
                float2 uv_BumpMap;
                float2 uv_AOMap;
            };

            inline half4 LightingCustomHalfLambert(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
            {
                half3 unlit = s.Albedo;

                half  NdotL = max(0, dot(s.Normal, lightDir));
                half  hLambert = NdotL * _LightIntensity + 0.5;

                half3 halfDir = normalize(lightDir + viewDir);
                half  NdotH = max(dot(s.Normal, halfDir), 0.0);
                half  spec = pow(NdotH, _Shininess);
                half3 specular = _SpecularColor.rgb * spec;

                half3 lit = s.Albedo * _LightColor0.rgb * (hLambert * atten * 2) + specular;
                half3 finalRGB = lerp(unlit, lit, saturate(_LitToggle));

                return half4(finalRGB, s.Alpha);
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                // Saturation: 0 = BW, 1 = gốc, >1 = tăng rực
                half gray = dot(c.rgb, half3(0.299, 0.587, 0.114));
                c.rgb = lerp(half3(gray, gray, gray), c.rgb, _Saturation);

                // AO Map
                half ao = tex2D(_AOMap, IN.uv_AOMap).r;
                ao = lerp(1.0, ao, _AOStrength); // blend AO strength
                c.rgb *= ao;

                o.Albedo = c.rgb;
                o.Alpha = c.a;

                half3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
                n.xy *= _BumpScale;
                n.z = sqrt(saturate(1.0 - dot(n.xy, n.xy)));
                o.Normal = n;
            }
            ENDCG
        }

            Fallback "Legacy Shaders/VertexLit"
}
