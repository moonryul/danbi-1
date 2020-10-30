Shader "Unlit/2x2PartsTexturesShader" {
    Properties{
        _MainTex0("Base (RGB)", 2D) = "white" {}
    //Added three more textures slots, one for each image  
    _MainTex1("Base (RGB)", 2D) = "white" {}
    _MainTex2("Base (RGB)", 2D) = "white" {}
    _MainTex3("Base (RGB)", 2D) = "white" {}
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert  

    sampler2D _MainTex0;
    //Added three more 2D samplers, one for each additional texture  
    sampler2D _MainTex1;
    sampler2D _MainTex2;
    sampler2D _MainTex3;

    struct Input {
        float2 uv_MainTex0;
    };

    //this variable stores the current texture coordinates multiplied by 2  
    float2 dbl_uv_MainTex0;

    void surf(Input IN, inout SurfaceOutput o) {

        //multiply the current vertex texture coordinate by two  
        dbl_uv_MainTex0 = IN.uv_MainTex0 * 2; // [0,2] x [0,2]

        //doubling the texture coordinates makes all the different images to be rendered 
        //as textures on top of each other, at the bottom left corner of the 3D model.
        // About clamping [we use clamping]  refer to https://www.cs.uregina.ca/Links/class-info/315/WWW/Lab5/

        //To reposition  each image at the bottom left corner of the 3D model in a way
        // that  they create one single image, we offsets to the uv coords. 
        //Then the color data returned by the tex2D() function calls are being stored
        //at the c0, c1, c2 and c3 variables: 

        half4 c0 = tex2D(_MainTex0, dbl_uv_MainTex0 - float2(0.0, 1.0));  //[0,2] x [-1,1]
        half4 c1 = tex2D(_MainTex1, dbl_uv_MainTex0 - float2(1.0, 1.0));  // [-1,1] x [-1,1]
        half4 c2 = tex2D(_MainTex2, dbl_uv_MainTex0);                      // [0,2]x[0,2]
        half4 c3 = tex2D(_MainTex3, dbl_uv_MainTex0 - float2(1.0, 0.0));   // [-1,1] x [0,2]

        //this if statement assures that the input textures won't overlap  
        if (IN.uv_MainTex0.x >= 0.5)
        {
            if (IN.uv_MainTex0.y <= 0.5)
            {
                c0.rgb = c1.rgb = c2.rgb = 0; // only c2 survives
            }
            else
            {
                c0.rgb = c2.rgb = c3.rgb = 0; // only c1 survives
            }
        }
        else
        {
            if (IN.uv_MainTex0.y <= 0.5)
            {
                c0.rgb = c1.rgb = c3.rgb = 0; // only c2 survives
            }
            else
            {
                c1.rgb = c2.rgb = c3.rgb = 0; // only c0 survives
            }
        }

        //sum the colors and the alpha, passing them to the Output Surface 'o'  
        o.Albedo = c0.rgb + c1.rgb + c2.rgb + c3.rgb;
        o.Alpha = c0.a + c1.a + c2.a + c3.a;
    }
    ENDCG
    }
        FallBack "Diffuse"
}

