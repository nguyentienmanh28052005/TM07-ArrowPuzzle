Shader "TAT/SpriteDissolveTopDownNoise"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1.2)) = 0.0
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.2
        [HDR]_EdgeColor ("Edge Color (HDR)", Color) = (2, 1, 0, 1)
        _EdgeWidth ("Edge Width", Range(0.001,0.2)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float _DissolveAmount;
            float _NoiseStrength;
            float4 _EdgeColor;
            float _EdgeWidth;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.texcoord, _NoiseTex); // Support Tiling/Offset
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float verticalMask = i.uv.y;
                float noise = tex2D(_NoiseTex, i.noiseUV).r;
                float dissolveVal = verticalMask + noise * _NoiseStrength;

                float dissolveEdge = _DissolveAmount - dissolveVal;

                float edge = saturate(1.0 - pow(abs(dissolveEdge / _EdgeWidth), 4.0));
                float inside = step(dissolveVal, _DissolveAmount);

                float3 baseColor = col.rgb * inside;
                float3 glow = _EdgeColor.rgb * edge;

                float3 finalColor = baseColor + glow;
                float finalAlpha = col.a * inside;

                return float4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
