

using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ParaboloidMirrorObject : MonoBehaviour {
  public int mMirrorType;


  [System.Serializable]
  public struct MeshOpticalProperty {
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  };

  public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty() {
    albedo = new Vector3(0.0f, 0.0f, 0.0f),
    specular = new Vector3(1.0f, 1.0f, 1.0f),
    smoothness = 1.0f,
    emission = new Vector3(0.0f, 0.0f, 0.0f)
  };


  [System.Serializable]
  public struct ParaboloidParam {
    public float notUseRatio;
    public float distanceFromCamera;
    public float height;
    public float focalLength;
    public float coefficientA;  // z = - ( x^2/a^2 + y^2/b^2)
    public float coefficientB;
  };

  public static float GetCoefficientA(float focalLength) {
    return Mathf.Sqrt(4 * focalLength);
  }

  [SerializeField, Header("Paraboloid Parameters")]
  public ParaboloidParam mParaboloidParam =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                             // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

    new ParaboloidParam {
      notUseRatio = 0.1f,
      distanceFromCamera = 0.3717f,     // 37.17cm
      height = 0.1111f, // 11.11 cm
      focalLength = 0.1f,
      //coefficientA = 0.03f, // 3cm
      //coefficientB = 0.03f
    };



  //Awake() and Start() are called only once per object
  // But OnEnable() can be called everytime  the object is enabled either by another
  // script or Unity; So use Awake() for the absolute initialization purpose

  private void OnEnable() {
    RayTracingMaster.RegisterParaboloidMirror(this);
  }

  private void OnDisable() {
    RayTracingMaster.UnregisterParaboloidMirror(this);
  }

  //This function is called when the script is loaded or a value is changed in the
  // Inspector
  private void OnValidate() {
    mParaboloidParam.coefficientB = mParaboloidParam.coefficientA 
      = ParaboloidMirrorObject.GetCoefficientA(mParaboloidParam.focalLength);
    var transFromCameraOrigin = new Vector3(0.0f, -mParaboloidParam.distanceFromCamera, 0.0f);

    var cameraOrigin = Camera.main.transform.position;

    this.gameObject.transform.position = cameraOrigin + transFromCameraOrigin;

    // Debug.Log("paraboloid transform0=" + this.gameObject.transform.position.ToString("F6"));


  }  //void OnValidate()

}  // class PyramidMirrorObject
