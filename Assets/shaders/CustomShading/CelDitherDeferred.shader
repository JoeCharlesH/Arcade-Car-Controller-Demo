// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Cel-Dither" {
Properties {
    _LightTexture0 ("", any) = "" {}
    _LightTextureB0 ("", 2D) = "" {}
    _ShadowMapTexture ("", any) = "" {}
    _SrcBlend ("", Float) = 1
    _DstBlend ("", Float) = 1
}
SubShader {

// Pass 1: Lighting pass
//  LDR case - Lighting encoded into a subtractive ARGB8 buffer
//  HDR case - Lighting additively blended into floating point buffer
Pass {
    ZWrite Off
    Blend [_SrcBlend] [_DstBlend]

CGPROGRAM
#pragma target 3.0
#pragma vertex vert_deferred
#pragma fragment frag
#pragma multi_compile_lightpass
#pragma multi_compile ___ UNITY_HDR_ON

#pragma exclude_renderers nomrt

#include "CustomBRDF.cginc"
#include "UnityCG.cginc"
#include "UnityDeferredLibrary.cginc" 
//#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardBRDF.cginc"

sampler2D _CameraGBufferTexture0;
sampler2D _CameraGBufferTexture1;
sampler2D _CameraGBufferTexture2;

int lSteps;
half stepDelta;
half lowerBound;
half2 pixelDim;
half darkSaturationFactor;
half darkHueFactor;

half3 rgb2hsv(half3 c) {
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

	half d = q.x - min(q.w, q.y);
	half e = 1.0e-10;
	return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half3 hsv2rgb(half3 c) {

	half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

half hueStep(half h) {
	return ((floor(h * 7) / 6) + 0.015h) / 1.015h;
}

half3 adjustedDarkness(half3 color, half ndl) {
	half3 hsv = rgb2hsv(color);
	half targetH = hueStep(hsv.x);
	half level = lSteps * (1 - ndl);
	return hsv2rgb(half3(saturate(lerp(hsv.x, targetH, darkHueFactor * level)), saturate(hsv.y * ((darkSaturationFactor * level) + 1)), hsv.z));
}

half lightStep(half ndl) {
	return floor(ndl * lSteps) / (lSteps - 1); 
}

half indexVal(half2 coord, half val) {
	coord.x = floor(fmod(coord.x, 2));
	coord.y = floor(fmod(coord.y, 2));
	return coord.y;
	int index = coord.x + coord.y * 2;

	if (coord.x >= 8) return val < 0.5? 0.0h : 1.0h;
	else if (index == 0) return 1.00h;
	else if (index == 1) return 0.50h; 
	else if (index == 2) return 0.25h; 
	else return 0.75h; 
}

half ditherNDL (half ndl, half2 uv) {
	//return lightStep(ndl);
	half l1 = lightStep(saturate(ndl - stepDelta));
	half l2 = lightStep(saturate(ndl + stepDelta));
	
	if (l1 - l2 != 0) {
		half diff1 = abs(ndl - l1);
		half range = l2 - l1;

		return (diff1 / range) < indexVal(uv * pixelDim, ndl) ? l1 : l2;
	}
	else return l1;
}

half4 CalculateLight (unity_v2f_deferred i)
{
    float3 wpos;
    float2 uv;
    float atten, fadeDist;
    UnityLight light;
    UNITY_INITIALIZE_OUTPUT(UnityLight, light);
    UnityDeferredCalculateLightParams (i, wpos, uv, light.dir, atten, fadeDist);

    light.color = _LightColor.rgb * atten;

    // unpack Gbuffer
    half4 gbuffer0 = tex2D (_CameraGBufferTexture0, uv);
    half4 gbuffer1 = tex2D (_CameraGBufferTexture1, uv);
    half4 gbuffer2 = tex2D (_CameraGBufferTexture2, uv);
    UnityStandardData data = UnityStandardDataFromGbuffer(gbuffer0, gbuffer1, gbuffer2);

    float3 eyeVec = normalize(wpos-_WorldSpaceCameraPos);
    half oneMinusReflectivity = 1 - SpecularStrength(data.specularColor.rgb);

    UnityIndirect ind;
    UNITY_INITIALIZE_OUTPUT(UnityIndirect, ind);
    ind.diffuse = 0;
    ind.specular = 0;

	half nl = ditherNDL(saturate(dot(data.normalWorld, light.dir)), uv);

	half nh = saturate(dot(data.normalWorld, Unity_SafeNormalize(light.dir - eyeVec)));
	nh = saturate((nh - lowerBound) / (1 - lowerBound));
	nh = ditherNDL(nh, uv);
	nh = ((1 - lowerBound) * nh) + lowerBound;

    half4 res = BRDF_CUSTOM(data.diffuseColor, data.specularColor, oneMinusReflectivity, data.smoothness, data.normalWorld, -eyeVec, light, ind, nl, nh);

    return half4(adjustedDarkness(res.rgb, nl), res.a);
}

#ifdef UNITY_HDR_ON
half4
#else
fixed4
#endif
frag (unity_v2f_deferred i) : SV_Target
{
    half4 c = CalculateLight(i);
    #ifdef UNITY_HDR_ON
    return c;
    #else
    return exp2(-c);
    #endif
}

ENDCG
}


// Pass 2: Final decode pass.
// Used only with HDR off, to decode the logarithmic buffer into the main RT
Pass {
    ZTest Always Cull Off ZWrite Off
    Stencil {
        ref [_StencilNonBackground]
        readmask [_StencilNonBackground]
        // Normally just comp would be sufficient, but there's a bug and only front face stencil state is set (case 583207)
        compback equal
        compfront equal
    }

CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma exclude_renderers nomrt

#include "UnityCG.cginc"

sampler2D _LightBuffer;
struct v2f {
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

v2f vert (float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(vertex);
    o.texcoord = texcoord.xy;
#ifdef UNITY_SINGLE_PASS_STEREO
    o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0f);
#endif
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    return -log2(tex2D(_LightBuffer, i.texcoord));
}
ENDCG
}

}
Fallback Off
}
