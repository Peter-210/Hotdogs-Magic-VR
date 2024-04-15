Shader "Custom/ShaderFresnel"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Fresnel("Fresnel Intensity", Range(0,10)) = 0.0
        _Dist("Distance Factor", Float) = 1


    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        sampler2D _MainTex;
        float _Fresnel, _Dist;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            float4 screenPos;
            float4 position;
        
            INTERNAL_DATA
        };

        struct appdata{
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };



        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
          
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            float4 screen = IN.screenPos;

            
            float amount = dot(IN.worldNormal, IN.viewDir);
            float intensity = clamp(pow(1-amount,3),0,1);
            float3 emission = _Fresnel * intensity * _Color;
            float distance = UNITY_Z_0_FAR_FROM_CLIPSPACE(screen.z);
            float distFactor = clamp(pow(_Dist-distance,3), 0, _Dist);
            
            o.Emission = emission * distFactor;
            
        }
        ENDCG
    }
    FallBack "Mobile/Diffuse"
}