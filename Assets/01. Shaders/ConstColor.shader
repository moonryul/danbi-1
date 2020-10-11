Shader "danbi/ConstColor" {
  Properties{
      _BaseColor("Base Color", Color) = ( 0, 0, 0, 1 )
  }
    SubShader{
      Tags { "RenderType" = "Opaque" }

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
              float4 vertex : SV_POSITION;
          };

          fixed4 _BaseColor;

          v2f vert(appdata v) {
              v2f o = (v2f)0;
              o.vertex = UnityObjectToClipPos(v.vertex);
              return o;
          }

          fixed4 frag(v2f i) : SV_Target {
            return _BaseColor;
          }
          ENDCG
      } // Pass
  } // SubShader
} // Shader
