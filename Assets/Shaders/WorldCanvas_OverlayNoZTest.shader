Shader "UI/Default_OverlayNoZTest" {
  Properties {
   [PreRenderedData] _MainTex("Sprite Texture", 2D) = "white" {}
  _Color  ("Tint", Color) =(1,1,1,1)
  _StencilComp ("Stencil Comparison", Float) = 8
	  _Stencil ("Stencil ID", Float) = 0
	  _StencilOp ("Stencil Operation", Float) = 0
	  _StencilWriteMask ("Stencil Write Mask", Float) = 255
	  _StencilReadMask ("Stencil Read Mask", Float) = 255
	  _ColorMask ("Color Mask" , Float) = 15

  }
    
  SubShader {  
	Tags
  {
	  "Queue" = "Overlay"
	  "IgnoreProjector" = "true"
	  "RenderType" = "Transparent"
	  "PreviewType" = "Plane"
	  "CanUseSpriteAtlas" = "true"
}

Stencil
  {
	  Ref [_Stencil]
	  Comp [_StencilComp]
	  Pass [_StencilOp]
	  ReadMask [_StencilReadMask]
	  WriteMask [_StencilWriteMask]

}
  Cull Off 
  Lighting Off
  ZWrite Off 
  //ZTest Always
  ZTest Off
  
	//Cull Off ZWrite Off ZTest Off
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask [_ColorMask]

    Pass {
      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        
        struct appdata {
          float4 vertex : POSITION;
		  float4 color: COLOR;
          float2 uv : TEXCOORD0;
        };
        
        struct v2f {
          float2 uv : TEXCOORD0;
		  float4 color: COLOR;
          float4 vertex : SV_POSITION;
        };
        
		fixed4 _Color;

        float _SampleCount;
        sampler2D _MainTex;
        float4 _MainTex_ST;
        
        v2f vert(appdata v) {
          v2f o = (v2f)0;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		  o.color = v.color * _Color;
          return o;
        }
        
		float4 frag(v2f i) : SV_Target{

			half4 color = tex2D(_MainTex, i.uv) * i.color;
			clip(color.a - 0.01);
			return color;
          // This shader will now just draw the first sample with an opacity of 1.
          // the next one with 1/2, 1/3, 1/4 and so on, averaging all samples with equal contribution.
          //return float4( tex2D(_MainTex, i.uv).rgb, 1.0 / ( _SampleCount + 1.0 ) );
        }
      ENDCG
    } // Pass
  } // SubShader
} // Shader
