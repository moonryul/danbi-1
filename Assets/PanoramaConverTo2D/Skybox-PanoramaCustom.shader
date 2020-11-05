
//https://github.com/Unity-Technologies/SkyboxPanoramicShader

Shader "Skybox/PanoramicCustom" {
	Properties{

		 _Tex("Spherical Panorama", 2D) = "grey" {}

		_CylinderHeight("Cylinder Height", float) = 1
		_CylinderRadius("Cylinder Radius", float) = 1
	}

		SubShader{
			Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
			Cull Off ZWrite Off

			Pass {

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				//#pragma target 2.0 
				/*#pragma multi_compile __ _MAPPING_6_FRAMES_LAYOUT*/

				#include "UnityCG.cginc"

				sampler2D _Tex;


	//ToUVOfEquiRectangularPanorama(resHit.position)
	float2 ToUVOfEquiRectangularPanorama(float3 xyz) // xyz= vertex position -> uv coord 
	{
		///////////////////////////////////////////////////////////////////
		// The position xyz is assumed to be a local position relative to the center of the mesh
		// to be rendered.
		
		float3 normalizedxyz = normalize(xyz);

		float latitude = asin(normalizedxyz.y);
		// asin(y) range: -PI/2 to PI/2

		float longitude = atan2(normalizedxyz.z, normalizedxyz.x);
		// atan2 range : -PI ~ PI        평면상의 사분면을 판단
		// The range of longitude is two times as that of the latitude: The aspect of the panorama image
		// is 2:1
	   //  uv should range from 0 to 1:
		float u = (longitude / UNITY_PI) * 0.5 + 0.5; //u  ranges from 0 to 1;
		float v = (latitude / UNITY_PI) + 0.5; // v ranges from 0 to 1
		//float2 uv = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI); 
		float2 uv = float2(u, v);
		// new latitude  range : 1.0 - 0    = 1  ~  1.0 - 1   = 0  : 0 ~ 1
		// new longitude range : 0.5-(-0.5) = 1  ~  0.5 - 0.5 = 0  : 0 ~ 1
		//   return float2(0.5,1.0) - sphereCoords;  // == image coords corresponde to current vertex (0 ~ 1)
		return uv;

	  	/////////////////////////////////////////////////////////////////////////

	}// ToUVOfEquiRectangularPanorama



				struct appdata_t {
					float4 vertex : POSITION;
					
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float3 texcoord : TEXCOORD0;
		
				};

				v2f vert(appdata_t v)
				{
					v2f o;
						
					o.vertex = UnityObjectToClipPos(v.vertex);
					
					// o.texcoord is used to represent the local position of o.vertex 
					o.texcoord = v.vertex.xyz;
		
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
		
					float2 tc = ToUVOfEquiRectangularPanorama(i.texcoord); // i.texcoord : local vertex
					

					half4 tex = tex2D(_Tex, tc);
					/*half3 c = DecodeHDR(tex, _Tex_HDR);
					c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
					c *= _Exposure;
					return half4(c, 1);*/
					return tex;
				}
				ENDCG
			}
	}


	Fallback Off

}
