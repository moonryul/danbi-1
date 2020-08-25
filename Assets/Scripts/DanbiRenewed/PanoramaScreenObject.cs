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
  PanoramaParametre PanoramaParams;

  public PanoramaParametre panoramaParams { get => PanoramaParams; set => PanoramaParams = value; }

  Transform MainCamRef;

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
    // 1. height (y-position)
    // (cl)
    var heightOffset = new Vector3(0.0f, PanoramaParams.lowRangeFromCamera, 0.0f);
    //var heightOffset = new Vector3(0, 0, PanoramaParams.lowRangeFromCamera);
    //mainCamRef = Camera.main.transform;

    if (MainCamRef.Null()) {
      MainCamRef = transform.parent;
    }
    
    var mainCamPos = MainCamRef.position;
    mainCamPos.z= 0.0f;
    mainCamPos.x = 0.0f;
    // Set the Y position of the Panorama.
    transform.position = mainCamPos + heightOffset;

    // 2. scaling the mesh.
    // 새로운 스케일 = (ch 높이 - cl 높이 ) / 원래 메쉬 사이즈(0.6748)
    float newScaleY = (PanoramaParams.highRangeFromCamera - PanoramaParams.lowRangeFromCamera) / OriginalHeightOfParnoramaMesh;
    transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
  }
};