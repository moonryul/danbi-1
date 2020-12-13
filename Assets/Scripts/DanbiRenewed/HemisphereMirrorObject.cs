using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class HemisphereMirrorObject : MonoBehaviour
{
    RayDrawer rayDrawer;
    RayTracingMaster rayTracingMaster;
    
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
        public float distanceFromCameraCenter;
        public float distanceFromCameraLens;
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
    public HemisphereParam hemisphereParam = new HemisphereParam
    {
        distanceFromCameraCenter = 0.45f,     // 43cm
        distanceFromCameraLens = 0.327f,     // 43cm
        height = 0.08f, // 8cm  
        usedHeight = 0.0f,
        bottomDiscRadius = 0.15f, // 15cm
        sphereRadius = 0.0f,
        notUsedHeightRatio = 0.15f,
    };


    //These functions will be called when the attached GameObject  is toggled.


    void OnEnable() 
    {
        //Assert.AreNotEqual(Camera.main, null, "Camera.main should not be null");

        //Debug.Log("OnEnable() is called in HemisphereMirrorObject.cs");
        //RayTracingMaster.RegisterHemisphereMirror(this);
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
        
    }

    private void Start()
    {
        Assert.AreNotEqual(Camera.main, null, "Camera.main should not be null");
        Debug.Log("Start() is called in HemisphereMirrorObject.cs");
       

        // RayDrawer component is attached to ranged_cube_screen which is the 2nd child of the
        // full_cube_screen which is the parent of this script component. 
        rayDrawer = this.gameObject.transform.parent.GetChild(2).GetComponent<RayDrawer>();

        if (rayDrawer == null)
        {
            Debug.Log("rayDrawer is not defined in HemisphereMirrorObject.cs");
        }

        // RayTracingMaster component is attached to camera which is the 0st child of the
        // hemisphereMirror + CubeScreen which is the grandparent of this script component. 
        rayTracingMaster = this.gameObject.transform.parent.parent.GetChild(0).GetComponent<RayTracingMaster>();

        if (rayTracingMaster == null)
        {
            Debug.Log("rayTracingMaster is not defined in HemisphereMirrorObject.cs");
        }

        Debug.Log("HemisphereInitialize() is called in Start() in  HemisphereMirrorObject.cs");
        HemisphereInitialize();
      
    }  // Start()

    void HemisphereInitialize()
    {


        // compute the radius from the height and the bottom disc radius of the dome
        // x^2 + y^2 = r^2
        // y = r-h; x^2 + r^2-2rh + h^2 = r^2; x^2 = 2rh - h^2, r > h

        float h = this.hemisphereParam.height;
        float dr = this.hemisphereParam.bottomDiscRadius;  // x^2 = 2 r h -h^2, x = dr
               
        this.hemisphereParam.sphereRadius = (h * h + (dr * dr)) / (2 * h);
        this.hemisphereParam.usedHeight = (1 - this.hemisphereParam.notUsedHeightRatio) * h;
        // change the scale of the half sphere mirror
        float scale = this.hemisphereParam.sphereRadius / unitRadius;
        this.gameObject.transform.localScale = new Vector3(scale, scale, scale);

        Debug.Log($"rayTracingMaster.mDistanceToCenterOfProjection={rayTracingMaster.mDistanceToCenterOfProjection}");


        this.hemisphereParam.distanceFromCameraCenter = rayTracingMaster.mDistanceToCenterOfProjection
                                                 + this.hemisphereParam.distanceFromCameraLens;

        Vector3 hemisphereFromCameraOrigin = new Vector3(0.0f,
                                              -(this.hemisphereParam.distanceFromCameraCenter                                           + this.hemisphereParam.distanceFromCameraLens
                                                 + this.hemisphereParam.sphereRadius),
                                                0.0f);
        this.gameObject.transform.position = Camera.main.transform.position + hemisphereFromCameraOrigin;
        // the center of the hemisphere

        Debug.Log("RegisterHemisphereMirror(this)  is called in Start() in HemisphereMirrorObject.cs");
        RayTracingMaster.RegisterHemisphereMirror(this);

    } // void HemisphereInitialize()

    private void OnValidate()
    {
        Debug.Log("HemisphereInitialize() is called in OnValidate() in  HemisphereMirrorObject.cs");
       // HemisphereInitialize();

       

    }


};