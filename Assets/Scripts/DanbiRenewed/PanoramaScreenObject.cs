using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PanoramaScreenObject : MonoBehaviour
{

    //public RayDrawer rayDrawer;

    public float OriginalHeightOfPanoramaMesh;


    [System.Serializable]
    public struct MeshOpticalProperty
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    };

    [SerializeField, Header("Panorama Screen Material Properties")]
    MeshOpticalProperty MeshOpticalProp = new MeshOpticalProperty
    {
        albedo = new Vector3(0.0f, 0.0f, 0.0f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 1.0f,
        emission = new Vector3(-1.0f, -1.0f, -1.0f)
    };

    public MeshOpticalProperty meshOpticalProp { get => MeshOpticalProp; }

    [System.Serializable]
    public struct PanoramaScreenParam
    {
        [UnityEngine.Header("The Top of the Panorama Image:")]
        public float highRangeFromCamera;
        [UnityEngine.Header("The Bottom of the Panorama Image:")]
        public float lowRangeFromCamera;
    };

    [SerializeField, Header("Panorama Mesh Parameters")]
    public PanoramaScreenParam panoramaScreenParam = new PanoramaScreenParam
    {
        highRangeFromCamera = 0.0f,
        lowRangeFromCamera = -1.2f,
    };


    [SerializeField, Header("The Position of the Topmost Ray:"), Space(20)]
    public float topMostRayPosition = 0.0f; // cm
    [SerializeField, Header("The Position of the BottomMost Ray:"), Space(20)]
    public float bottomMostRayPosition = -2.13f; // 213cm


    // (1) If the initial state of the game object (ie gameObject) is off, t
    //hen the Awake function will not execute when running the program; otherwise,
    //if the initial state of the game object is on, the Awake function will execute.
    //Also, it's worth noting that the execution of the Awake function is not related
    //to the state of the script instance (enabled or disabled),
    //but to the state of the game object to which the script instance is bound.

    //The object (the Awake function has been executed once), close it and open it again, 
    //the Awake function will not execute again.
    //It seems that this corresponds to the case described in the manual,
    // which is only executed once during the entire life cycle of the script instance.

   void Awake() 
    {
        //It's highly advisable to use Awake to initialize your private variables instead of OnValidate. OnValidate is for Editor time to check the values when someone change them in the inspector. Nothing else.
        Debug.Log("Awake() is called in PanoramaScreenObject.cs");
        PanoramaInitialize();
    }

    //  the Start function is only executed when the script instance is enabled; 
    void Start()
    {
    }

    //https://3dmpengines.tistory.com/1729
    //Reflection: https://3dmpengines.tistory.com/1728?category=541869
    //So OnEnable is called just after Awake per object.
    //One object's Awake is not guaranteed to run before another object's OnEnable.

    void OnEnable() {
        Debug.Log("OnEnable() is called in PanoramaScreenObject.cs");
        RayTracingMaster.RegisterPanoramaMesh(this);
    }
    void OnDisable() {
        Debug.Log("OnDisable() is called in PanoramaScreenObject.cs"); 
        RayTracingMaster.UnregisterPanoramaMesh(this); }

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

    private void OnValidate()
    {
        // This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).

        //https://forum.unity.com/threads/onvalidate-gets-called-at-startup-before-properties-in-other-components-have-deserialized.452658/

        //OnValidate is, I think, not intended to access data in other components, 
        // and as such its execution order should not be relied on.

        //You may be able to work around this by validating on both sides, e.g.:

        Debug.Log("************************************************");
        Debug.Log(" onValidate in PanoramaScreenObject: Panorams Screen Params Set or Updated");

        PanoramaInitialize();
    }
    void PanoramaInitialize()
    {   
            //Debug.Log("Camera.main in PanoramaScreenObject=", Camera.main);

            if (Camera.main == null)
            {
                Debug.Log("Camera.main is not defined yet");
                return;
            }

            else
            {
                //Debug.Log("Camera.main is defined =", Camera.main);

            }
            var transFromCameraOrigin = new Vector3(0.0f, this.panoramaScreenParam.lowRangeFromCamera, 0.0f);

            this.gameObject.transform.position = Camera.main.transform.position + transFromCameraOrigin;
            float scaleY = (this.panoramaScreenParam.highRangeFromCamera - this.panoramaScreenParam.lowRangeFromCamera)
                                 / OriginalHeightOfPanoramaMesh;
            this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x, scaleY,
                                                               this.gameObject.transform.localScale.z);
           

    }   //Initialize()

};