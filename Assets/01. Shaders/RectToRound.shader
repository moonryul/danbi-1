Shader "Unlit/RectToRound"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Offset("_Offset", float) = 2
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			Cull off
			// inside Pass
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 100
		
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

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}
				float _Offset;
	#define PI 3.141592
				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture

				//float posX = i.uv.x - 0.5f;
				//float posY = i.uv.x - 0.5f;
				//float x;
				//float3 xAxis = float3(1, 0 ,0);
				//
				//if (posX > 0 && posY > 0) // 1
				//{
				//	x = atan2(i.uv.y - 0.5, i.uv.x - 0.5) / (2 * PI);
				//}
				//if (posX < 0 && posY > 0) // 2
				//{
				//	x = atan2(i.uv.y - 0.5, i.uv.x - 0.5) / (2 * PI) + 0.25;
				//}
				//if (posX < 0 && posY < 0) // 3
				//{
				//	x = atan2(i.uv.y - 0.5, i.uv.x - 0.5) / (2 * PI) + 0.5;
				//}
				//if (posX > 0 && posY < 0) // 4
				//{
				//	x = atan2(i.uv.y - 0.5, i.uv.x - 0.5) / (2 * PI) + 0.75;
				//}
					clip(0.25 - (pow(i.uv.x - 0.5,2) + pow(i.uv.y - 0.5,2)));
					float x = atan2(i.uv.y - 0.5, i.uv.x - 0.5);



					float y = sqrt(pow(i.uv.x - 0.5, 2) + pow(i.uv.y - 0.5 , 2))* _Offset;
					clip(1 - y);
					i.uv.x = x;
					i.uv.y = y;
					fixed4 col = tex2D(_MainTex, i.uv);
					/*if (col.r == 0)
						clip(-1);*/
					

					return col;
				}
				ENDCG
			}
		}
}
