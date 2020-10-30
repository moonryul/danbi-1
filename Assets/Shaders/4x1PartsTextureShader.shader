Shader "Unlit/4x1PartsTexturesShader"
{
	Properties{

	 _MainTex0("Base (RGB)", 2D) = "white" {}
	//Added three more textures slots, one for each image  
	_MainTex1("Base (RGB)", 2D) = "white" {}
	_MainTex2("Base (RGB)", 2D) = "white" {}
	_MainTex3("Base (RGB)", 2D) = "white" {}
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull off
		ZWrite Off

	Pass {

		 CGPROGRAM

		 #pragma vertex vert
		  #pragma fragment frag            
		  #include "UnityCG.cginc"

		  struct appdata {
			  float4 vertex : POSITION;
			  float2 uv : TEXCOORD0;
		  };

		  struct v2f {
			  float2 uv : TEXCOORD0;
			  float4 vertex : SV_POSITION;
		  };


		  int _NumOfTargetTextures;
		  sampler2D _MainTex0;
		  //Added three more 2D samplers, one for each additional texture  
		  sampler2D _MainTex1;
		  sampler2D _MainTex2;
		  sampler2D _MainTex3;


		  //float4 _MainTex_ST;

		  v2f vert(appdata v) {
			  v2f o;
			  o.vertex = UnityObjectToClipPos(v.vertex);

			  //o.uv = TRANSFORM_TEX(v.uv, _MainTex0);
			  o.uv = v.uv;
			  return o;
		  }

		  fixed4 frag(v2f i) : SV_Target{

			fixed4 res;

		  if (_NumOfTargetTextures == 1)
		  {
			  res = tex2D(_MainTex0, float2(i.uv.x, i.uv.y) );
		  }
		  else if (_NumOfTargetTextures == 2)
		  {
			  if (i.uv.x <= 1 / 2.0)
			  {
				  res = tex2D(_MainTex0, float2(i.uv.x * 2, i.uv.y));
			  }
			  else
			  {
				  res = tex2D(_MainTex1, float2(i.uv.x * 2 - 1.0, i.uv.y));

			  }
		  }
		  else if (_NumOfTargetTextures == 3)
		  {
			  if (i.uv.x <= 1 / 3.0)
			  {
				  res = tex2D(_MainTex0, float2(i.uv.x * 3, i.uv.y));
			  }
			  else if (i.uv.x <= 2 / 3.0)
			  {
				  res = tex2D(_MainTex1, float2(i.uv.x * 3 - 1.0, i.uv.y));

			  }

			  else
			  { // 2/4 <  i.uv.x < 3/4; 2 < i.uv.x* 4 < 3;
				  // 0 < i.uv.x* 4 -2.0 < 1
				  res = tex2D(_MainTex2, float2(i.uv.x * 3 - 2.0, i.uv.y));

			  }


		  }

		  else // (_NumOfTargetTextures == 4)
		  {
			  if (i.uv.x <= 1 / 4.0)
			  {
				  res = tex2D(_MainTex0, float2(i.uv.x * 4, i.uv.y));
			  }
			  else if (i.uv.x <= 2 / 4.0)
			  {   // 1/4 < i.uv.x < 1/2 =>  1 < i.uv.x * 4 <2 =>
				  // 0<  i.uv.x * 4 -1.0 <1
				  res = tex2D(_MainTex1, float2(i.uv.x * 4 - 1.0, i.uv.y));

			  }

			  else if (i.uv.x <= 3 / 4.0)
			  { // 2/4 <  i.uv.x < 3/4; 2 < i.uv.x* 4 < 3;
				  // 0 < i.uv.x* 4 -2.0 < 1
				  res = tex2D(_MainTex2, float2(i.uv.x * 4 - 2.0, i.uv.y));

			  }

			  else
			  { // 3/4 <  i.uv.x < 4/4;  3 < i.uv.x* 4 <4
				  // 0 < i.uv.x* 4 -3 <1

				  res = tex2D(_MainTex3, float2(i.uv.x * 4 - 3.0, i.uv.y));
			  }
		  }



			return res;
		  } //  frag(v2f i) 

		  ENDCG
	} // Pass
	} // SubShader
} // Shader

