Shader "Custom/UnlitGlitterBright"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Iridescence ("Iridescence", Range(0,3)) = 1.5
        _GlitterSpeed ("Glitter Speed", Float) = 2.0
        _GlitterThreshold ("Glitter Threshold", Range(0,1)) = 0.4
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _StarIntensity ("Star Intensity", Range(0,5)) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha One
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            fixed4 _BaseColor;
            float _Iridescence;
            float _GlitterSpeed;
            float _GlitterThreshold;
            float _PulseSpeed;
            float _StarIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv + float2(_Time.y * _GlitterSpeed, 0);
                float noise = tex2D(_NoiseTex, uv * 5).r;
                float glitter = step(_GlitterThreshold, noise);

                float fresnel = pow(1.0 - saturate(dot(i.viewDir, float3(0, 0, 1))), 2.0);
                float3 iridescentColor = lerp(_BaseColor.rgb, float3(0.8, 0.5, 1.0), fresnel * _Iridescence);

                float pulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed + uv.x * 10.0);
                float sparkle = pow(noise * pulse, 6.0) * _StarIntensity;

                float3 finalColor = iridescentColor * glitter * pulse + sparkle;
                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
