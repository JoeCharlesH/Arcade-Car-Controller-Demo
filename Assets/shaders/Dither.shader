Shader "Custom/Dither"
{
	Properties{
		lStep("Light Steps", Int) = 4
		lStepDelta("Light Step Delta", Float) = 0.125
		sThreshold("Saturation Threshold", Float) = 0.2
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 scrPos: TEXCOORD0;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			half4 palette[16];
			int paletteSize;
			float lStepDelta;
			float sThreshold;
			int lStep;

			half3 HUEtoRGB(in float H) {
				return saturate(half3(abs(H * 6 - 3) - 1, 2 - abs(H * 6 - 2), 2 - abs(H * 6 - 4)));
			}

			half3 HSLtoRGB(in float3 HSL) {
				half3 RGB = HUEtoRGB(HSL.x);
				half C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
				return (RGB - 0.5) * C + HSL.z;
			}

			half3 RGBtoHCV(in float3 RGB) {
				// Based on work by Sam Hocevar and Emil Persson
				half4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + (1e-10)) + Q.z);
				return float3(H, C, Q.x);
			}

			float3 RGBtoHSL(in float3 RGB) {
				float3 HCV = RGBtoHCV(RGB);
				float L = HCV.z - HCV.y * 0.5;
				float S = HCV.y / (1 - abs(L * 2 - 1) + (1e-10));
				return float3(HCV.x, S, L);
			}

			half indexValue(half2 coord) {
				static const int index[64] = {
					0, 32, 8, 40, 2, 34, 10, 42,
					48, 16, 56, 24, 50, 18, 58, 26,
					12, 44, 4, 36, 14, 46, 6, 38,
					60, 28, 52, 20, 62, 30, 54, 22,
					3, 35, 11, 43, 1, 33, 9, 41,
					51, 19, 59, 27, 49, 17, 57, 25,
					15, 47, 7, 39, 13, 45, 5, 37,
					63, 31, 55, 23, 61, 29, 53, 21
				};

				return index[(int(fmod(coord.x, 8)) + int(fmod(coord.y, 8)) * 8)] / 64.0;
			}

			half hueDistance(float h1, float h2) {
				float diff = abs((h1 - h2));
				return min(abs((1.0 - diff)), diff);
			}

			void closestColors(half hue, out half3 closest, out half3 secondClosest) {
				closest = half3(-2, 0, 0);
				secondClosest = half3(-2, 0, 0);
				half3 paletteColor;
				half currentDistance;

				for (int i = 0; i < paletteSize; ++i) {
					paletteColor = palette[i];
					currentDistance = hueDistance(paletteColor.x, hue);

					if (currentDistance < hueDistance(closest.x, hue)) {
						secondClosest = closest;
						closest = paletteColor;
					}
					else if (currentDistance < hueDistance(secondClosest.x, hue)) {
						secondClosest = paletteColor;
					}
				}
			}

			half lightnessStep(float l) {
				return floor((0.5 + l * lStep)) / lStep;
			}

			half3 dither(half3 color, half2 uv) {
				half3 hsl = RGBtoHSL(color);
				half3 c1, c2;
				half hueDiff;

				if (hsl.y > sThreshold) {
					closestColors(hsl.x, c1, c2);
					hueDiff = hueDistance(hsl.x, c1.x) / hueDistance(c2.x, c1.x);
				}
				else {
					c1 = c2 = half3(0, 0, 1);
					hueDiff = 0.5;
				}

				half l1 = lightnessStep(max((hsl.z - lStepDelta), 0.0));
				half l2 = lightnessStep(min((hsl.z + lStepDelta), 1.0));
				float lightnessDiff = (hsl.z - l1) / (l2 - l1);

				half d = indexValue(uv * _MainTex_TexelSize.zw);
				return HSLtoRGB(half3((hueDiff < d ? c1.xy : c2.xy), (lightnessDiff < d ? l1 : l2)));
			}

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.scrPos = ComputeScreenPos(o.pos);
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) o.scrPos.y = 1 - o.scrPos.y;
				#endif

				return o;
			}

			half3 frag(v2f i) : COLOR {
				return dither(tex2D(_MainTex, i.scrPos.xy).rgb, i.scrPos.xy);
			}
			ENDCG
		}
	}
}
