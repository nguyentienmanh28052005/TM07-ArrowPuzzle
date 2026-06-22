Shader "XGame/Jelly"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _ColorStrength("Color Strength", Range(-2,2)) = 1.0
        _MainTex("Main Texture", 2D) = "white" {}

        _BumpMap("Normal Map", 2D) = "bump" {}
        _NormalMin("Normal Min", Range(0,2)) = 0.5
        _NormalMax("Normal Max", Range(0,2)) = 0.9
        _NormalSpeed("Normal Speed", Range(0,10)) = 1.0

        _MatCap1("MatCap 1", 2D) = "black" {}
        _MatCap1Color("MatCap 1 Color", Color) = (1,1,1,1)
        _MatCap1Strength("MatCap 1 Strength", Range(0,2)) = 1.0

        _MatCap2("MatCap 2", 2D) = "black" {}
        _MatCap2Color("MatCap 2 Color", Color) = (1,1,1,1)
        _MatCap2Strength("MatCap 2 Strength", Range(0,2)) = 1.0

        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.1,8)) = 2.0
        _RimStrength("Rim Strength", Range(0,1)) = 0.5
        _MatCap2Rotation("MatCap 2 Rotation", Range(0, 360)) = 0
        [Enum(Off,0, On,1)]_ATM("en2", Int) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry"
        }
        LOD 200

        AlphaToMask [_ATM]

        CGPROGRAM
        #pragma surface surf Unlit alpha:fade

        half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MatCap1;
        sampler2D _MatCap2;

        fixed4 _Color;
        half _ColorStrength;
        half _NormalMin;
        half _NormalMax;
        half _NormalSpeed;
        fixed4 _MatCap1Color;
        half _MatCap1Strength;
        fixed4 _MatCap2Color;
        half _MatCap2Strength;
        fixed4 _RimColor;
        half _RimPower;
        half _RimStrength;
        half _MatCap2Rotation;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldNormal;
            float3 viewDir;
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * _ColorStrength;

            float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            // Tạo hiệu ứng dao động giữa _NormalMin và _NormalMax
            half t = (sin(_Time.y * _NormalSpeed) + 1) * 0.5; // Chuyển sin từ [-1,1] về [0,1]
            half dynamicStrength = lerp(_NormalMin, _NormalMax, t);
            normal = normalize(lerp(float3(0, 0, 1), normal, dynamicStrength));

            o.Normal = normal;
            o.Albedo = c.rgb;

            float3 viewDir = normalize(IN.viewDir);
            float3 reflectDir = reflect(-viewDir, normal);
            float2 matcapUV = reflectDir.xy * 0.5 + 0.5;

            fixed4 matCap1 = tex2D(_MatCap1, matcapUV) * _MatCap1Color * _MatCap1Strength;

            float angle = radians(_MatCap2Rotation);
            float sinA = sin(angle);
            float cosA = cos(angle);
            float2 rotatedUV = float2(
                cosA * (matcapUV.x - 0.5) - sinA * (matcapUV.y - 0.5) + 0.5,
                sinA * (matcapUV.x - 0.5) + cosA * (matcapUV.y - 0.5) + 0.5
            );

            fixed4 matCap2 = tex2D(_MatCap2, rotatedUV) * _MatCap2Color * _MatCap2Strength;

            float rim = 1.0 - saturate(dot(viewDir, normal));
            rim = pow(rim, _RimPower) * _RimStrength;
            fixed4 rimColor = _RimColor * rim;

            o.Emission = matCap1.rgb + matCap2.rgb + rimColor.rgb;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Specular"
}