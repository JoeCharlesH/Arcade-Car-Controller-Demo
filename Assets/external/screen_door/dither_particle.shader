Shader "Dither/dither_particle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Cull Off Lighting Off ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Dither Functions.cginc"

			half4 _TintColor;

			struct appdata_t {
				float4 vertex: POSITION;
				half4 color: COLOR;
				float2 texcoord: TEXCOORD0;
				half2 offset: TEXCOORD1;
			};

			struct v2f {
				float4 pos: POSITION;
				float2 uv : TEXCOORD0;
				half4 color: COLOR;
				float4 spos: TEXCOORD1;
				half2 offset: TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata_t v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				o.spos = ComputeScreenPos(o.pos);
				o.color = v.color;
				o.offset = v.offset;
				return o;
			}
			
			half4 frag (v2f i) : COLOR {
				// sample the texture
				fixed4 col = i.color * tex2D(_MainTex, i.uv);
				ditherClip((i.spos.xy / i.spos.w) * _ScreenParams.xy + half2(round(i.offset.x), round(i.offset.y)), col.a);
				return col;
			}
			ENDCG
		}
	}
}
