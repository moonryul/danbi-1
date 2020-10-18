using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakeTexture : MonoBehaviour
{

    public RenderTexture myRenderTexture;

    public Material postProcessMaterial;

    void OnPreRender()
    {
        myRenderTexture = RenderTexture.GetTemporary(1024, 1024, 16);
    }

    void OnPostRender()
    {
        Graphics.Blit(myRenderTexture, null as RenderTexture, postProcessMaterial, -1);
        postProcessMaterial.SetTexture("_MainTex", myRenderTexture);
        RenderTexture.ReleaseTemporary(myRenderTexture);
    }
}





