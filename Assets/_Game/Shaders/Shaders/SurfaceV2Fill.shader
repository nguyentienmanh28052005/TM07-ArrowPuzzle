Shader "XGame/SurfaceV2Fill"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}

        _FillAmount("Fill Amount", Range(0, 1)) = 1
        _FlipFillAmount("Flip Fill Amount", Range(0, 1)) = 1
        _MaxValue("_MaxValue", Range(-100, 100)) = 0
        _MinValue("_MinValue", Range(-100, 100)) = 0
        _FillDir("_FillDir", Vector) = (1,1,1,1)

        _Color("Color Tint", Color) = (1,1,1,1)
        _Saturation("Color Tint Saturation", Range(0, 10)) = 1
        _Brightness("Brightness", Range(0.1, 10)) = 1
        _Ambient("Ambient Light", Range(0, 1)) = 0.3
        _DiffusePower("Diffuse Power", Range(0, 2)) = 0.7
        _LightDir("Light Direction", Vector) = (0.4,1,0.6,0)

        _NormalStrength("Normal Strength", Range(0, 5)) = 1
        _NormalLightingInfluence("Normal Lighting Influence", Range(0,1)) = 0
        _LightingBlend("Lighting Blend", Range(0, 2)) = 2

        // -------- DISSOLVE BÁM MÉP, LAN TỎA TỪ TỪ --------
        [Header(DISSOLVE)]
        [Toggle] _EnableDissolve ("Enable Dissolve", Float) = 1
        _DissolveNoise ("Dissolve Noise", 2D) = "gray" {}
        _Dissolve ("Dissolve Threshold", Range(0,1)) = 0.3
        _DissolveSoftness ("Dissolve Softness", Range(0.001, 0.5)) = 0.08
        _Cutoff ("Mask Clip Value", Range(0,1)) = 0.35
        _EdgeRegion ("Edge Region Size", Range(0.001, 5)) = 0.5
        _DissolveReveal ("Dissolve Reveal (0..1)", Range(0,5)) = 0
        _DissolveGap ("Dissolve Gap from Fill", Range(-5, 0.5)) = 0.02
        _EdgeWidth ("Edge Glow Width", Range(0.001, 0.5)) = 0.05
        [HDR] _EdgeColor ("Edge Color", Color) = (1,0.7,0.2,1)
        _EdgeIntensity ("Edge Intensity", Range(0,10)) = 2
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 200
        Cull Off
        ZWrite On

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex, _BumpMap;
            float4 _MainTex_ST, _BumpMap_ST;

            sampler2D _DissolveNoise;
            float4 _DissolveNoise_ST;

            float4 _Color;
            float _Saturation, _Brightness, _Ambient, _DiffusePower;
            float4 _LightDir;

            float _NormalStrength, _NormalLightingInfluence, _LightingBlend;

            float _FillAmount, _FlipFillAmount, _MinValue, _MaxValue;
            float4 _FillDir;

            float _EnableDissolve;
            float _Dissolve, _DissolveSoftness, _Cutoff, _EdgeRegion, _DissolveReveal, _DissolveGap, _EdgeWidth, _EdgeIntensity;
            float4 _EdgeColor;

            struct VS_IN
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent: TANGENT;
            };

            struct VS_OUT
            {
                float4 pos       : SV_POSITION;
                float2 uv        : TEXCOORD0;
                float2 uv2       : TEXCOORD1;
                float3 localPos  : TEXCOORD2;
                float3 normal    : TEXCOORD3;
                float3 tangent   : TEXCOORD4;
                float3 bitangent : TEXCOORD5;
            };

            VS_OUT vert(VS_IN v)
            {
                VS_OUT o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _BumpMap);
                o.localPos = v.vertex.xyz;

                float3 wn = UnityObjectToWorldNormal(v.normal);
                float3 wt = UnityObjectToWorldDir(v.tangent.xyz);
                float  ts = v.tangent.w * unity_WorldTransformParams.w;
                float3 wb = cross(wn, wt) * ts;

                o.normal = wn;
                o.tangent = wt;
                o.bitangent = wb;
                return o;
            }

            float3 ApplySaturation(float3 c, float s)
            {
                float g = dot(c, float3(0.2126, 0.7152, 0.0722));
                return lerp(float3(g,g,g), c, s);
            }

            float4 frag(VS_OUT i) : SV_Target
            {
                // ======= TÍNH MÉP FILL (LOCAL) =======
                float3 fillN = normalize(_FillDir.xyz);
                float proj   = dot(i.localPos, fillN);

                bool useA = _FillAmount != 1.0;
                float threshA = lerp(_MinValue, _MaxValue, _FillAmount);     // giữ phía proj < threshA

                bool useB = _FlipFillAmount != 1.0;
                float threshB = lerp(_MaxValue, _MinValue, _FlipFillAmount); // giữ phía proj > threshB

                if (useA && proj >= threshA) discard;
                if (useB && proj <= threshB) discard;

                // ======= Base + Lighting =======
                float4 texColor  = tex2D(_MainTex, i.uv);
                float4 colorTint = _Color; colorTint.rgb = ApplySaturation(colorTint.rgb, _Saturation);

                float3 nTex = UnpackNormal(tex2D(_BumpMap, i.uv2));
                nTex.xy *= _NormalStrength;
                nTex = normalize(float3(nTex.xy, sqrt(saturate(1.0 - dot(nTex.xy, nTex.xy)))));

                float3x3 TBN = float3x3(normalize(i.tangent), normalize(i.tangent), normalize(i.normal));
                float3 worldN_detail = normalize(mul(nTex, TBN));
                float3 worldN_base   = normalize(i.normal);

                float3 L = normalize(_LightDir.xyz);
                float3 N = normalize(lerp(worldN_base, worldN_detail, _NormalLightingInfluence));
                float NdotL = max(dot(N, L), 0.0);
                float lighting = _Ambient + NdotL * _DiffusePower;

                float4 unlit = texColor * colorTint * _Brightness;
                float4 lit   = texColor * colorTint * lighting * _Brightness;
                float4 col   = lerp(unlit, lit, _LightingBlend);

                // ======= DISSOLVE BẬT/TẮT =======
                if (_EnableDissolve > 0.5 && !useB)
                {
                    // Reveal ∈ [0..1]; _EdgeRegion là bề dày tối đa quanh mép.
                    float reveal = saturate(_DissolveReveal);

                    // Cho phép gap âm để viền xuất hiện ở cả mép "trên"
                    float gap = clamp(_DissolveGap, -_EdgeRegion + 1e-5, _EdgeRegion - 1e-5);

                    // Giới hạn tổng bề rộng hoạt động dựa trên |gap|
                    float activeMax   = max(0.0, _EdgeRegion - abs(gap));
                    float activeWidth = max(1e-5, min(activeMax, activeMax * reveal));
                    float feather     = max(1e-5, activeWidth * 0.25); // mép mượt

                    float edgeMask = 0.0;

                    if (useA) {
                        float insideA = threshA - proj; // >0: phía được giữ
                        float aStart = smoothstep(gap,                     gap + feather,                     insideA);
                        float aEnd   = 1.0 - smoothstep(gap + activeWidth, gap + activeWidth + feather, insideA);
                        float mA = aStart * aEnd;
                        mA *= step(0.0, insideA);
                        edgeMask = max(edgeMask, mA);
                    }
                    if (useB) {
                        float insideB = proj - threshB; // >0: phía được giữ
                        float bStart = smoothstep(gap,                     gap + feather,                     insideB);
                        float bEnd   = 1.0 - smoothstep(gap + activeWidth, gap + activeWidth + feather, insideB);
                        float mB = bStart * bEnd;
                        mB *= step(0.0, insideB);
                        edgeMask = max(edgeMask, mB);
                    }

                    if (edgeMask > 0.0001)
                    {
                        float2 duv   = TRANSFORM_TEX(i.uv, _DissolveNoise);
                        float dNoise = tex2D(_DissolveNoise, duv).r;

                        // Làm mềm ngưỡng dissolve
                        float aBand = smoothstep(_Dissolve, _Dissolve + _DissolveSoftness, dNoise);

                        // Ngoài vùng: alpha = 1; trong vùng: pha theo aBand
                        float alpha = lerp(1.0, aBand, edgeMask);

                        // Viền sáng
                        float center = _Dissolve + 0.5 * _DissolveSoftness;
                        float edgeGlow = 1.0 - smoothstep(0.0, _EdgeWidth, abs(dNoise - center));
                        col.rgb += _EdgeColor.rgb * (_EdgeIntensity * edgeGlow * edgeMask);

                        clip(alpha - _Cutoff);
                    }
                }

                return col;
            }
            ENDHLSL
        }
    }
}
