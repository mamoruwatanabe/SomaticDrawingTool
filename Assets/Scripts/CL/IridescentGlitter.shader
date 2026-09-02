Shader "Custom/IridescentGlitter"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Iridescence ("Iridescence", Range(0,1)) = 0.5
        _GlitterSpeed ("Glitter Speed", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        fixed4 _BaseColor;
        float _Iridescence;
        float _GlitterSpeed;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Unityが自動的に渡す時間変数：_Time
            float2 glitterUV = IN.uv_MainTex + float2(_Time.y * _GlitterSpeed, 0);
            float noise = tex2D(_NoiseTex, glitterUV * 3).r;

            float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), 3.0);
            float3 iridescentColor = lerp(_BaseColor.rgb, float3(1.0, 0.5, 1.0) * noise, fresnel * _Iridescence);

            o.Albedo = iridescentColor;
            o.Metallic = 0.2;
            o.Smoothness = 0.9;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
