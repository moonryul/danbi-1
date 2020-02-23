using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangularConeMirrorObject : MonoBehaviour
{
    struct _MeshOpticalProperty
    {
        Vector3 albedo;
        Vector3 Specular;
        float Smoothness;
        Vector3 emission;
    }

    string name;
    [Range(0, 1)]
    int mirrotype;
    _MeshOpticalProperty MeshOpticalProperty;
}
