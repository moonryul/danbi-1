Shader "danbi/Panorama-4faces"
{
    Properties
    {
        _NumOfTargetTex (" Number of Target Textures", int) = 1
        _MainTex0 ("First Texture Albedo (RGB)", 2D) = "white" {}        
        _MainTex1 ("Second Texture Albedo (RGB)", 2D) = "white" {}        
        _MainTex2 ("Third Texture Albedo (RGB)", 2D) = "white" {}        
        _MainTex3 ("Fourth Texture Albedo (RGB)", 2D) = "white" {}  
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off

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

            int _NumOfTargetTex;

            sampler2D _MainTex0;
            float4 _MainTex0_ST;

            sampler2D _MainTex1;
            float4 _MainTex1_ST;

            sampler2D _MainTex2;
            float4 _MainTex2_ST;

            sampler2D _MainTex3;
            float4 _MainTex3_ST;

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);    
                o.uv = v.uv;            
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 res = (float4)0;
                float uvX = i.uv.x;
                float uvY = i.uv.y;

                if (_NumOfTargetTex == 1)
                {
                    res = tex2D(_MainTex0, uvX);
                }
                else if (_NumOfTargetTex == 2)
                {
                    if (uvX <= 1 / 2.0)
                    {
                        res = tex2D(_MainTex0, float2(uvX * 2, uvY));
                    }
                    else
                    {
                        res = tex2D(_MainTex1, float2(uvX * 2 - 1.0, uvY));
                    }
                }
                else if (_NumOfTargetTex == 3)
                {
                    if (uvX <= 1 / 3.0)
                    {
                        res = tex2D(_MainTex0, float2(uvX * 3, uvY));
                    }
                    else if (uvX <= 2 / 3.0)
                    {
                        res = tex2D(_MainTex1, float2(uvX * 3 - 1.0, uvY));
                    }
                    else
                    {
                        res = tex2D(_MainTex2, float2(uvX * 3 - 2.0, uvY));
                    }
                }
                else // _NumOfTargetTex == 4
                {
                    if (uvX <= 1 / 4.0)
                    {
                        res = tex2D(_MainTex0, float2(uvX * 4, uvY));
                    }
                    else if (uvX <= 2 / 4.0)
                    {
                        res = tex2D(_MainTex1, float2(uvX * 4 - 1.0, uvY));
                    }
                    else if (uvX <= 3 / 4.0)
                    {
                        res = tex2D(_MainTex2, float2(uvX * 4 - 2.0, uvY));
                    }
                    else
                    {
                        res = tex2D(_MainTex3, float2(uvX * 4 - 3.0, uvY));
                    }
                }

                return res;                
            }
            ENDCG
        }
    }
}
