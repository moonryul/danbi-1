using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PanoramaScreenObject : MonoBehaviour {
  Camera MainCamera;
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
  [SerializeField, Header("Panorama Screen Material Properties")]
  MeshMaterialProperty MeshMaterialProp =   new MeshMaterialProperty
    {
        albedo = new Vector3(0.0f, 0.0f, 0.0f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 1.0f,
        emission = new Vector3(-1.0f, -1.0f, -1.0f)
    };

public MeshMaterialProperty meshMaterialProp { get => MeshMaterialProp; }
 
    /// <summary>
    /// 
    /// </summary>
 [SerializeField, Header("Panorama Mesh Parameters")]
  PanoramaParams PanoramaParams = new PanoramaParams
    {
        highRangeFromCamera = 0.0f,
        lowRangeFromCamera = -1.2f,
    };


 public PanoramaParams panoramaParams { get => PanoramaParams; }


 //void Awake() {
               
 //       if (string.IsNullOrWhiteSpace(ObjectName)) {
 //     ObjectName = gameObject.name;
 //   }
 //   RayTracingMaster.RegisterPanoramaMesh(this);
 // }

 //void Start()
 //   {


 //   }


  void OnEnable() { RayTracingMaster.RegisterPanoramaMesh(this); }
  void OnDisable() { RayTracingMaster.UnregisterPanoramaMesh(this); }

    //OnValidate is called by Unity on Components whenever a serialized property of
    //that component is changed. That includes "sub-properties" in custom classes.
    //    Unity doesn't serialize custom classes on their own. 
    //    They are simply treated like "structs". 
    //    So all properties of a custom class simply become properties of the MonoBehaviour
    //    class that exposes your custom class.

    //    So when you change a variable in the inspector,
    //    no matter if it's an actual field of the MonoBehaviour or 
    //    if it's nested in a custom class,
    //    Unity will call the OnValidate method of the MonoBehaviour.
    //    If you want to apply special handling in the custom class itself, 
    //    you can simply add your own OnValidate method and 
    //    call it from the MonoBehaviour that contains that class:
      void OnValidate()
    {   // This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).

        //https://forum.unity.com/threads/onvalidate-gets-called-at-startup-before-properties-in-other-components-have-deserialized.452658/

        //OnValidate is, I think, not intended to access data in other components, 
        // and as such its execution order should not be relied on.

        //You may be able to work around this by validating on both sides, e.g.:

    MainCamera = Camera.main;
    if (MainCamera == null)
        {
            Debug.Log("MainCamera is not defined; OnValidate() cannot be performed; return");
            return;
        }
        
    var transFromCameraOrigin = new Vector3(0.0f, PanoramaParams.lowRangeFromCamera, 0.0f);
    
    transform.position = MainCamera.transform.position + transFromCameraOrigin;
    float scaleY = (PanoramaParams.highRangeFromCamera - PanoramaParams.lowRangeFromCamera) / OriginalHeightOfParnoramaMesh;
    transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
  }

};