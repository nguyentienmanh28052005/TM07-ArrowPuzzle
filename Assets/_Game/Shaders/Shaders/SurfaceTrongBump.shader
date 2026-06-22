Shader "XGame/SurfaceTrongBump"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Intensity", Range(0, 2)) = 1.0
        _LightIntensity("Light Intensity", Float) = 0.5
        _Shininess("Shininess", Range(1, 100)) = 20
        _SpecularColor("Specular Color", Color) = (0.0588, 0.0588, 0.0588, 1)
      //  _Outline("Outline Color", Color) = (0,0,0,1)
      // _OutlineSize("Outline Thickness", Float) = 0.05
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            // ✅ Outline Pass
            Pass
            {
                Cull Front
                ZWrite On
                ZTest LEqual
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                half _OutlineSize;
                fixed4 _Outline;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    float3 norm = normalize(v.normal);
                    v2f o;
                    float3 offset = norm * _OutlineSize;
                    o.pos = UnityObjectToClipPos(v.vertex + float4(offset, 0));
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return _Outline;
                }
                ENDCG
            }

            // ✅ Main Surface Pass
            LOD 200
            ZWrite On
            Cull Back

            CGPROGRAM
            #pragma surface surf CustomHalfLambert addshadow
            #pragma target 3.0

            sampler2D _MainTex;
            sampler2D _BumpMap;
            fixed4 _Color;
            half _LightIntensity;
            half _Shininess;
            fixed4 _SpecularColor;
            half _BumpScale;

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_BumpMap;
            };

            inline half4 LightingCustomHalfLambert(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
            {
                half NdotL = max(0, dot(s.Normal, lightDir));
                half hLambert = NdotL * _LightIntensity + 0.5;

                half3 halfDir = normalize(lightDir + viewDir);
                half NdotH = max(dot(s.Normal, halfDir), 0.0);
                half spec = pow(NdotH, _Shininess);
                half3 specular = _SpecularColor.rgb * spec;

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

                float3 unpackedNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
                unpackedNormal = lerp(float3(0, 0, 1), unpackedNormal, _BumpScale); // Điều chỉnh độ mạnh của bump
                o.Normal = normalize(unpackedNormal);
            }
            ENDCG
        }

            Fallback "Legacy Shaders/VertexLit"
}
