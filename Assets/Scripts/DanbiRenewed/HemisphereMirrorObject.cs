using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class HemisphereMirrorObject : MonoBehaviour {
  Camera MainCamera;
  public string objectName;
  public int mirrorType;

  // public   Camera _camera;
  // MeshOpticalProperty struct is defined in RayTracingObject.cs file
  // outside of the class defined in that file


  [System.Serializable]
  public struct MeshOpticalProperty {
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  };

  /// <summary>
  /// Initialised with "object initializer syntax"</object>
  /// </summary>
  [SerializeField, Header("Hemisphere Mirror Parameters"), Space(20)]
  public MeshOpticalProperty MeshOpticalProp = new MeshOpticalProperty {
    albedo = new Vector3(0.0f, 0.0f, 0.0f),
    specular = new Vector3(1.0f, 1.0f, 1.0f),
    smoothness = 1.0f,
    emission = new Vector3(0.0f, 0.0f, 0.0f)
  };

  [System.Serializable]
  public struct HemisphereParam {
    public float distanceFromCamera;
    public float height;
    public float notUseRatio;
    public float radius;
  };


  /// <summary>
  /// Initialized with "object initializer syntax"</object>
  /// </summary>
  [SerializeField, Header("Hemisphere Mirror Parameters"), Space(20)]
  public HemisphereParam HemiSphereParam = new HemisphereParam {
    distanceFromCamera = 0.08f,
    height = 0.05f, // 5cm
    notUseRatio = 0.1f,
    radius = 0.027f, // 2.7cm
  };


  void OnEnable() { RayTracingMaster.RegisterHemisphereMirror(this); }

  void OnDisable() { RayTracingMaster.UnregisterHemisphereMirror(this); }

  void OnValidate() {
    MainCamera = Camera.main;
    if (MainCamera) {
      var transFromCameraOrigin = new Vector3(0.0f, -(HemiSphereParam.distanceFromCamera + HemiSphereParam.radius), 0.0f);
      Vector3 cameraOrigin = MainCamera.transform.position;
      transform.position = cameraOrigin + transFromCameraOrigin;  // the center of the hemisphere
    }
  } // OnValidate()
};