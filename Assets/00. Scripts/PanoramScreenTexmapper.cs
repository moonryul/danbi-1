using Danbi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PanoramScreenTexmapper : MonoBehaviour
{

    private void Start()
    {
        // panorama is the second child of the dome / cube which is (this.gameObject).
        Transform panorama = this.gameObject.transform.GetChild(1);        
        Renderer render = panorama.GetComponent<Renderer>();
        // cache it for high, low.  
        DanbiCubePanorama panoramaInfo = panorama.GetComponent<DanbiCubePanorama>();

        Vector3 panoramaOrigin = panorama.position;
        // height is the half of the total of high and low.
        Vector3 panoramaCenterPos = panoramaOrigin 
            + new Vector3(0.0f, panoramaInfo.shapeData.high - panoramaInfo.shapeData.low * 0.5f, 0.0f);

        render.material.SetVector("_centerOfMesh", panoramaCenterPos);
    }
}
