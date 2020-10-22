using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class HemisphereMirrorObject : MonoBehaviour
{
    GameObject MainCameraObj;
    public string objectName;
    public int mirrorType;
    public float unitRadius = 0.08f; //[m]

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

    /// <summary>
    /// Initialised with "object initializer syntax"</object>
    /// </summary>
    [SerializeField, Header("Hemisphere Mirror Parameters"), Space(20)]
    public MeshOpticalProperty MeshOpticalProp = new MeshOpticalProperty
    {
        albedo = new Vector3(0.0f, 0.0f, 0.0f),
        specular = new Vector3(1.0f, 1.0f, 1.0f),
        smoothness = 1.0f,
        emission = new Vector3(0.0f, 0.0f, 0.0f)
    };

    [System.Serializable]
    public struct HemisphereParam
    {
        public float distanceFromCamera;
        public float height;
        public float usedHeight;
        public float bottomDiscRadius;
        public float sphereRadius;
        public float notUsedHeightRatio;
    };


    /// <summary>
    /// Initialized with "object initializer syntax"</object>
    /// </summary>
    [SerializeField, Header("Dome Mirror Parameters"), Space(20)]
    public HemisphereParam HemiSphereParam = new HemisphereParam
    {
        distanceFromCamera = 0.08f,
        height = 0.08f, // 5cm  
        usedHeight = 0.0f,
        bottomDiscRadius = 0.15f, // 15cm
        sphereRadius = 0.0f,
        notUsedHeightRatio = 0.15f,
    };


    void OnEnable() { RayTracingMaster.RegisterHemisphereMirror(this); }

    void OnDisable() { RayTracingMaster.UnregisterHemisphereMirror(this); }

    private void Start()
    {


        Debug.Log("Set the MainCamera GameObject");
        MainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (MainCameraObj == null)
        {
            Debug.Log("MainCamera GameObject not found");

        }

        OnValidate(); // Call this function to set the transform of HemisphereMirrorObject.

    }
    void OnValidate()
    {

        if (MainCameraObj == null)
        {
            return;
        }

        var transFromCameraOrigin = new Vector3(0.0f,
                                                    -(HemiSphereParam.distanceFromCamera + HemiSphereParam.sphereRadius),
                                                    0.0f);
        gameObject.transform.position = MainCameraObj.transform.position + transFromCameraOrigin;  // the center of the hemisphere

        // compute the radius from the height and the bottom disc radius of the dome
        // (r-h) ^2 + (dr)^2 = r^2, where dr = bottom disc radius
        float h = HemiSphereParam.height;
        float dr = HemiSphereParam.bottomDiscRadius;

        HemiSphereParam.sphereRadius = (h * h + (dr * dr)) / (2 * h);
        HemiSphereParam.usedHeight = (1 - HemiSphereParam.notUsedHeightRatio) * h;
        // change the scale of the half sphere mirror
        float scale = HemiSphereParam.sphereRadius / unitRadius;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);


    } // OnValidate()
};