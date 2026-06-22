Shader "XGame/Surface-Mobile"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Depth", Range(0, 2)) = 1.35
        
        _LightIntensity("Light Intensity", Range(0,1)) = 0.90
        _Shininess("Shininess", Range(1, 100)) = 80
        _SpecularColor("Specular Color", Color) = (0.98, 1.0, 1.0, 1)
        
        _WrapBase("Wrap Base", Range(0,1)) = 0.45
        _DiffBase("Diffuse Gain", Range(0,2)) = 0.90
        _DiffFloor("Diffuse Floor", Range(0,1)) = 0.28
        
        _ToonSteps("Toon Steps", Range(2,6)) = 3
        _RampSmooth("Ramp Smoothness", Range(0.0,0.15)) = 0.06
        _ToonBlend("Toon Blend", Range(0,1)) = 0.8
        
        _RimBase("Rim Base", Range(0,2)) = 0.75
        _GlowGain("Glow Gain", Range(0,2)) = 0.45
        
        _SatGain("Saturation Gain", Range(0.8,1.6)) = 1.20
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        Cull Back
        ZWrite On

        CGPROGRAM
        #pragma surface surf CustomHalfLambert addshadow noforwardadd
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma instancing_options assumeuniformscaling

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;
        half _LightIntensity;
        half _Shininess;
        fixed4 _SpecularColor;
        half _WrapBase, _DiffBase, _DiffFloor;
        half _ToonSteps, _RampSmooth, _ToonBlend;
        half _RimBase, _GlowGain;
        half _SatGain;

        // instancing buffer cho _Color
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
        };

        // Simple toon quantization (mobile optimized)
        inline float SimpleToon(float v, float steps, float smooth)
        {
            float x = saturate(v) * steps;
            float q = floor(x + smooth);
            return saturate(q / steps);
        }

        // Mobile-optimized Custom Half-Lambert Lighting
        inline half4 LightingCustomHalfLambert(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            float3 N = normalize((float3)s.Normal);
            float3 L = normalize((float3)lightDir);
            float3 V = normalize((float3)viewDir);

            float lit = saturate((float)_LightIntensity);

            // Simple Wrap Half-Lambert
            float wrap = (float)_WrapBase;
            float numer = dot(N, L) + wrap;
            float denom = max(0.001, 1.0 + wrap);
            float NdotLw = saturate(numer / denom);
            float hLambert = NdotLw * (float)_DiffBase + (float)_DiffFloor;

            // Simple toon quantization
            float steps = max(2.0, (float)_ToonSteps);
            float qd = SimpleToon(hLambert, steps, (float)_RampSmooth);
            float diffuseRaw = saturate(hLambert);

            // Simple specular
            float3 H = normalize(L + V);
            float NdotH = saturate(dot(N, H));
            float spec = pow(NdotH, (float)_Shininess);

            // Rim lighting
            float NdotV = saturate(dot(N, V));
            float fres = pow(1.0 - NdotV, 2.2);
            float3 rim = _SpecularColor.rgb * fres * (float)_RimBase;
            float3 glow = _SpecularColor.rgb * fres * (float)_GlowGain;

            // Build colors
            float3 colSmooth = s.Albedo * _LightColor0.rgb * (diffuseRaw * atten * 2.0);
            float3 colToon = s.Albedo * _LightColor0.rgb * (qd * atten * 2.0);
            float3 specSmooth = spec * _SpecularColor.rgb;
            float3 specToon = SimpleToon(spec, steps, (float)_RampSmooth) * _SpecularColor.rgb;

            // Final blend
            float3 col = lerp(colSmooth + specSmooth * atten, colToon + specToon * atten, (float)_ToonBlend);
            col += rim + glow;

            // Simple saturation
            float luma = dot(col, float3(0.299, 0.587, 0.114));
            col = lerp(luma.xxx, col, (float)_SatGain);

            return half4(col, s.Alpha);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float4 instColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed4 baseC = tex2D(_MainTex, IN.uv_MainTex) * instColor;
            o.Albedo = baseC.rgb;
            o.Alpha = baseC.a;

            float3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            n.xy *= (float)_BumpScale;
            float xy2 = saturate(dot(n.xy, n.xy));
            n.z = sqrt(max(1e-4, 1.0 - xy2));
            o.Normal = normalize(n);
        }
        ENDCG
    }

    Fallback "Legacy Shaders/VertexLit"
}
