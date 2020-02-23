using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingObject : MonoBehaviour
{
    struct _MeshOpticalProperty
    {
        Vector3 albedo;
        Vector3 Specular;
        float Smoothness;
        Vector3 emission;
    }

    string name;
    _MeshOpticalProperty MeshOpticalProperty;
}
