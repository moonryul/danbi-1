using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PanoramaMeshObject : MonoBehaviour {
  public string objectName;
  public float mHeightOfRangedCylinder = 0.54f; // 54cm

  [System.Serializable]
  public struct MeshOpticalProperty {
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  };

  public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty() {
    albedo = new Vector3(0.9f, 0.9f, 0.9f),
    specular = new Vector3(0.1f, 0.1f, 0.1f),
    smoothness = 0.9f,
    emission = new Vector3(-1.0f, -1.0f, -1.0f)
  };

  [System.Serializable]
  public struct PanoramaMeshParam {
    public float highRangeFromCamera;     // relative to the camera
    public float lowRangeFromCamera;
  };

  [SerializeField] bool bNewMeasureEnabled = false;

  [SerializeField, Header("Panorama Mesh Parameters"), Space(20)]
  public PanoramaMeshParam mPanoramaMeshParam;

  void Awake() {
    if (string.IsNullOrWhiteSpace(objectName)) {
      objectName = gameObject.name;
    }
    RayTracingMaster.RegisterPanoramaMesh(this);
  }

  void OnDisable() {
    RayTracingMaster.UnregisterPanoramaMesh(this);
  }

  private void OnValidate() {
    var transFromCameraOrigin = new Vector3(0.0f, mPanoramaMeshParam.lowRangeFromCamera, 0.0f);

    transform.position = Camera.main.transform.position + transFromCameraOrigin;
    float scaleY = (mPanoramaMeshParam.highRangeFromCamera - mPanoramaMeshParam.lowRangeFromCamera) / (mHeightOfRangedCylinder);

    // Debug.Log("localScale (before)=" + this.gameObject.transform.localScale);
    transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
    //Debug.Log("localScale (after) =" + this.gameObject.transform.localScale);  
  }  //void OnValidate()
};