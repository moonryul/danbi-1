using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PanoramaMeshObject : MonoBehaviour
{

    public string objectName;
    public float mHeightOfRangedCylinder = 0.54f; // 54cm

    [System.Serializable]
    public struct MeshOpticalProperty
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    };


    public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty()
    {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = new Vector3(-1.0f, -1.0f, -1.0f)
    };


    [System.Serializable]
    public struct PanoramaMeshParam
    {
        public float highRangeFromCamera;     // relative to the camera
        public float lowRangeFromCamera;

    };


    [SerializeField, Header("Panorama Mesh Parameters"), Space(20)]
    public PanoramaMeshParam mPanoramaMeshParam =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                                   // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

      new PanoramaMeshParam
      {

          highRangeFromCamera = 0.1268f, // 12.68cm
          lowRangeFromCamera = -2.0961f, // -209.61 cm

      };



    void Awake()
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            objectName = gameObject.name;
        }
        RayTracingMaster.RegisterPanoramaMesh(this);

    }

    //void OnEnable() {
    //  RayTracingMaster.RegisterPanoramaMesh(this);
    //}

    void OnDisable()
    {
        RayTracingMaster.UnregisterPanoramaMesh(this);
    }


    //This function is called when the script is loaded or a value is changed in the
    // Inspector
    private void OnValidate()
    {
        Vector3 transFromCameraOrigin = new Vector3(0.0f, mPanoramaMeshParam.lowRangeFromCamera, 0.0f);

        Vector3 cameraOrigin = Camera.main.transform.position;

        this.gameObject.transform.position = cameraOrigin + transFromCameraOrigin;

        //float heightOfRangedCylinder = 0.54f; // 54cm
        float scaleY = (mPanoramaMeshParam.highRangeFromCamera - mPanoramaMeshParam.lowRangeFromCamera)
                       / mHeightOfRangedCylinder;

        // Debug.Log("localScale (before)=" + this.gameObject.transform.localScale);

        this.gameObject.transform.localScale = new Vector3(
                                                   0.99f, scaleY, 0.99f);
        //Debug.Log("localScale (after) =" + this.gameObject.transform.localScale);


        //Debug.Log("paraboloid transform0=" + this.gameObject.transform.position.ToString("F6"));


    }  //void OnValidate()

}