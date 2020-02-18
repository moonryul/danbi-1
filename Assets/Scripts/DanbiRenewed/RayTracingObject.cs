using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RayTracingObject : MonoBehaviour
{


    [System.Serializable]
    public struct MeshOpticalProperty
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    };

    public string objectName;

    public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty()
    {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = new Vector3(0.0f, 0.0f, 0.0f)
    };

    void Awake()
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            objectName = gameObject.name;
        }

        RayTracingMaster.RegisterObject(this);
    }

    //void OnEnable() {
    //   RayTracingMaster.RegisterObject(this);
    // }

    void OnDisable()
    {
        RayTracingMaster.UnregisterObject(this);
    }
}