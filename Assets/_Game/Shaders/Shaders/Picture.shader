Shader "XGame/Basket"
{
    Properties
    {
        _BaseColor("Color", Color) = (1,1,1,1)
        _BaseMap("Albedo", 2D) = "white" {}
        _HalfLambertStrength("HalfLambert Strength", Range(0,1)) = 1

        _Brightness("Brightness", Range(0,2)) = 1
        _Contrast("Contrast",   Range(0,2)) = 1
        _Saturation("Saturation", Range(0,2)) = 1
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf HalfLambert fullforwardshadows alpha:fade
            #pragma target 3.0

            sampler2D _BaseMap;
            fixed4 _BaseColor;
            float _HalfLambertStrength;

            float _Brightness;
            float _Contrast;
            float _Saturation;

            struct Input
            {
                float2 uv_BaseMap;
                float4 color : COLOR;
            };

            inline fixed3 ApplyContrast(fixed3 col, float contrast)
            {
                // contrast = 1 => giữ nguyên
                return (col - 0.5) * contrast + 0.5;
            }

            inline fixed3 ApplySaturation(fixed3 col, float sat)
            {
                // sat = 1 => giữ nguyên, 0 => grayscale
                float luma = dot(col, fixed3(0.2126, 0.7152, 0.0722));
                return lerp(fixed3(luma, luma, luma), col, sat);
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor * IN.color;

                fixed3 rgb = c.rgb;

                // 👉 Contrast & Saturation control
                rgb = ApplyContrast(rgb, _Contrast);
                rgb = ApplySaturation(rgb, _Saturation);

                o.Albedo = saturate(rgb);
                o.Alpha = c.a;
            }

            inline fixed4 LightingHalfLambert(SurfaceOutput s, fixed3 lightDir, fixed atten)
            {
                float ndl = dot(s.Normal, lightDir);
                float halfLambert = ndl * 0.5 + 0.5;
                halfLambert = lerp(saturate(ndl), saturate(halfLambert), _HalfLambertStrength);

                fixed3 col = s.Albedo * _LightColor0.rgb * halfLambert * atten;

                // 👉 Brightness control (tổng thể sau lighting)
                col *= _Brightness;

                return fixed4(col, s.Alpha);
            }
            ENDCG
        }

            FallBack "Diffuse"
}
