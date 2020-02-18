Shader "UI/Default Font Drawn On Top" {
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
	  // Transparent is a transparent queue along with Overlay and all queues after 2500.
	  // Opaque queue = Front to back sorting is applied; The nearer objects are drawn first and 
	  // The ZTest rejects pixels whose depths are already written
	  // Transparent queue = back to front sorting is applied. Those pixels written earlier will bbe
	  // see through those pixels written later; You do not want to use Ztest.
	  

	  "Queue" = "Transparent"
	  "IgnoreProjector" = "True"
	  "RenderType" = "Transparent"
	  "PreviewType" = "Plane"
	
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

			half4 color = i.color;
			color.a *= tex2D(_MainTex, i.uv).a;
			clip(color.a - 0.01);
			return color;
          
        }
      ENDCG
    } // Pass
  } // SubShader
} // Shader
