using UnityEngine;

public class PanoramaObject : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    // [SerializeField]
    public float OriginalHeightOfParnoramaMesh = 0.6748f;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField, Header("Mesh Optical Properties")]
    MeshMaterialProperty MeshMaterialProp = new MeshMaterialProperty
    {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = new Vector3(-1.0f, -1.0f, -1.0f)
    };

    public MeshMaterialProperty meshMaterialProp => MeshMaterialProp;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField, Header("Panorama Mesh Parameters")]
    PanoramaParametre Param;

    public PanoramaParametre param => Param;

    void OnReset() {
      OriginalHeightOfParnoramaMesh = 0.6748f;

      MeshMaterialProp = new MeshMaterialProperty {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = new Vector3(-1.0f, -1.0f, -1.0f)
      };
    }

    void Awake() { RayTracingMaster.RegisterPanoramaMesh(this); }

    void OnDisable() { RayTracingMaster.UnregisterPanoramaMesh(this); }

    void OnValidate() {
      // Set the Y position of the Panorama.
      var heightOffset = new Vector3(0.0f, Param.lowRangeFromCamera, 0.0f);
      transform.position = Camera.main.transform.position + heightOffset;

      // 2. scaling the mesh.
      // 새로운 스케일 = (ch 높이 - cl 높이 ) / 원래 메쉬 사이즈(0.6748)
      float newScaleY = (Param.highRangeFromCamera - Param.lowRangeFromCamera) / OriginalHeightOfParnoramaMesh;
      transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
    }
};