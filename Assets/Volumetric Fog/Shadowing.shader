Shader "Hidden/Shadowing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			
			float4 frag (v2f i) : SV_Target
			{
				float2 texelSize = 1.0 / _ScreenParams.xy;
				int offset = 6;
				
				float4 depth = 0.0;
				for (int k = -offset; k <= offset; k++) {
					for (int j = -offset; j <= offset; j++) {
						depth += tex2D(_CameraDepthTexture, float2(i.uv.x + j * texelSize.x, i.uv.y + k * texelSize.y)).r;
					}
				}
				depth /= pow(offset * 2 + 1, 2);
				
				return depth;
			}
			ENDCG
		}
		
		/*GrabPass { }
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _GrabTexture;
			
			float4 frag (v2f i) : SV_Target
			{
				float4 depth = tex2D(_GrabTexture,i.uv); 
				
				return depth;
			}
			ENDCG
		}*/
	}
}
