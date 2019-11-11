Shader "Hidden/BoostEffect"
{
	Properties {
		_boostColor ("Boost Color", Color) = (1, 1, 1, 1)
		_colorMix ("Color Mix", Float) = 0.15
		_brightness ("Brightness", Float) = 2
		_contrast ("Contrast", Float) = 0.2
		_blurStrength ("Blur Strength", Float) = 0.5
		_blurWidth ("Blur Width", Float) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform half3 _boostColor;
			uniform half _blurStrength;
			uniform half _blurWidth;
			uniform half _contrast;
			uniform half _brightness;
			uniform half _colorMix;

			half3 radialBlur(half3 color, half2 uv) {
				static const half samples[7] = {-0.01h, 0.005h, 0.02h, 0.035h, 0.05h, 0.065h, 0.08h};
				half3 sum = color;
				half2 dir = half2(0.5h, 0.5h) - uv;
				dir.y = abs(dir.y);
				dir.x /= 5;
				half dist = sqrt((dir.x * dir.x) + (dir.y * dir.y));

				dir /= dist;
				for (int i = 0; i < 7; i++)
					sum += tex2D(_MainTex, uv + (dir * samples[i] * _blurWidth));

				return lerp(color, sum / 8.0h, saturate(dist * _blurStrength));
			}

			void contrastAndBrighten(inout half3 color) {
				color = ((color - 0.5h) * _contrast) + 0.5h;
				color = saturate(color + _brightness);
			}

			half3 getTint(half3 base) {
				static const half3 NTSC = half3(0.299h, 0.587h, 0.114h);
				half grey = dot(base, NTSC);
				return half3(grey, grey, grey) * _boostColor;
			}
			
			half3 frag (v2f_img i) : COLOR {
				//comment
				half3 blurColor = radialBlur(tex2D(_MainTex, i.uv), i.uv);
				contrastAndBrighten(blurColor);

				return lerp(blurColor, getTint(blurColor), _colorMix);
			}
			ENDCG
		}
	}
}
