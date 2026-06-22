Shader "XGame/Basket"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _BaseMap ("Albedo", 2D) = "white" {}
        _HalfLambertStrength ("HalfLambert Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf HalfLambert fullforwardshadows
        #pragma target 3.0

        sampler2D _BaseMap;
        fixed4 _BaseColor;
        float _HalfLambertStrength;

        struct Input
        {
            float2 uv_BaseMap;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor;
            o.Albedo = c.rgb;
            o.Alpha  = c.a;
        }

        inline fixed4 LightingHalfLambert(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            // Half-Lambert: (N.L * 0.5 + 0.5)
            float ndl = dot(s.Normal, lightDir);
            float halfLambert = ndl * 0.5 + 0.5;
            halfLambert = lerp(saturate(ndl), saturate(halfLambert), _HalfLambertStrength);

            fixed3 col = s.Albedo * _LightColor0.rgb * halfLambert * atten;
            return fixed4(col, s.Alpha);
        }
        ENDCG
    }

    FallBack "Diffuse"
}
