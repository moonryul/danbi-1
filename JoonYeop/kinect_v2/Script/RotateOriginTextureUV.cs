using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOriginTextureUV : MonoBehaviour
{
    public float RotateAngleClockWise;
    Material Material_Screen;
    // Start is called before the first frame update
    void Start()
    {
        RotateAngleClockWise = 0;
        Material_Screen = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        Material_Screen.SetFloat("RotateAngleClockWise", RotateAngleClockWise);
    }
}
