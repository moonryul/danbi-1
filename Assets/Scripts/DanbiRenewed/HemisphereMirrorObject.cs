using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class HemisphereMirrorObject : MonoBehaviour
{
    RayDrawer m_RayDrawer;

    
    public int mirrorType;
    public float unitRadius = 0.08f; //[m]

    // MeshOpticalProperty struct was defined in RayTracingObject.cs file
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
    [SerializeField, Header("Hemisphere Optical Properties"), Space(20)]
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
    [SerializeField, Header("Hemisphere Mirror Geometry"), Space(20)]
    public HemisphereParam m_HemisphereParam = new HemisphereParam
    {
        distanceFromCamera = 0.43f,     // 43cm
        height = 0.08f, // 8cm  
        usedHeight = 0.0f,
        bottomDiscRadius = 0.15f, // 15cm
        sphereRadius = 0.0f,
        notUsedHeightRatio = 0.15f,
    };


    public HemisphereParam hemisphereParam { get => m_HemisphereParam; }

    //These functions will be called when the attached GameObject  is toggled.


    void OnEnable() 
    {
        Assert.AreNotEqual(Camera.main, null, "Camera.main should not be null");

        Debug.Log("OnEnable() is called in HemisphereMirrorObject.cs");
        RayTracingMaster.RegisterHemisphereMirror(this);
    }
    //void RegisterHemisphereMirror(HemisphereMirrorObject obj)


    void OnDisable() 
    {
        Debug.Log("OnDisable() is called in  HemisphereMirrorObject.cs "); 
        RayTracingMaster.UnregisterHemisphereMirror(this);
    }

    //Unity Methods execution order:
    //Awake();  OnEnable(); Start(); FixedUpdate(); Update(); LateUpdate()
    //OnDisable() happens when you use SetActive(false) on the object.  
    //OnEnable() is called again on disabled objects when SetActive(true) is called.
    //OnDestroy() is called if you happen to destroy the object with Destroy(gameObject).
    // If the GC does get involved, it will destroy the object, thus forcing the object to receive OnDestroy().
    //FixedUpdate() precedes Update() and LateUpdate() and is used for physics calculations.
    // Update() is used to update object state and other non-physics tasks.
    //LateUpdate() supercedes all other Update() methods.

    //Find methods such as GameObject.Find, Object.FindWithTag, FindObjectOfType, etc
    //    are generally unreliable unless you know what you're doing.
    //    You are checking to make sure the object in question isn't null,
    //    which is good, but if you want a particular object, it will likely fail to find it correctly.
    //    All the find methods which only return a single object instead of a collection will only
    //        return the first available object in the hierarchy where the find condition is true. 
    //    You'll have to revise your design if you want to get a particular object.
    private void Awake()
    {
        Assert.AreNotEqual(Camera.main, null, "Camera.main should not be null");
        Debug.Log("Awake() is called in HemisphereMirrorObject.cs");
        Debug.Log("Initialize in HemisphereMirrorObject.cs");
        Initialize();
    }

    private void Start()
    {
    }  // Start()

    void Initialize()
    {


        // compute the radius from the height and the bottom disc radius of the dome
        // (r-h) ^2 + (dr)^2 = r^2, where dr = bottom disc radius
        float h = this.m_HemisphereParam.height;
        float dr = this.m_HemisphereParam.bottomDiscRadius;

        this.m_HemisphereParam.sphereRadius = (h * h + (dr * dr)) / (2 * h);
        this.m_HemisphereParam.usedHeight = (1 - this.m_HemisphereParam.notUsedHeightRatio) * h;
        // change the scale of the half sphere mirror
        float scale = this.m_HemisphereParam.sphereRadius / unitRadius;
        this.gameObject.transform.localScale = new Vector3(scale, scale, scale);


        var transFromCameraOrigin = new Vector3(0.0f,
                                                    -(this.m_HemisphereParam.distanceFromCamera
                                                    + this.m_HemisphereParam.sphereRadius),
                                                    0.0f);
        this.gameObject.transform.position = Camera.main.transform.position + transFromCameraOrigin;  // the center of the hemisphere
           

    } // Initialize()

    private void OnValidate()
    {
        Assert.AreNotEqual(Camera.main, null, "Camera.main should not be null");
        Debug.Log("HemisphereMirror is changed; update the rays :  in HemisphereMirrorObject.cs");

        m_RayDrawer = this.gameObject.transform.parent.gameObject.GetComponent<RayDrawer>();
        Assert.AreNotEqual(m_RayDrawer, null, "m_RayDrawer should not be null");
        if ( m_RayDrawer.m_HemisphereMirrorObject != null)
        {   // Update the rays only when m_HemisphereMirrorObject gets the reference by Start() of RayDrawer;
            Initialize();
            m_RayDrawer.UpdateRays();
        }
        
        
    }


};