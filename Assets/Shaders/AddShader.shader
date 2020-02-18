Shader "Hidden/AddShader" {
  Properties {
    _MainTex("Texture", 2D) = "white" {}
  }
    
  SubShader {    
  Cull Off ZWrite Off ZTest Always
	//Cull Off ZWrite Off ZTest Off
  Blend SrcAlpha OneMinusSrcAlpha

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
        
        float _SampleCount;
        sampler2D _MainTex;
        float4 _MainTex_ST;
        
        v2f vert(appdata v) {
          v2f o = (v2f)0;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = TRANSFORM_TEX(v.uv, _MainTex);
          return o;
        }
        
        float4 frag(v2f i) : SV_Target {
          // This shader will now just draw the first sample with an opacity of 1.
          // the next one with 1/2, 1/3, 1/4 and so on, averaging all samples with equal contribution.
          return float4( tex2D(_MainTex, i.uv).rgb, 1.0 / ( _SampleCount + 1.0 ) );
        }
      ENDCG
    } // Pass
  } // SubShader
} // Shader
