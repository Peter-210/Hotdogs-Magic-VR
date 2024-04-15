

Shader "Custom/CustomGlassRefract" {
    Properties {
	_Refraction("Refraction Depth", Range(0,30)) = 0
    _Cur("'Cursedness'", Range(0,10)) = 0    
    _Reflection("Reflection", Range(0,1)) = 0    
    _Blur("Blurriness", Range(0,10)) = 0    
    _Curse("Cursed Map", 2D) = "white" {}
    _Roughness("Roughness Map", 2D) = "bump" {}    
        
        
}
    SubShader {

   
        Tags { "Queue" = "Transparent" }
        
        GrabPass { }

        
        
        Pass {
         
            
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha // standard alpha blending               

CGPROGRAM
#pragma debug
#pragma vertex vert
#pragma fragment frag 

#pragma multi_compile_instancing

#include "UnityCG.cginc"

sampler2D _GrabTexture : register(s0);
float _Refraction, _Reflection;
float _Blur, _Cur;
sampler2D _Curse, _Roughness;






struct vertexInput {

    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    float4 tangent: TANGENT;
    
     UNITY_VERTEX_INPUT_INSTANCE_ID
};

 

struct v2f {
    
    float4 position : POSITION;
    float4 screenPos : TEXCOORD0;
    half3 worldNormal: TEXCOORD1;
    half3 worldReflection: TEXCOORD2;
    float3 normal : NORMAL;

    //tangent space matrix
    half3 tangentMatR1: TEXCOORD3;
    half3 tangentMatR2: TEXCOORD4;
    half3 tangentMatR3: TEXCOORD5;
    float2 bumpUV: TEXCOORD6;

    float3 worldPos: TEXCOORD7;
    UNITY_VERTEX_OUTPUT_STEREO
};

 

v2f vert(vertexInput input){
    
    v2f vertexOutput;
    UNITY_SETUP_INSTANCE_ID(input);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);
    
    vertexOutput.position = UnityObjectToClipPos(input.vertex);
    vertexOutput.screenPos = vertexOutput.position;
    vertexOutput.worldNormal = UnityObjectToWorldNormal(input.normal);

     ///vertexWorldPos
    float3 vertexPos = mul(unity_ObjectToWorld, input.vertex).xyz;
    vertexOutput.worldPos = vertexPos;
    
    float3 worldViewDirection = normalize(UnityWorldSpaceViewDir(vertexPos));
    float3 worldNormal = UnityObjectToWorldNormal(input.normal);
    vertexOutput.worldReflection = reflect(-worldViewDirection, worldNormal);


    
    half tangentDirection = input.tangent.w * unity_WorldTransformParams.w;
    half3 worldTangent = UnityObjectToWorldDir(input.tangent.xyz);
    half3 worldBiTangent = cross(worldNormal, worldTangent) * tangentDirection;

    vertexOutput.tangentMatR1 = half3(worldTangent.x, worldBiTangent.x, worldNormal.x);
    vertexOutput.tangentMatR2 = half3(worldTangent.y, worldBiTangent.y, worldNormal.y);
    vertexOutput.tangentMatR3 = half3(worldTangent.z, worldBiTangent.z, worldNormal.z);
    vertexOutput.bumpUV = input.uv;
    
    
    return vertexOutput;

}



fixed4 frag( v2f input ) : SV_Target
{

    
    
    float4 col = tex2D(_Curse,input.screenPos.xy);
    float2 screenPos = input.screenPos.xy / input.screenPos.w;
	float depth = _Refraction*0.0005;
    screenPos.x = (screenPos.x + 1) * 0.5;
    screenPos.y = 1-(screenPos.y + 1) * 0.5;
    
    
    half4 sum = half4(0.0h,0.0h,0.0h,0.0h);
    col *= 0.02;
    sum += col;

    //basically we're using a lot of refraction to achieve the bluriness
    for (int i = 0; i < _Refraction; i ++)
    {
        sum += tex2D( _GrabTexture, float2(screenPos.x-i * depth, screenPos.y+i * depth)) * 0.01;
        sum += tex2D( _GrabTexture, screenPos.y * i * depth) * 0.1;
        sum += tex2D( _GrabTexture, screenPos.x * i * depth) * 0.1;

        float noise2 = frac(sin(dot(float2(i, i*2), float2(1000,1000))) * 10) * 6;
        float noise = frac(sin(dot(float2(noise2, noise2 * 3), float2(678,234))) * 10) * 3;
        
        sum += tex2D( _GrabTexture, float2(screenPos.x - _Blur * depth * noise, screenPos.y+ _Blur * depth * (-noise)) ) *0.3;
        sum = sum - tex2D(_Curse, col.x - col.w) * _Cur;
    }


    half3 texNormal = UnpackNormal(tex2D(_Roughness, input.bumpUV));
    half3 worldNormal;
    worldNormal.x = dot(input.tangentMatR1, texNormal);
    worldNormal.y = dot(input.tangentMatR2, texNormal);
    worldNormal.z = dot(input.tangentMatR3, texNormal);

    half3 worldViewDirection = normalize(UnityWorldSpaceViewDir(input.worldPos));
    half3 worldReflection = reflect(-worldViewDirection, worldNormal);
    
    //reflection for the glass
    half4 skybox = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldReflection);
    half3 skyColor = DecodeHDR(skybox, unity_SpecCube0_HDR);

    skyColor *= _Reflection * 2;
    sum /= 2;


    //sum.w
    half4 output = half4(sum.x + skyColor.x, sum.y + skyColor.y, sum.z + skyColor.z , _Reflection);
    output /= 2;
    
    return output;

   // return output;

}
ENDCG
        }
    }

Fallback Off
} 