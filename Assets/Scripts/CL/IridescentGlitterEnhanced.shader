Shader "Custom/IridescentGlitterEnhanced"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Iridescence ("Iridescence", Range(0,2)) = 1.0
        _GlitterSpeed ("Glitter Speed", Float) = 1.0
        _GlowIntensity ("Glow Intensity", Float) = 2.0
        _GlitterThreshold ("Glitter Threshold", Range(0,1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        fixed4 _BaseColor;
        float _Iridescence;
        float _GlitterSpeed;
        float _GlowIntensity;
        float _GlitterThreshold;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uvOffset = float2(_Time.y * _GlitterSpeed, _Time.y * 0.3);
            float2 glitterUV = IN.uv_MainTex + uvOffset;

            // ノイズの取得と閾値処理（強い点滅感）
            float noise = tex2D(_NoiseTex, glitterUV * 1).r;
            float glitter = step(_GlitterThreshold, noise); // 0か1

            // Fresnel風 虹色効果
            float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), 3.0);
            float3 rainbow = lerp(float3(1,1,1), float3(1,0.5,1), fresnel * _Iridescence);

            // 輝きの調整：ノイズ × 虹 × ベース × グロー強度
            float3 finalColor = _BaseColor.rgb * rainbow * glitter * _GlowIntensity;

            o.Albedo = finalColor;
            o.Emission = finalColor; // 🔥 自己発光で光らせる！
            o.Metallic = 0.1;
            o.Smoothness = 1.0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
