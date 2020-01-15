Shader "Yoonsang/ColoredLine"
{
  Properties
  {
    _Color("Color", Color) = (1, 1, 1, 1)
  }
    SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile BLUE_ON RED_ON BLACK_ON PURPLE_ON GREEN_ON YELLOW_ON
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;

      v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
      }

      fixed4 frag(v2f i) : SV_Target
      {
#ifdef BLUE_ON
        return fixed4(0.0f, 0.0f, 1.0f, 1.0f);
#elif RED_ON
        return fixed4(1.0f, 0.0f, 0.0f, 1.0f);
#elif BLACK_ON
        return fixed4(0.0f, 0.0f, 0.0f, 1.0f);
#elif GREEN_ON
        return fixed4(0.0f, 1.0f, 0.0f, 1.0f);
#elif YELLOW_ON
        return fixed4(1.0f, 1.0f, 0.0f, 1.0f);
#elif PURPLE_ON
        return fixed4(1.0f, 0.0f, 1.0f, 1.0f);
#endif
      }
      ENDCG
    }
  }
}
