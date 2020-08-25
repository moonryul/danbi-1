using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PanoramaScreenObject : MonoBehaviour {
  /// <summary>
  /// 
  /// </summary>
  [HideInInspector]
  public float OriginalHeightOfParnoramaMesh;

  /// <summary>
  /// 
  /// </summary>
  [SerializeField, Header("Mesh Optical Properties")]
  MeshMaterialProperty MeshMaterialProp;

  public MeshMaterialProperty meshMaterialProp { get => MeshMaterialProp; set => MeshMaterialProp = value; }

  /// <summary>
  /// 
  /// </summary>
  [SerializeField, Header("Panorama Mesh Parameters")]
  PanoramaParametre param;

  public PanoramaParametre panoramaParams { get => param; set => param = value; }

  Transform MainCamRef;

  Transform mainCamRefInternal {
    get {
      if (MainCamRef.Null()) {
        MainCamRef = Camera.main.transform;
      }
      return MainCamRef;
    }
  }

  public PanoramaScreenObject() {
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
    var heightOffset = new Vector3(0.0f, param.lowRangeFromCamera, 0.0f);
    transform.position = mainCamRefInternal.position + heightOffset;

    // 2. scaling the mesh.
    // 새로운 스케일 = (ch 높이 - cl 높이 ) / 원래 메쉬 사이즈(0.6748)
    float newScaleY = (param.highRangeFromCamera - param.lowRangeFromCamera) / OriginalHeightOfParnoramaMesh;
    transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
  }
};