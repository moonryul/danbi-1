using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PanoramaScreenObject : MonoBehaviour {
  GameObject MainCameraObj;
  /// <summary>
  /// Object Name for readability to debugging.
  /// </summary>
  public string ObjectName;

  /// <summary>
  /// 
  /// </summary>
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

  public PanoramaScreenObject() {
    OriginalHeightOfParnoramaMesh = 0.6748f;
    MeshMaterialProp = new MeshMaterialProperty {
      albedo = new Vector3(0.9f, 0.9f, 0.9f),
      specular = new Vector3(0.1f, 0.1f, 0.1f),
      smoothness = 0.9f,
      emission = new Vector3(-1.0f, -1.0f, -1.0f)
    };
  }

  void Awake() {
               
        if (string.IsNullOrWhiteSpace(ObjectName)) {
      ObjectName = gameObject.name;
    }
    RayTracingMaster.RegisterPanoramaMesh(this);
  }

   void Start()
    {

       Debug.Log("Set the MainCamera GameObject");
       MainCameraObj = GameObject.FindGameObjectWithTag("MainCamera"); 
       if (MainCameraObj == null)
        {
            Debug.Log("MainCamera GameObject not found");

        }

        OnValidate(); // call this function to set the transform of PanoramaScreenObject.



    }


  void OnEnable() { RayTracingMaster.RegisterPanoramaMesh(this); }
  void OnDisable() { RayTracingMaster.UnregisterPanoramaMesh(this); }

  void OnValidate()
    {   // This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).

        //https://forum.unity.com/threads/onvalidate-gets-called-at-startup-before-properties-in-other-components-have-deserialized.452658/

        //OnValidate is, I think, not intended to access data in other components, 
        // and as such its execution order should not be relied on.

        //You may be able to work around this by validating on both sides, e.g.:

    if (MainCameraObj == null)
    {
            return;
    }

    var transFromCameraOrigin = new Vector3(0.0f, PanoramaParams.lowRangeFromCamera, 0.0f);
    
    transform.position = MainCameraObj.transform.position + transFromCameraOrigin;
    float scaleY = (PanoramaParams.highRangeFromCamera - PanoramaParams.lowRangeFromCamera) / OriginalHeightOfParnoramaMesh;
    transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
  }

};