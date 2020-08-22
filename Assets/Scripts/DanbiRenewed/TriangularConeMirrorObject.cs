using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class TriangularConeMirrorObject : MonoBehaviour {

  public string objectName;
  public int mirrorType;

  // public   Camera _camera;
  // MeshOpticalProperty struct is defined in RayTracingObject.cs file
  // outside of the class defined in that file
  // 
  [System.Serializable]
  public struct MeshOpticalProperty {
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  };

  public MeshOpticalProperty MeshOpticalProp = new MeshOpticalProperty() {
    albedo = new Vector3(0.0f, 0.0f, 0.0f),
    specular = new Vector3(1.0f, 1.0f, 1.0f),
    smoothness = 1.0f,
    emission = new Vector3(0.0f, 0.0f, 0.0f)
  };

  [System.Serializable]
  public struct ConeParam {
    public float distanceFromCamera;
    public float originalHeight;
    public float height;
    public float originalRadius;
    public float radius;
  };

  [SerializeField, Header("Triangular Cone Mirror Parameters"), Space(20)]
  public ConeParam mConeParam =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                 // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

    new ConeParam {
      distanceFromCamera = 0.08f,
      originalHeight = 0.0f,
      height = 0.05f, // 5cm
      originalRadius = 0.0f,
      radius = 0.027f, // 2.7cm
    };

  void OnEnable() {
    RayTracingMaster.RegisterTriangularConeMirror(this);
  }

  void OnDisable() {
    RayTracingMaster.UnregisterTriangularConeMirror(this);
  }

  //This function is called when the script is loaded or a value is changed in the
  // Inspector
  void OnValidate() {

    // set the transform component of the gameObject to which this script
    // component is attached

    //_camera= this.gameObject.GetComponent<Camera>();

    //Vector3 transFromCameraOrigin = new Vector3(transform.position.x, -(mConeParam.distanceFromCamera + mConeParam.height), transform.position.z);
    //Vector3 cameraOrigin = Camera.main.transform.position;
    //transform.position = cameraOrigin + transFromCameraOrigin;
    //mConeParam.height = transform.localScale.y * mConeParam.originalHeight;
    //mConeParam.radius = transform.localScale.x * mConeParam.originalRadius;

    //Debug.Log("cone transform0=" + this.gameObject.transform.position.ToString("F6"));


  }  //void OnValidate()
}