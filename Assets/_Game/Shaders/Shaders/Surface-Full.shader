Shader "XGame/Surface-Full"
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
        _WrapScale("Wrap Scale", Range(0,1)) = 0.35
        _DiffBase("Diffuse Gain Base", Range(0,2)) = 0.90
        _DiffLit("Diffuse Gain By Light", Range(0,2)) = 0.60
        _DiffFloor("Diffuse Floor", Range(0,1)) = 0.28
        
        _SpecSoftMul("Spec Soft Power Mult", Range(0.5,2)) = 1.10
        _SpecTightMul("Spec Tight Power Mult", Range(0.5,2)) = 1.15
        _SpecSoftW("Spec Soft Weight", Range(0,2)) = 0.55
        _SpecTightW("Spec Tight Weight", Range(0,2)) = 0.85
        
        _RimBase("Rim Base", Range(0,2)) = 0.75
        _RimLit("Rim By Light", Range(0,2)) = 0.45
        _GlowPow("Glow Fresnel Power", Range(0.8,3)) = 1.35
        _GlowGain("Glow Gain", Range(0,2)) = 0.45
        
        _DirH("Highlight Dir (xyz)", Vector) = (-0.45, 0.85, 0.35, 0)
        
        _AOStart("AO Start", Range(0,1)) = 0.20
        _AOEnd("AO End", Range(0,1)) = 0.90
        _AOGain("AO Albedo Gain", Range(0,0.5)) = 0.10
        _AOEmit("AO Emission Pull", Range(0,0.5)) = 0.05
        
        _CorePow("Core Power", Range(1,12)) = 6
        _CoreGain("Core Gain", Range(0,0.5)) = 0.08
        
        _SatGain("Saturation Gain", Range(0.8,1.6)) = 1.20
        _GammaLift("Gamma Lift", Range(0.9,1.3)) = 1.08
        
        _ToonSteps("Toon Diffuse Steps", Range(2,8)) = 4
        _SpecSteps("Toon Spec Steps", Range(2,8)) = 4
        _RampSmooth("Ramp Smoothness", Range(0.0,0.2)) = 0.04
        _RampFeather("Ramp Feather Scale", Range(0.5,4.0)) = 1.0
        
        _FaceLightDir("Face Light Dir (xyz)", Vector) = (0.4, 0.6, 0.7, 0)
        _FaceLightWeight("Face Light Blend", Range(0,1)) = 0.0
        _ToonBias("Toon Threshold Bias", Range(-0.5,0.5)) = 0.0
        _AmbientColor("Ambient Tint", Color) = (0.05,0.05,0.08,1)
        
        _ToonBlend("Toon Blend (0=smooth,1=toon)", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 250
        Cull Back
        ZWrite On

        CGPROGRAM
        #pragma surface surf CustomHalfLambert addshadow noforwardadd
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma instancing_options assumeuniformscaling

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;
        half _LightIntensity;
        half _Shininess;
        fixed4 _SpecularColor;
        half _WrapBase, _WrapScale, _DiffBase, _DiffLit, _DiffFloor;
        half _SpecSoftMul, _SpecTightMul, _SpecSoftW, _SpecTightW;
        half _RimBase, _RimLit, _GlowPow, _GlowGain;
        float4 _DirH;
        half _AOStart, _AOEnd, _AOGain, _AOEmit;
        half _CorePow, _CoreGain;
        half _SatGain, _GammaLift;
        half _ToonSteps, _SpecSteps, _RampSmooth, _RampFeather, _ToonBlend;
        float4 _FaceLightDir;
        half _FaceLightWeight, _ToonBias;
        fixed4 _AmbientColor;

        // instancing buffer cho _Color
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
            float3 worldPos;
            INTERNAL_DATA
        };

        // Safe normalize
        inline float3 SafeNormalize(float3 v)
        {
            float len = max(length(v), 1e-5);
            return v / len;
        }

        // Smooth toon quantization
        inline float SmoothToon(float v, float steps, float smooth, float feather)
        {
            float x = saturate(v) * steps;
            float q0 = floor(x);
            float center = q0 + 0.5;
            float fw = fwidth(x);
            float w = max(smooth * steps, fw * feather);
            float a = smoothstep(-w, w, x - center);
            float q = lerp(q0, q0 + 1.0, a);
            return saturate(q / steps);
        }

        // Custom Half-Lambert Lighting with Full Features
        inline half4 LightingCustomHalfLambert(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            float3 N = SafeNormalize((float3)s.Normal);
            float3 Lmain = SafeNormalize((float3)lightDir);
            float3 Lface = SafeNormalize(_FaceLightDir.xyz);
            float3 L = SafeNormalize(lerp(Lmain, Lface, (float)_FaceLightWeight));
            float3 V = SafeNormalize((float3)viewDir);

            float lit = saturate((float)_LightIntensity);

            // Wrap Half-Lambert
            float wrap = _WrapBase + lit * _WrapScale;
            float numer = dot(N, L) + wrap;
            float denom = max(0.001, 1.0 + wrap);
            float NdotLw = saturate(numer / denom);
            float hLambert = NdotLw * (_DiffBase + lit * _DiffLit) + _DiffFloor + (float)_ToonBias;

            // Toon quantization for diffuse
            float steps = max(2.0, (float)_ToonSteps);
            float qd = SmoothToon(hLambert, steps, (float)_RampSmooth, (float)_RampFeather);
            float diffuseRaw = saturate(hLambert);

            // Dual-spec
            float3 H = SafeNormalize(L + V);
            float NdotH = saturate(dot(N, H));
            float shinS = max(1.0, (float)_Shininess) * _SpecSoftMul;
            float shinT = max(2.0, (float)_Shininess * 2.0) * _SpecTightMul;
            float specSoft = pow(NdotH, shinS);
            float specTight = pow(NdotH, shinT);
            float specMix = (specSoft * _SpecSoftW + specTight * _SpecTightW);

            // Toon quantization for specular
            float ssteps = max(2.0, (float)_SpecSteps);
            float qs = SmoothToon(specMix, ssteps, (float)_RampSmooth, (float)_RampFeather);

            // Rim + fake bloom
            float NdotV = saturate(dot(N, V));
            float fres = pow(1.0 - NdotV, 2.2);
            float3 rim = _SpecularColor.rgb * fres * (_RimBase + lit * _RimLit);
            float3 glow = _SpecularColor.rgb * pow(fres, _GlowPow) * _GlowGain;

            // Build both smooth and toon colors
            float3 colSmooth = s.Albedo * (_LightColor0.rgb * (diffuseRaw * atten * 2.0) + _AmbientColor.rgb);
            float3 colToon = s.Albedo * (_LightColor0.rgb * (qd * atten * 2.0) + _AmbientColor.rgb);
            float3 specSmooth = saturate(specMix) * _SpecularColor.rgb;
            float3 specToon = qs * _SpecularColor.rgb;

            // Final blend
            float3 col = lerp(colSmooth + specSmooth * atten, colToon + specToon * atten, (float)_ToonBlend);
            col += rim + glow;

            // Pastel grading
            float luma = dot(col, float3(0.299, 0.587, 0.114));
            col = lerp(luma.xxx, col, _SatGain);
            float invGamma = 1.0 / max(0.001, (float)_GammaLift);
            col = pow(saturate(col), invGamma);

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

            float3 Nw = SafeNormalize(WorldNormalVector(IN, o.Normal));
            float3 DirH = SafeNormalize(_DirH.xyz);

            float lobe = pow(saturate(dot(Nw, DirH)), max(8.0, (float)_Shininess * 1.2));
            o.Emission += _SpecularColor.rgb * lobe * 0.90;

            float ao = smoothstep(_AOStart, _AOEnd, -Nw.y);
            o.Albedo *= (1.0 - ao * _AOGain);
            o.Emission += -_SpecularColor.rgb * ao * _AOEmit;

            float NdotV = saturate(dot(Nw, SafeNormalize(IN.viewDir)));
            float core = pow(NdotV, max(1.0, (float)_CorePow)) * _CoreGain;
            o.Emission += _SpecularColor.rgb * core;

            float s = saturate((float)_LightIntensity);
            o.Albedo = lerp(o.Albedo * 0.98, o.Albedo * 1.02, s);
        }
        ENDCG
    }

    Fallback "Legacy Shaders/VertexLit"
}
