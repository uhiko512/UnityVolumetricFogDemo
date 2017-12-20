Shader "Hidden/ApplyFog"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Tags { "RenderType"="Opaque" }
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Noise.cginc"

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

			float4 _Test;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler3D _Volume;
			float fogFar;
			float cameraFar;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 texelSize = 1.0 / _ScreenParams.xy;
				fixed depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv)).r;
				float4 scattering = tex3D(_Volume, float3(i.uv, saturate((depth * cameraFar) / fogFar)));
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 fog = col * scattering.a + scattering;
				
				return fog;
			}
			ENDCG
		}
	}
}
