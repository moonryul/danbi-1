using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class GeoConeMirrorObject : MonoBehaviour {

  public string objectName;
  public int mirrorType;

 // public   Camera _camera;
    // MeshOpticalProperty struct is defined in RayTracingObject.cs file
    // outside of the class defined in that file


    [System.Serializable]
    public struct MeshOpticalProperty
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    };

    public MeshOpticalProperty MeshOpticalProp = new MeshOpticalProperty()
    {
        albedo = new Vector3(0.0f, 0.0f, 0.0f),
        specular = new Vector3(1.0f, 1.0f, 1.0f),
        smoothness = 1.0f,
        emission = new Vector3(0.0f, 0.0f, 0.0f)
    };



    [System.Serializable]
    public struct ConeParam
    {
     
        public float distanceFromCamera;
        public float height;
        public float notUseRatio;
        public float radius;

    };



    [SerializeField, Header("Geo Cone Mirror Parameters"), Space(20)]
    public ConeParam mConeParam =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                               // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

      new ConeParam
      {
         
          distanceFromCamera = 0.08f,
          height = 0.05f, // 5cm
          notUseRatio = 0.1f,
          radius = 0.027f, // 2.7cm
      };


    private void OnEnable() {        
    RayTracingMaster.RegisterGeoConeMirror(this);
  }

  private void OnDisable() {
    RayTracingMaster.UnregisterGeoConeMirror(this);
  }

    //This function is called when the script is loaded or a value is changed in the
    // Inspector
    private void OnValidate()
    {

        // set the transform component of the gameObject to which this script
        // component is attachdd

        //_camera= this.gameObject.GetComponent<Camera>();

        Vector3 transFromCameraOrigin = new Vector3(0.0f, -(mConeParam.distanceFromCamera + mConeParam.height), 0.0f);
        
        Vector3 cameraOrigin = Camera.main.transform.position;                      

        this.gameObject.transform.position = cameraOrigin + transFromCameraOrigin;

        //Debug.Log("cone transform0=" + this.gameObject.transform.position.ToString("F6"));


    }  //void OnValidate()
}