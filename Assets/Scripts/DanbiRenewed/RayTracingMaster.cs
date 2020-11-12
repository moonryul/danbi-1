//The current code is based on https://bitbucket.org/Daerst/gpu-ray-tracing-in-unity/src/master/
//http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/

using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

using Danbi;

using UnityEngine;
using UnityEngine.UI;
using System.IO;

using System.Runtime.CompilerServices;
//using static Unity.Mathematics.math;  //public static partial class math witin Unity.Mathematics namespace

using Unity.Mathematics;
using TMPro;

// https://github.com/Unity-Technologies/Unity.Mathematics/blob/master/src/Unity.Mathematics/matrix.cs
//https://stackoverflow.com/questions/64185330/error-when-importing-mathematics-in-unity3d



public class RayTracingMaster : MonoBehaviour
{
    string path = "F:/Dropbox/DanbiProject/FinalReport/danbiSceneChanged/debug.txt";
    StreamWriter m_writer;

    public float mCameraHeight = 2.0127f;      // height of chessboard = 0.5cm

    protected bool bCaptureFinished;
    [SerializeField] protected bool UseGPUDebugging = false;
    [SerializeField] protected bool UseMeshDebugging = false;
    [SerializeField] protected bool UseCalibratedCamera = false;
    [SerializeField] protected bool UseLensDistortion = false;
    [SerializeField] protected EDanbiUndistortMode UndistortMode = EDanbiUndistortMode.E_Direct;

    [SerializeField] protected EDanbiMirrorMode MirrorMode = EDanbiMirrorMode.E_HemisphereMirror;

    [SerializeField] protected float ThresholdIterative = 0.01f;
    [SerializeField] protected int SafeCounter = 5;

    [SerializeField] protected float ThresholdNewton = 0.1f;

    [SerializeField, Header("16:9 or 16:10")]
    protected EDanbiScreenAspects TargetScreenAspect = EDanbiScreenAspects.E_16_9;

    [SerializeField, Header("2K(2560 x 1440), 4K(3840 x 2160) or 8K(7680 x 4320)")]
    protected EDanbiScreenResolutions TargetScreenResolution = EDanbiScreenResolutions.E_4K;

    // TODO: Change this variable with Readonly Attribute.
    [SerializeField]
    public Vector2Int CurrentScreenResolutions;

    protected int SizeMultiplier = 1;



    /// <summary>
    /// Panorama object of current simulation set
    /// </summary>
    protected List<PanoramaScreenObject> CurrentPanoramaList = new List<PanoramaScreenObject>();


    [SerializeField, Header("The Number of Target Textures [upto 4]:")]
    protected int NumOfTargetTextures = 1; // It's set on the inspector.                 

    [SerializeField, Header("The Image To Be Projected On Screen:")]
    protected Texture2D TargetPanoramaTex0; // It's set on the inspector.                               

    protected Texture2D targetPanoramaTex0
    {
        get { return TargetPanoramaTex0; }
        set { TargetPanoramaTex0 = value; }
    }


    [SerializeField, Header("The Image To Be Projected On Screen:")]
    protected Texture2D TargetPanoramaTex1; // It's set on the inspector.                               

    protected Texture2D targetPanoramaTex1
    {
        get { return TargetPanoramaTex1; }
        set { TargetPanoramaTex1 = value; }
    }

    [SerializeField, Header("The Image To Be Projected On Screen:")]
    protected Texture2D TargetPanoramaTex2; // It's set on the inspector.                               

    protected Texture2D targetPanoramaTex2
    {
        get { return TargetPanoramaTex2; }
        set { TargetPanoramaTex2 = value; }
    }

    [SerializeField, Header("The Image To Be Projected On Screen:")]
    protected Texture2D TargetPanoramaTex3; // It's set on the inspector.                               

    protected Texture2D targetPanoramaTex3
    {
        get { return TargetPanoramaTex3; }
        set { TargetPanoramaTex3 = value; }
    }

    [SerializeField, Header("Ray-Tracer Compute Shader"), Space(10)]
    public ComputeShader RTShader;       // RayTracingShader.compute

    [SerializeField, Header("The Max Number of Ray Bouncing In Raytracer:")]
    protected int MaxNumOfBounce = 2;

    //[SerializeField, Space(10)]
    //Texture SkyboxTex;

    //[SerializeField]
    //Light DirectionalLight;

    //[Header("Spheres")]
    //public int SphereSeed;
    //public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    //public uint SpheresMax = 100;
    //public float SpherePlacementRadius = 100.0f;

    /// <summary>
    /// used to raytracing to obtain  distorted image and to project the distorted image onto the scene
    /// </summary>
    protected Camera MainCamera;

    [SerializeField, Header("The Script for the Projector:")]
    public DanbiProjector projector; // The reference to DanbiProjector Script Component

    [SerializeField, Header("The Input Field for the Predistorted Image Name:")]
    protected InputField SaveFileInputField;    // this should refer to an input field within the canvas gameobject
                                                // set in the inspector

    [SerializeField, Header("Rendering State = InProgress, Completed:"), Space(20)]
    protected EDanbiRenderingState RenderingState = EDanbiRenderingState.Completed;       // used in projector script

    // processing Button commands

    /// <summary>
    /// it's used to map the Result.
    /// </summary>
    public RenderTexture ResultRenderTex;   // used in projector script

    public RenderTexture ConvergedRenderTexForNewImage;    //used in projector script

    /// <summary>
    /// this refers to the result of projecting the distorted image
    /// </summary>
    protected RenderTexture Dbg_RWTex;

    //protected Texture2D ResultTex1;
    //protected Texture2D ResultTex2;
    //protected Texture2D ResTex3;


    //public Material AddMaterial_WholeSizeScreenSampling;  // defined in projector script
    [SerializeField]
    protected uint CurrentSamplingCountForRendering = 0;
    [SerializeField]
    protected uint MaxSamplingCountForRendering = 5;   // User should experiment with it; used in projector script


    [System.Serializable]
    public struct DanbiExternalCameraParameters
    {
        public float3 xAxis;
        public float3 yAxis;
        public float3 zAxis;
        public float3 translation;
        

    };

    [System.Serializable]        public struct UnityWorldToOpenCVWorld
    {
        public float3 column0;
        public float3 column1;
        public float3 column2;            


    };

    [SerializeField, Space(15)]
    float3x3 UnityToOpenCVMat = new float3x3(
        1, 0, 0,
        0, 0, -1,  
        0, -1, 0); 
    
    


    //[SerializeField, Space(15)]
    //DanbiExternalCameraParameters CameraExternalParameters = new DanbiExternalCameraParameters
    //{             

    //    xAxis = new float3( (float)9.99941183e-01, (float)-8.42821727e-05, (float)1.08454589e-02),
    //    yAxis = new float3( (float)1.41065599e-04, (float)9.99986287e-01, (float)-5.23502838e-03),
    //    zAxis = new float3( (float)-1.08448690e-02, (float) 5.23625039e-03, (float)9.99927483e-01),
    //    translation = new float3( 0,0, (float)2.01277), // the translation of the camera from the 
    //    // world space (chessboard), represented relative to the camera frame.
    //} ;



    [SerializeField, Space(15)]
    DanbiExternalCameraParameters CameraExternalParameters = new DanbiExternalCameraParameters
    {

        xAxis = new float3(1, 0,0),
        yAxis = new float3(0,1,0),
        zAxis = new float3(0,0,1),
        translation = new float3(0, 0, (float)2.01277), // the translation of the camera from the 
        // world space (chessboard), represented relative to the camera frame.
    };

    [SerializeField, Space(15)]
    DanbiCamAdditionalData CameraInternalParameters = new DanbiCamAdditionalData
    {
        RadialCoefficient = new float3( (float)-0.0520976604,(float)0.0699545566,(float)0.16062151),           
        TangentialCoefficient = new float2((float)-0.000560844514, (float) 0.000146228103),
        PrincipalPoint = new float2( (float)1889.51007, (float) 1105.84649), 
        FocalLength = new float2( (float)3066.28926, (float) 3065.59332),   
    };
    protected ComputeBuffer CameraParamsForUndistortImageBuf;

    //[SerializeField, Space(5)]
    //Vector2Int ChessboardWidthHeight;

    //protected List<Transform> TransformListToWatch = new List<Transform>();

    // create a dictionary for DistanceFromCamera, highRange, lowRange  values.

    static bool bObjectsNeedRebuild = false;
    static bool bMeshObjectsNeedRebuild = false;
    static bool bConeMirrorNeedRebuild = false;
    static bool bHemisphereMirrorNeedRebuild = false;

    static bool bPyramidMeshObjectNeedRebuild = false;
    static bool bGeoConeMirrorNeedRebuild = false;
    static bool bParaboloidMeshObjectNeedRebuild = false;

    static bool bPanoramaMeshObjectNeedRebuild = false;

    static List<RayTracingObject> RayTracedObjectsList = new List<RayTracingObject>();
    static List<MeshObject> RayTracedMeshObjectsList = new List<MeshObject>();

    static List<Vector3> VerticesList = new List<Vector3>();
    static List<int> IndicesList = new List<int>();
    static List<Vector2> TexcoordsList = new List<Vector2>();

    // added by Moon Jung
    static List<PyramidMirror> PyramidMirrorsList = new List<PyramidMirror>();
    static List<PyramidMirrorObject> PyramidMirrorObjectsList = new List<PyramidMirrorObject>();

    static List<ParaboloidMirror> ParaboloidMirrorsList = new List<ParaboloidMirror>();
    static List<ParaboloidMirrorObject> ParaboloidMirrorObjectsList = new List<ParaboloidMirrorObject>();

    static List<GeoConeMirror> GeoConeMirrorsList = new List<GeoConeMirror>();
    static List<GeoConeMirrorObject> GeoConeMirrorObjectsList = new List<GeoConeMirrorObject>();

    static List<HemisphereMirrorObject> HemisphereMirrorObjectsList = new List<HemisphereMirrorObject>();
    static List<HemisphereMirror> HemisphereMirrorsList = new List<HemisphereMirror>();

    static List<PanoramaScreen> PanoramaScreensList = new List<PanoramaScreen>();
    static List<PanoramaScreenObject> PanoramaSreenObjectsList = new List<PanoramaScreenObject>();

    static List<TriangularConeMirrorObject> TriangularConeMirrorObjectsList = new List<TriangularConeMirrorObject>();
    static List<TriangularConeMirror> TriangularConeMirrorsList = new List<TriangularConeMirror>();

    ComputeBuffer SphereBuf;
    ComputeBuffer MeshObjectBuf;

    ComputeBuffer VerticesBuf;
    ComputeBuffer IndicesBuf;
    ComputeBuffer TexcoordsBuf;

    ComputeBuffer Dbg_VerticesRWBuf;

    ComputeBuffer PyramidMirrorBuf;
    ComputeBuffer ParaboloidMirrorBuf;
    ComputeBuffer GeoConeMirrorBuf;

    ComputeBuffer PanoramaScreenBuf;

    ComputeBuffer TriangularConeMirrorBuf;
    ComputeBuffer HemisphereMirrorBuf;

    // for debugging
    public ComputeBuffer Dbg_RayDirectionRWBuf;
    public ComputeBuffer Dbg_IntersectionRWBuf;
    public ComputeBuffer Dbg_AccumRayEnergyRWBuf;
    public ComputeBuffer Dbg_EmissionRWBuf;
    public ComputeBuffer Dbg_SpecularRwBuf;
    //
    // ComputeBuffer(int count, int stride, ComputeBufferType type);
    // 
    Vector3[] Deb_VerticeArr;
    Vector4[] Dbg_RayDirectionArr;
    Vector4[] Dbg_IntersectionArr;
    Vector4[] Dbg_AccumulatedRayEnergyArr;
    Vector4[] Dbg_EmissionArr;
    Vector4[] Dbg_SpecularArr;

    //GameObject mInputFieldObj;
    InputField CurrentInputField;
    GameObject CurrentPlaceHolder;



    protected virtual void Start()
    {

        OnValidate();

        Debug.Log("************************************");
        Debug.Log("Now this is a new branch working-calib-2DTextureArray");

        Debug.Log("************************************");

        MainCamera = Camera.main;

        DanbiImage.ScreenResolutions = CurrentScreenResolutions;

#if UNITY_EDITOR
        DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
#endif

        CurrentInputField = SaveFileInputField.gameObject.GetComponent<InputField>();

        CurrentInputField.onEndEdit.AddListener(

        val =>
          {
              if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
              {
                  bCaptureFinished = DanbiImage.CaptureScreenToFileName(//currentSimulatorMode: SimulatorMode,
                                                                    convergedRT: ConvergedRenderTexForNewImage,
                                                                    //distortedResult: out DistortedResultImage,
                                                                    name: CurrentInputField.textComponent.text);
              }
          }
        );

        #region unused
        //MainCamera = gameObject.GetComponent<Camera>();

        // TransformListToWatch.Add(transform);   // mainCamera

        //ResultTex1 = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);
        //ResultTex2 = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);
        //ResTex3 = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);


        // RebuildObjectBuffers();      // Call this function also during Update() if necessary




        //DirectionalLight = GameObject.Find("Sun").GetComponent<Light>();    
        // this.gameObject is the CameraMain gameObject to which the current script "this"
        // is attached. 
        //
        // The 3rd child of the root gameObject is Canvas whose 4th child is InputField
        // You can get references to gameObjects/their components even though they
        // are activated/enabled.
        //
        //mPlaceHolder = mInputFieldObj.transform.GetChild(0).gameObject;    
        //
        //Deactivate the inputField Obj so that it will be popped up when relevant
        //
        //mInputFieldObj.SetActive(false);
        //
        //GameObject placeHolder = mInputFieldObj.transform.GetChild(0).gameObject;
        // 
        //mPlaceHolder.SetActive(false);
        //
        // disable Canvas gameObject so that it will show relevant menu items when 
        // the  scene objects are registered.  It will be activated in Start() method
        // which is called after all Awake() methods of the object registering scripts are 
        // executed; The objects are registered by the Awake() methods of these scripts
        //
        //mCanvasObj = GameObject.FindWithTag("Canvas");
        //Debug.Log("Canvas =" + mCanvasObj);
        //
        ////mCanvasObj.SetActive(false);  // deactivate the canvas gameobject initially
        //Canvas canvas = mCanvasObj.GetComponent<Canvas>();
        //canvas.enabled = false;
        // 
        //kernelCreateImageTriConeMirror = CurrentRayTracerShader.FindKernel("CreateImageTriConeMirror");
        //kernelCreateImageGeoConeMirror = CurrentRayTracerShader.FindKernel("CreateImageGeoConeMirror");
        //kernelCreateImageParaboloidMirror = CurrentRayTracerShader.FindKernel("CreateImageParaboloidMirror");
        //kernelCreateImageHemisphereMirror = CurrentRayTracerShader.FindKernel("CreateImageHemisphereMirror");
        //kernelProjectImageTriConeMirror = CurrentRayTracerShader.FindKernel("ProjectImageTriConeMirror");
        //kernelProjectImageGeoConeMirror = CurrentRayTracerShader.FindKernel("ProjectImageGeoConeMirror");
        //kernelProjectImageParaboloidMirror = CurrentRayTracerShader.FindKernel("ProjectImageParaboloidMirror");
        //kernelProjectImageHemisphereMirror = CurrentRayTracerShader.FindKernel("ProjectImageHemisphereMirror");
        //kernelViewImageOnPanoramaScreen = CurrentRayTracerShader.FindKernel("ViewImageOnPanoramaScreen");
        //
        // CameraUser = GameObject.FindWithTag("CameraUser").GetComponent<Camera>();
        //
        //TransformListToWatch.Add(DirectionalLight.transform);
        //Validator = GetComponent<RTRayDirectionValidator>(); 
        //
        #endregion
    }    //void Start()

    protected virtual void OnDisable()
    {
        SphereBuf?.Release();
        MeshObjectBuf?.Release();
        VerticesBuf?.Release();
        IndicesBuf?.Release();

        Dbg_IntersectionRWBuf?.Release();
        Dbg_AccumRayEnergyRWBuf?.Release();
        Dbg_EmissionRWBuf?.Release();
        Dbg_SpecularRwBuf?.Release();
    }

    protected virtual void Update()
    {

        CurrentSamplingCountForRendering = projector.CurrentSamplingCountForRendering;    // value



        //Debug.Log(" I am here in Update() of RayTacingMaster.cs");
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Utils.QuitEditorManually();
            //m_writer.Close();
        }


        //if ( Input.GetKeyDown(KeyCode.Space) ) // Print the GPU debug message
        //{


        //    //Write some text to the test.txt file
        //    m_writer = new StreamWriter(path);

        //    // writer.WriteLine("Test");
        //    // writer.Close();
        //    //sr.WriteLine ("Time and Vector3 coordinates are: {0},{1},{2} and {3}", t, x, y, z);
        //    Debug.Log("***************************************");
        //    Debug.Log("***************************************");
        //    Debug.Log("Start GPU Debug to file");
        //    DebugLogOfRWBuffers();
        //    Debug.Log("Finish GPU Debug to file");
        //    Debug.Log("***************************************");
        //    Debug.Log("***************************************");

        //    m_writer.Close();

        //}

        #region unused codes

        //foreach (var t in TransformListToWatch)
        //{
        //    if (t.hasChanged)
        //    {
        //        projector.CurrentSamplingCountForRendering = 0;
        //        projector.RenderingState = EDanbiRenderingState.InProgress;

        //        // restart the ray tracing in Projector script  when these transforms have been changed
        //        t.hasChanged = false;
        //    }
        //}


        /*
        if (_cameraMain.fieldOfView != _lastFieldOfView)
        {
            _currentSample = 0;
            _lastFieldOfView = _cameraMain.fieldOfView;
        }*/

        /*
         these were used to implement to change parameter and geometries at runtime.
         RebuildObjectBuffersWithoutMirror();
         InitCreatePreDistortedImage();
         SetShaderFrameParameters();  // parameters need to be set every frame*/
        #endregion
    }    //void Update()

    #region Register|Unregister
    public static void RegisterObject(RayTracingObject obj)
    {
        Debug.Log("Raytracing Object registered");

        RayTracedObjectsList.Add(obj);
        bMeshObjectsNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }
    public static void UnregisterObject(RayTracingObject obj)
    {
        RayTracedObjectsList.Remove(obj);
        bMeshObjectsNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void RegisterTriangularConeMirror(TriangularConeMirrorObject obj)
    {
        Debug.Log("Triangular Cone Mirror registered");
        TriangularConeMirrorObjectsList.Add(obj);
        bConeMirrorNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void UnregisterTriangularConeMirror(TriangularConeMirrorObject obj)
    {
        TriangularConeMirrorObjectsList.Remove(obj);
        bConeMirrorNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void RegisterPyramidMirror(PyramidMirrorObject obj)
    {
        Debug.Log("Pyramid Mirror registered");
        PyramidMirrorObjectsList.Add(obj);
        bPyramidMeshObjectNeedRebuild = true;
        // bObjectsNeedRebuild = true;
    }

    public static void UnregisterPyramidMirror(PyramidMirrorObject obj)
    {
        PyramidMirrorObjectsList.Remove(obj);
        bPyramidMeshObjectNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void RegisterParaboloidMirror(ParaboloidMirrorObject obj)
    {
        Debug.Log("Paraboloid Mirror registered");
        ParaboloidMirrorObjectsList.Add(obj);
        bParaboloidMeshObjectNeedRebuild = true;
        // bObjectsNeedRebuild = true;
    }

    public static void UnregisterParaboloidMirror(ParaboloidMirrorObject obj)
    {
        ParaboloidMirrorObjectsList.Remove(obj);
        bParaboloidMeshObjectNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void RegisterHemisphereMirror(HemisphereMirrorObject obj)
    {
        Debug.Log("Hemisphere Mirror registered");
        //iList.IndexOf(value) == -1
        //HemisphereMirrorObjectsList.Add(obj);   // Add the reference obj to the list
        if (HemisphereMirrorObjectsList.Count == 0)
        {
            HemisphereMirrorObjectsList.Add(obj);
            bHemisphereMirrorNeedRebuild = true;
            // bObjectsNeedRebuild = true;
        }
        else   // The Count is 1; The list can contain only one item.        
        {
            HemisphereMirrorObjectsList.RemoveAt(0);   // remove the item from the index 0
            HemisphereMirrorObjectsList.Add(obj);
            bHemisphereMirrorNeedRebuild = true;
            // bObjectsNeedRebuild = true;

        }
    }

    public static void UnregisterHemisphereMirror(HemisphereMirrorObject obj)
    {
        HemisphereMirrorObjectsList.Remove(obj);
        bHemisphereMirrorNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void RegisterGeoConeMirror(GeoConeMirrorObject obj)
    {
        Debug.Log("Geometric Cone Mirror registered");
        GeoConeMirrorObjectsList.Add(obj);
        bGeoConeMirrorNeedRebuild = true;
        //bObjectsNeedRebuild = true;
    }

    public static void UnregisterGeoConeMirror(GeoConeMirrorObject obj)
    {
        GeoConeMirrorObjectsList.Remove(obj);
        bGeoConeMirrorNeedRebuild = true;
        // bObjectsNeedRebuild = true;
    }

    public static void RegisterPanoramaMesh(PanoramaScreenObject obj)
    {
        Debug.Log("panorama Mesh registered");
        if (PanoramaSreenObjectsList.Count == 0)
        {
            PanoramaSreenObjectsList.Add(obj);
            bPanoramaMeshObjectNeedRebuild = true;
            // bObjectsNeedRebuild = true;
        }
        else
        {
            PanoramaSreenObjectsList.RemoveAt(0);
            PanoramaSreenObjectsList.Add(obj);
            bPanoramaMeshObjectNeedRebuild = true;
            //  bObjectsNeedRebuild = true;
        }
    }

    public static void UnregisterPanoramaMesh(PanoramaScreenObject obj)
    {
        PanoramaSreenObjectsList.Remove(obj);
        bPanoramaMeshObjectNeedRebuild = true;
        // bObjectsNeedRebuild = true;
    }
    #endregion

    #region SetupScene
    //void SetUpScene() {
    //  Random.InitState(SphereSeed);
    //  var spheres = new List<Sphere>();

    //  // Add a number of random spheres
    //  for (int i = 0; i < SpheresMax; i++) {
    //    var sphere = new Sphere();

    //    // Radius and radius
    //    sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
    //    Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
    //    sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

    //    // Reject spheres that are intersecting others
    //    foreach (Sphere other in spheres) {
    //      float minDist = sphere.radius + other.radius;
    //      if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist) {
    //        goto SkipSphere;
    //      }
    //    }

    //    // Albedo and specular color
    //    Color color = Random.ColorHSV();
    //    float chance = Random.value;
    //    if (chance < 0.8f) {
    //      bool metal = chance < 0.4f;
    //      sphere.albedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
    //      sphere.specular = metal ? new Vector4(color.r, color.g, color.b) : new Vector4(0.04f, 0.04f, 0.04f);
    //      sphere.smoothness = Random.value;
    //    } else {
    //      Color emission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
    //      sphere.emission = new Vector3(emission.r, emission.g, emission.b);
    //    }

    //    // Add the sphere to the list
    //    spheres.Add(sphere);

    //  SkipSphere:
    //    continue;
    //  }

    //  // Assign to compute buffer
    //  if (_sphereBuffer != null) {
    //    _sphereBuffer.Release();
    //  }

    //  if (spheres.Count > 0) {
    //    _sphereBuffer = new ComputeBuffer(spheres.Count, 56);
    //    _sphereBuffer.SetData(spheres);
    //  }
    //}   //void SetUpScene()

    #endregion

    /// <summary>
    /// need to transfer this to the dbg class.
    /// </summary>
    protected void CreateDebugBuffers()
    {
        //// for debugging
        //_meshObjectArray = new MeshObjectRW[_meshObjects.Count];

        //int meshObjRWStride = 16 * sizeof(float) + sizeof(float)
        //               + 3 * 3 * sizeof(float);

        //_meshObjectBufferRW = new ComputeBuffer(_meshObjects.Count, meshObjRWStride);

        ////ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.

        Dbg_VerticesRWBuf = new ComputeBuffer(VerticesList.Count, 3 * sizeof(float), ComputeBufferType.Default);
        Dbg_IntersectionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
        Dbg_RayDirectionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
        Dbg_IntersectionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
        Dbg_AccumRayEnergyRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
        Dbg_EmissionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
        Dbg_SpecularRwBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);

        Deb_VerticeArr = new Vector3[VerticesList.Count];
        Dbg_RayDirectionArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
        Dbg_IntersectionArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
        Dbg_AccumulatedRayEnergyArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
        Dbg_EmissionArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
        Dbg_SpecularArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];

        ////The static Array.Clear() method "sets a range of elements in the Array to zero, to false, or to Nothing, depending on the element type".If you want to clear your entire array, you could use this method an provide it 0 as start index and myArray.Length as length:
        //// Array.Clear(mUVMapArray, 0, mUVMapArray.Length);
        //// _meshObjectBufferRW.SetData(_meshObjectArray);

        Dbg_VerticesRWBuf.SetData(Deb_VerticeArr);
        Dbg_RayDirectionRWBuf.SetData(Dbg_RayDirectionArr);
        Dbg_IntersectionRWBuf.SetData(Dbg_IntersectionArr);
        Dbg_AccumRayEnergyRWBuf.SetData(Dbg_AccumulatedRayEnergyArr);
        Dbg_EmissionRWBuf.SetData(Dbg_EmissionArr);
        Dbg_SpecularRwBuf.SetData(Dbg_SpecularArr);
    }

    #region  Rebuild buffers
    //RebuildObjectBuffers();      // Call this function also during Update() if necessary

    void RebuildObjectBuffers()
    {
        //if (!bObjectsNeedRebuild)
        //{
        //    Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
        //    Debug.Log("The mesh objects are already built");
        //    Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");

        //    return;
        //}


        //bObjectsNeedRebuild = false;

        // Clear the list of vertices, indices, and texture coordinates of all the meshes in the scene

        VerticesList.Clear();
        IndicesList.Clear();
        TexcoordsList.Clear();

        bool mirrorDefined = false;

        //MOON: you need to define the buffer data whether you use it or not 
        // in the compute shader

        CreateComputeBuffer<DanbiCamAdditionalData>(ref CameraParamsForUndistortImageBuf,
                                                  new List<DanbiCamAdditionalData>() { CameraInternalParameters },
                                                  40);

        if (TriangularConeMirrorObjectsList.Count != 0)
        {
            RebuildTriangularConeMirrorBuffer();
            mirrorDefined = true;
        }
        else if (GeoConeMirrorObjectsList.Count != 0)
        {
            RebuildGeoConeMirrorBuffer();
            mirrorDefined = true;
        }
        else if (ParaboloidMirrorObjectsList.Count != 0)
        {
            RebuildParaboloidMirrorBuffer();
            mirrorDefined = true;
        }
        else if (HemisphereMirrorObjectsList.Count != 0)
        {
            RebuildHemisphereMirrorBuffer();
            mirrorDefined = true;
        }
        // Either panoramaScreenObject or panoramaMeshObject should be defined
        // so that the projector image will be projected onto it.

        else // mirrorDefined == false
        {
            Debug.LogError("A mirror should be defined");
            Utils.StopPlaying();
        }

        if (PanoramaSreenObjectsList.Count != 0)
        {
            RebuildPanoramaMeshBuffer();
        }
        else
        {
            Debug.LogError(" panoramaMeshObject should be defined\n" +
                           "so that the projector image will be projected onto it.");
            Utils.StopPlaying();
        }

        //if (_meshObjects.Count != 0) // The meshes other than the panorama screen mesh
        if (RayTracedObjectsList.Count != 0)
        {
            RebuildMeshObjectBuffer();
        }

        // create computeBuffers holding the vertices information about the various
        // objects created by the above RebuildXBuffer()'s
        CreateComputeBuffer(buffer: ref VerticesBuf, data: VerticesList, stride: 12);
        CreateComputeBuffer(buffer: ref IndicesBuf, data: IndicesList, stride: 4);
        CreateComputeBuffer(buffer: ref TexcoordsBuf, data: TexcoordsList, stride: 8);

        //#region debug
        //if (UseGPUDebugging)
        //{
        //    CreateDebugBuffers();
        //}
        //#endregion
    }  // RebuildObjectBuffers()

    void RebuildMeshObjectBuffer()   // other than the mirror and the panorama screen mesh
    {
        //    if (!bMeshObjectsNeedRebuild)
        //    {
        //        return;
        //    }


        //    bMeshObjectsNeedRebuild = false;

        //// Clear all lists

        RayTracedMeshObjectsList.Clear();


        // Loop over all objects and gather their data

        foreach (var obj in RayTracedObjectsList)
        {

            string objectName = obj.objectName;


            var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            // Add vertex data  to the vertex list
            // get the current number of vertices in the vertex list
            int currentVertexOffset = VerticesList.Count;  // The number of vertices so far created; will be used
                                                           // as the index of the first vertex of the newly created mesh
            VerticesList.AddRange(mesh.vertices);

            // Add index data - if the vertex buffer wasn't empty before, the
            // indices need to be offset
            int currentIndexOffset = IndicesList.Count; // the current count of _indices  list; will be used
                                                        // as the index offset in _indices for the newly created mesh
            int[] meshVertexIndices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                                          // when the mesh topology is triangle;
                                                          // indices will contain a multiple of three indices
                                                          // our mesh is actually a triangular mesh.

            if (UseMeshDebugging)
            {
                Debug.Log("mesh object=" + objectName);

                // show the local coordinates of the triangles
                for (int i = 0; i < meshVertexIndices.Length; i += 3)
                {   // a triangle v0,v1,v2 

                    Debug.Log("triangle vertex (local) =(" + mesh.vertices[meshVertexIndices[i]].ToString("F6")
                              + "," + mesh.vertices[meshVertexIndices[i + 1]].ToString("F6")
                              + "," + mesh.vertices[meshVertexIndices[i + 2]].ToString("F6") + ")");
                }


            } //    if (UseMeshDebugging)

            IndicesList.AddRange(meshVertexIndices.Select(index => index + currentVertexOffset));


            // Add Texcoords data.
            TexcoordsList.AddRange(mesh.uv);

            // Add the object itself

            if (RayTracedMeshObjectsList.Count == 0)
            {
                RayTracedMeshObjectsList.Add(
                new MeshObject()
                {
                    localToWorldMatrix = obj.transform.localToWorldMatrix,
                    albedo = obj.MeshOpticalProp.albedo,

                    specular = obj.MeshOpticalProp.specular,
                    smoothness = obj.MeshOpticalProp.smoothness,
                    emission = obj.MeshOpticalProp.emission,

                    indices_offset = currentIndexOffset,
                    indices_count = meshVertexIndices.Length // set the index count of the mesh of the current obj
                }
                );
            }

            else
            {
                RayTracedMeshObjectsList[0] = new MeshObject()
                {
                    localToWorldMatrix = obj.transform.localToWorldMatrix,
                    albedo = obj.MeshOpticalProp.albedo,

                    specular = obj.MeshOpticalProp.specular,
                    smoothness = obj.MeshOpticalProp.smoothness,
                    emission = obj.MeshOpticalProp.emission,

                    indices_offset = currentIndexOffset,
                    indices_count = meshVertexIndices.Length // set the index count of the mesh of the current obj
                };
            }


        }// foreach (RayTracingObject obj in _rayTracingObjects)



        //    struct MeshObject
        //{
        //    public Matrix4x4 localToWorldMatrix;
        //    public Vector3 albedo;
        //    public Vector3 specular;
        //    public float smoothness;
        //    public Vector3 emission;
        //    public int indices_offset;
        //    public int indices_count;
        //}

        int meshObjStride = 16 * sizeof(float)
                        + 3 * 3 * sizeof(float) + sizeof(float) + 2 * sizeof(int) - 4;

        // create a computebuffer and set the data to it
        // If _meshObjects.Count ==0 ==> the computeBuffer is not created.

        CreateComputeBuffer(ref MeshObjectBuf, RayTracedMeshObjectsList, meshObjStride);
    }   // RebuildMeshObjectBuffer()

    void RebuildTriangularConeMirrorBuffer()
    {
        // if obj.mirrorType is the given mirrorType
        //if (!bConeMirrorNeedRebuild)
        //{
        //    return;
        //}

        //bConeMirrorNeedRebuild = false;

        // Clear all lists
        TriangularConeMirrorsList.Clear();


        var obj = TriangularConeMirrorObjectsList[0];

        string objectName = obj.objectName;

        var mesh = obj.GetComponent<MeshFilter>().sharedMesh;


        // Add vertex data
        // get the current number of vertices in the vertex list
        int currentVertexOffset = VerticesList.Count;  // The number of vertices so far created; will be used
                                                       // as the index of the first vertex of the newly created mesh
        VerticesList.AddRange(mesh.vertices);
        // Add Texcoords data.
        TexcoordsList.AddRange(mesh.uv);

        // Add index data - if the vertex buffer wasn't empty before, the
        // indices need to be offset
        int currentIndexOffset = IndicesList.Count; // the current count of _indices  list; will be used
                                                    // as the index offset in _indices for the newly created mesh
        int[] meshVertexIndices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                                      // when the mesh topology is triangle;
                                                      // indices will contain a multiple of three indices
                                                      // our mesh is actually a triangular mesh.


        if (UseMeshDebugging)
        {
            Debug.Log("mirror mesh object=" + objectName);

            // show the local coordinates of the triangles
            for (int i = 0; i < meshVertexIndices.Length; i += 3)
            {   // a triangle v0,v1,v2 

                Debug.Log("triangle vertex (local) =(" + mesh.vertices[meshVertexIndices[i]].ToString("F6")
                          + "," + mesh.vertices[meshVertexIndices[i + 1]].ToString("F6")
                          + "," + mesh.vertices[meshVertexIndices[i + 2]].ToString("F6") + ")");
            }


        } //    if (UseMeshDebugging)



        IndicesList.AddRange(meshVertexIndices.Select(index => index + currentVertexOffset));


        // Add Texcoords data.
        //_texcoords.AddRange(mesh.uv);

        // Add the object itself
        if (TriangularConeMirrorsList.Count == 0)
        {
            TriangularConeMirrorsList.Add(
            new TriangularConeMirror()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,

                distanceToOrigin = obj.mConeParam.distanceFromCamera,
                height = obj.mConeParam.height,
                radius = obj.mConeParam.radius,
                albedo = obj.MeshOpticalProp.albedo,

                specular = obj.MeshOpticalProp.specular,
                smoothness = obj.MeshOpticalProp.smoothness,
                emission = obj.MeshOpticalProp.emission,
                indices_offset = currentIndexOffset,
                indices_count = meshVertexIndices.Length // set the index count of the mesh of the current obj
            }
            );
        }
        else
        {
            TriangularConeMirrorsList[0] = new TriangularConeMirror()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,

                distanceToOrigin = obj.mConeParam.distanceFromCamera,
                height = obj.mConeParam.height,
                radius = obj.mConeParam.radius,
                albedo = obj.MeshOpticalProp.albedo,

                specular = obj.MeshOpticalProp.specular,
                smoothness = obj.MeshOpticalProp.smoothness,
                emission = obj.MeshOpticalProp.emission,
                indices_offset = currentIndexOffset,
                indices_count = meshVertexIndices.Length // set the index count of the mesh of the current obj
            };
        }





        //int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
        //             + 5 * sizeof(float) + 2 * sizeof(int) - 4;     // by YoonSang

        int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                       + 5 * sizeof(float) + 2 * sizeof(int);

        // create a computebuffer and set the data to it

        CreateComputeBuffer(ref TriangularConeMirrorBuf, TriangularConeMirrorsList, stride);
    }   // RebuildTriangularConeMirrorBuffer()

    void RebuildHemisphereMirrorBuffer()
    {
        //    // if obj.mirrorType is the given mirrorType
        //    if (!bHemisphereMirrorNeedRebuild)
        //    {
        //        return;
        //    }

        //    bHemisphereMirrorNeedRebuild = false;

        // Clear the hemisphereMirror List 
        HemisphereMirrorsList.Clear();

        var obj = HemisphereMirrorObjectsList[0];   // obj is the reference to the hemisphere mirror object

        //string objectName = obj.objectName;
        // _mirrorType = obj.mirrorType;


        var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

        // Add vertex data
        // get the current number of vertices in the vertex list
        int currentVertexOffset = VerticesList.Count;  // The number of vertices so far created; will be used
                                                       // as the index of the first vertex of the newly created mesh
        VerticesList.AddRange(mesh.vertices);
        // Add Texcoords data.
        TexcoordsList.AddRange(mesh.uv);

        // Add index data - if the vertex buffer wasn't empty before, the
        // indices need to be offset
        int currentIndexOffset = IndicesList.Count; // the current count of _indices  list; will be used
                                                    // as the index offset in _indices for the newly created mesh
        int[] meshVertexIndices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                                      // when the mesh topology is triangle;
                                                      // indices will contain a multiple of three indices
                                                      // our mesh is actually a triangular mesh.

        if (UseMeshDebugging)
        {
            //Debug.Log("mirror object=" + objectName);


            //show the local coordinates of the triangles
            for (int i = 0; i < meshVertexIndices.Length; i += 3)
            {   // a triangle v0,v1,v2 
                Debug.Log("triangular Mirror: triangle indices (local) =(" + mesh.vertices[meshVertexIndices[i]].ToString("F6")
                          + "," + mesh.vertices[meshVertexIndices[i + 1]].ToString("F6")
                          + "," + mesh.vertices[meshVertexIndices[i + 2]].ToString("F6") + ")");
            }

        }

        IndicesList.AddRange(meshVertexIndices.Select(index => index + currentVertexOffset));


        // Add Texcoords data.
        //_texcoords.AddRange(mesh.uv);

        // Add the object itself

        if (HemisphereMirrorsList.Count == 0)
        {
            HemisphereMirrorsList.Add(
                new HemisphereMirror()
                {
                    localToWorldMatrix = obj.transform.localToWorldMatrix,

                    distanceToOrigin = obj.hemisphereParam.distanceFromCamera,
                    height = obj.hemisphereParam.height,
                    usedHeight = obj.hemisphereParam.usedHeight,  // value is copied
                    bottomDiscRadius = obj.hemisphereParam.bottomDiscRadius,
                    sphereRadius = obj.hemisphereParam.sphereRadius,
                    notUsedHeightRatio = obj.hemisphereParam.notUsedHeightRatio,

                    albedo = obj.MeshOpticalProp.albedo,

                    specular = obj.MeshOpticalProp.specular,
                    smoothness = obj.MeshOpticalProp.smoothness,
                    emission = obj.MeshOpticalProp.emission,
                    //indices_offset = currentIndexOffset, // not used because the computer shader uses
                    // the hemisphere mirrir not as a triangular mesh but a geometric figure.
                    //indices_count =  meshVertexIndices.Length // set the index count of the mesh of the current obj
                }
                );
        }
        else
        {

            HemisphereMirrorsList[0] =
                new HemisphereMirror()
                {
                    localToWorldMatrix = obj.transform.localToWorldMatrix,

                    distanceToOrigin = obj.hemisphereParam.distanceFromCamera,
                    height = obj.hemisphereParam.height,
                    usedHeight = obj.hemisphereParam.usedHeight,  // value is copied
                    bottomDiscRadius = obj.hemisphereParam.bottomDiscRadius,
                    sphereRadius = obj.hemisphereParam.sphereRadius,
                    notUsedHeightRatio = obj.hemisphereParam.notUsedHeightRatio,

                    albedo = obj.MeshOpticalProp.albedo,

                    specular = obj.MeshOpticalProp.specular,
                    smoothness = obj.MeshOpticalProp.smoothness,
                    emission = obj.MeshOpticalProp.emission,
                    //indices_offset = currentIndexOffset, // not used because the computer shader uses
                    // the hemisphere mirrir not as a triangular mesh but a geometric figure.
                    //indices_count =  meshVertexIndices.Length // set the index count of the mesh of the current obj
                };
        }

        int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                     + 7 * sizeof(float);

        // int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
        //            + 5 * sizeof(float) - 4;
        // create a compute buffer and set the data to it

        Debug.Log($"HemisphereMirror.height= { obj.hemisphereParam.height}, HemisphereMirror.height" +
                   $"HemisphereMirror.usedHeight={obj.hemisphereParam.usedHeight}," +
                   $"HemisphereMirror.notusedHeightRatio= {obj.hemisphereParam.notUsedHeightRatio}");

        CreateComputeBuffer(ref HemisphereMirrorBuf,
                              HemisphereMirrorsList, stride);
    }   // RebuildHemisphereMirrorBuffer()

    void RebuildGeoConeMirrorBuffer()
    {
        //    if (!bGeoConeMirrorNeedRebuild)
        //    {
        //        return;
        //    }

        //    bGeoConeMirrorNeedRebuild = false;


        // Clear the cone mirror list
        GeoConeMirrorsList.Clear();

        // Loop over all objects and gather their data
        //foreach (RayTracingObject obj in _rayTracingObjects)
        var obj = GeoConeMirrorObjectsList[0];
        // _mirrorType = obj.mMirrorType;

        // Add the object itself
        if (GeoConeMirrorsList.Count == 0)
        {
            GeoConeMirrorsList.Add(
             new GeoConeMirror()
             {
                 localToWorldMatrix = obj.transform.localToWorldMatrix,
                 distanceToOrigin = obj.mConeParam.distanceFromCamera,
                 height = obj.mConeParam.height,
                 radius = obj.mConeParam.radius,
                 albedo = obj.MeshOpticalProp.albedo,

                 specular = obj.MeshOpticalProp.specular,
                 smoothness = obj.MeshOpticalProp.smoothness,
                 emission = obj.MeshOpticalProp.emission,



             }
             );
        }
        else
        {
            GeoConeMirrorsList[0] = new GeoConeMirror()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                distanceToOrigin = obj.mConeParam.distanceFromCamera,
                height = obj.mConeParam.height,
                radius = obj.mConeParam.radius,
                albedo = obj.MeshOpticalProp.albedo,

                specular = obj.MeshOpticalProp.specular,
                smoothness = obj.MeshOpticalProp.smoothness,
                emission = obj.MeshOpticalProp.emission,



            };
        }




        //    public struct GeoConeMirror
        //{
        //    public Matrix4x4 localToWorldMatrix; // the world frame of the cone
        //    public float distanceToOrigin;
        //    public Vector3 albedo;
        //    public Vector3 specular;
        //    public float smoothness;
        //    public Vector3 emission;
        //    public float height;    
        //    public float radius;  // the radius of the base of the cone



        //};




        int geoConeMirrorStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                                      + 4 * sizeof(float);

        CreateComputeBuffer(ref GeoConeMirrorBuf, GeoConeMirrorsList, geoConeMirrorStride);



    }    //RebuildGeoConeMirrorBuffer()

    void RebuildParaboloidMirrorBuffer()
    {
        //    if (!bParaboloidMeshObjectNeedRebuild)
        //    {
        //        return;
        //    }

        //    bParaboloidMeshObjectNeedRebuild = false;

        // Clear all lists
        ParaboloidMirrorsList.Clear();

        // Loop over all objects and gather their data
        //foreach (RayTracingObject obj in _rayTracingObjects)
        var obj = ParaboloidMirrorObjectsList[0];
        // _mirrorType = obj.mMirrorType;



        // Add the object itself
        if (ParaboloidMirrorsList.Count == 0)
        {
            ParaboloidMirrorsList.Add(
            new ParaboloidMirror()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                distanceToOrigin = obj.paraboloidParam.distanceFromCamera,
                height = obj.paraboloidParam.height,
                albedo = obj.MeshOpticalProp.albedo,

                specular = obj.MeshOpticalProp.specular,
                smoothness = obj.MeshOpticalProp.smoothness,
                emission = obj.MeshOpticalProp.emission,

                coefficientA = obj.paraboloidParam.coefficientA,
                coefficientB = obj.paraboloidParam.coefficientB,

            }
            );

        }
        else
        {
            ParaboloidMirrorsList[0] =
               new ParaboloidMirror()
               {
                   localToWorldMatrix = obj.transform.localToWorldMatrix,
                   distanceToOrigin = obj.paraboloidParam.distanceFromCamera,
                   height = obj.paraboloidParam.height,
                   albedo = obj.MeshOpticalProp.albedo,

                   specular = obj.MeshOpticalProp.specular,
                   smoothness = obj.MeshOpticalProp.smoothness,
                   emission = obj.MeshOpticalProp.emission,

                   coefficientA = obj.paraboloidParam.coefficientA,
                   coefficientB = obj.paraboloidParam.coefficientB,

               };
        }


        //        struct ParaboloidMirror
        //{
        //    public Matrix4x4 localToWorldMatrix; // the world frame of the cone
        //    public float distanceToOrigin; // distance from the camera to the origin of the paraboloid
        //    public float height;    
        //    public Vector3 albedo;
        //    public Vector3 specular;
        //    public float smoothness;
        //    public Vector3 emission;
        //    public float coefficientA;  // z = - ( x^2/a^2 + y^2/b^2)
        //    public float coefficientB;

        //};

        int paraboloidMirrorStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                                      + 5 * sizeof(float);
        CreateComputeBuffer(ref ParaboloidMirrorBuf, ParaboloidMirrorsList, paraboloidMirrorStride);

    }   // RebuildParaboloidMirrorObjectBuffer()

    void RebuildPanoramaMeshBuffer()
    {
        //if (!bPanoramaMeshObjectNeedRebuild)
        //{
        //    return;
        //}

        //bPanoramaMeshObjectNeedRebuild = false;

        var obj = PanoramaSreenObjectsList[0];   // obj should be changed when the panorama screen info is updated
                                                 // in the inspector
                                                 // var mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        var mesh = obj.gameObject.GetComponent<MeshFilter>().sharedMesh;


        // Add vertex data
        // get the current number of vertices in the vertex list
        int currentVertexOffset = VerticesList.Count;  // The number of vertices so far created; will be used
                                                       // as the index of the first vertex of the newly created mesh
        VerticesList.AddRange(mesh.vertices);
        // Add Texcoords data.
        TexcoordsList.AddRange(mesh.uv);

        // Add index data - if the vertex buffer wasn't empty before, the
        // indices need to be offset
        int currentIndexOffset = IndicesList.Count; // the current count of _indices  list; will be used
                                                    // as the index offset in _indices for the newly created mesh
        int[] meshVertexIndices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                                      // when the mesh topology is triangle;
                                                      // indices will contain a multiple of three indices
                                                      // our mesh is actually a triangular mesh.


        if (UseMeshDebugging)
        {
            Debug.Log("paorama screen mesh object=" + obj.name);

            //// show the local coordinates of the triangles
            for (int i = 0; i < meshVertexIndices.Length; i += 3)
            {   // a triangle v0,v1,v2 
                Debug.Log("Panorama: vertex (local) =(" + mesh.vertices[meshVertexIndices[i]].ToString("F6")
                          + "," + mesh.vertices[meshVertexIndices[i + 1]].ToString("F6")
                          + "," + mesh.vertices[meshVertexIndices[i + 2]].ToString("F6") + ")");


                Debug.Log("panorama: uv coord =(" + mesh.uv[meshVertexIndices[i]].ToString("F6")
                        + "," + mesh.uv[meshVertexIndices[i + 1]].ToString("F6")
                        + "," + mesh.uv[meshVertexIndices[i + 2]].ToString("F6") + ")");

            }

        }

        IndicesList.AddRange(meshVertexIndices.Select(index => index + currentVertexOffset));

        // Add the object itself
        if (PanoramaScreensList.Count == 0)
        {
            PanoramaScreensList.Add(
                new PanoramaScreen()
                {    //https://answers.unity.com/questions/1656903/how-to-manually-calculate-localtoworldmatrixworldt.html

                    localToWorldMatrix = obj.gameObject.transform.localToWorldMatrix,
                    highRange = obj.panoramaScreenParam.highRangeFromCamera,
                    lowRange = obj.panoramaScreenParam.lowRangeFromCamera,
                    albedo = obj.meshOpticalProp.albedo,

                    specular = obj.meshOpticalProp.specular,
                    smoothness = obj.meshOpticalProp.smoothness,
                    emission = obj.meshOpticalProp.emission,

                    indices_offset = currentIndexOffset,
                    indices_count = meshVertexIndices.Length, // set the index count of the mesh of the current obj
                }
                );
        }
        else
        {

            PanoramaScreensList[0] = new PanoramaScreen()
            {    //https://answers.unity.com/questions/1656903/how-to-manually-calculate-localtoworldmatrixworldt.html

                localToWorldMatrix = obj.gameObject.transform.localToWorldMatrix,
                highRange = obj.panoramaScreenParam.highRangeFromCamera,
                lowRange = obj.panoramaScreenParam.lowRangeFromCamera,
                albedo = obj.meshOpticalProp.albedo,

                specular = obj.meshOpticalProp.specular,
                smoothness = obj.meshOpticalProp.smoothness,
                emission = obj.meshOpticalProp.emission,

                indices_offset = currentIndexOffset,
                indices_count = meshVertexIndices.Length, // set the index count of the mesh of the current obj
            };
        }


        Debug.Log("Panorama Transform in RayTracingMaster");

        MyIO.DebugLogMatrix(obj.gameObject.transform.localToWorldMatrix);


        //struct PanoramaMesh
        //{
        //    public Matrix4x4 localToWorldMatrix;
        //    public float highRange;
        //    public float lowRange;
        //    public Vector3 albedo;
        //    public Vector3 specular;
        //    public float smoothness;
        //    public Vector3 emission;
        //    public int indices_offset;
        //    public int indices_count;
        //}

        int panoramaMeshStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                                          + 3 * sizeof(float) + 2 * sizeof(int);
        CreateComputeBuffer(ref PanoramaScreenBuf, PanoramaScreensList, panoramaMeshStride);

    }   // RebuildPanoramaMeshBuffer()

    #endregion

    protected static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride) where T : struct
    {
        // Do we already have a compute buffer?
        if (!ReferenceEquals(buffer, null))
        {
            // If no data or buffer doesn't match the given criteria, release it
            if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
            else
            {
                // The buffer can be reused.
            }
        }

        // buffer is null
        if (data.Count != 0)
        {
            // If the buffer has been released or wasn't there to begin with, create it
            if (ReferenceEquals(buffer, null))  // buffer is null => create it
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }

            // Set the reference to data on the buffer which is newly created not not.
            buffer.SetData(data);
        }

    }       //CreateComputeBuffer<T>

    protected void SetDbgBufsToShader()
    {
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_VertexBufferRW", Dbg_VerticesRWBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RayDirectionBuffer", Dbg_RayDirectionRWBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_IntersectionBuffer", Dbg_IntersectionRWBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_AccumRayEnergyBuffer", Dbg_AccumRayEnergyRWBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_EmissionBuffer", Dbg_EmissionRWBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_SpecularBuffer", Dbg_SpecularRwBuf);
    }

    protected void InitRenderTextureForCreateImage()
    {

        //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
        // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)    

        if (ResultRenderTex == null)
        {
            // Create the camera's render target for Ray Tracing
            //_Target = new RenderTexture(Screen.width, Screen.height, 0,
            ResultRenderTex = new RenderTexture(CurrentScreenResolutions.x,
                                                CurrentScreenResolutions.y,
                                                0,
                                                RenderTextureFormat.ARGBFloat,
                                                RenderTextureReadWrite.Linear)
            {

                //MOON: Change CurrentScreenResolution to Projector Width and Height

                //Render Textures can also be written into from compute shaders,
                //if they have “random access” flag set(“unordered access view” in DX11).

                enableRandomWrite = true
            };
            ResultRenderTex.Create();

        }
        if (ConvergedRenderTexForNewImage == null)
        {
            //_converged = new RenderTexture(Screen.width, Screen.height, 0,
            ConvergedRenderTexForNewImage = new RenderTexture(CurrentScreenResolutions.x,
                                                              CurrentScreenResolutions.y,
                                                              0,
                                                              RenderTextureFormat.ARGBFloat,
                                                              RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            ConvergedRenderTexForNewImage.Create();
        }

    }  //InitRenderTextureForCreateImage()


    #region DBG  
    //https://medium.com/google-developers/real-time-image-capture-in-unity-458de1364a4c

    //void Dbg_ProcessRenderTextures1(RenderTexture target) {
    //  //save the active renderTexture
    //  var savedTarget = RenderTexture.active;

    //  RenderTexture.active = target;
    //  // RenderTexture.active = _mainScreenRT;

    //  // RenderTexture.active = _Target;

    //  // Read pixels  from the currently active render texture, _Target
    //  ResultTex1.ReadPixels(new Rect(0, 0, CurrentScreenResolutions.x, CurrentScreenResolutions.y), 0, 0);
    //  // Actually apply all previous SetPixel and SetPixels changes.
    //  ResultTex1.Apply();

    //  RenderTexture.active = savedTarget;

    //  for (int y = 0; y < CurrentScreenResolutions.y; y += 5) {
    //    for (int x = 0; x < CurrentScreenResolutions.x; x += 5) {
    //      int idx = y * CurrentScreenResolutions.x + x;
    //      Debug.Log("_Predistorted[" + x + "," + y + "]=");
    //      Debug.Log(ResultTex1.GetPixel(x, y));
    //    }
    //  }
    //} // DebugRenderTexture()

    ///// <summary>
    ///// Need to transfer this to the dbg class
    ///// </summary>
    ///// <param name="target"></param>
    //void Dbg_PrintAllPixelsOfResultTex1(Texture2D target) {
    //  for (int y = 0; y < CurrentScreenResolutions.y; y += 5) {
    //    for (int x = 0; x < CurrentScreenResolutions.x; x += 5) {
    //      int idx = y * CurrentScreenResolutions.x + x;

    //      Debug.Log("_PredistortedImage[" + x + "," + y + "]=");
    //      Debug.Log(ResultTex1.GetPixel(x, y));
    //    }
    //  }



    //}   // DebugTexture()

    //void Dbg_ProcessRenderTextures2() {
    //  // RenderTexture.active = _Target;
    //  // RenderTexture.active = _mainScreenRT;

    //  // Read pixels  from the currently active render texture
    //  // _resultTexture.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
    //  // Actually apply all previous SetPixel and SetPixels changes.
    //  // _resultTexture.Apply();

    //  RenderTexture.active = Dbg_RWTex;
    //  //RenderTexture.active = _mainScreenRT;
    //  //RenderTexture.active = _Target;

    //  // Read pixels  from the currently active render texture
    //  ResultTex2.ReadPixels(new Rect(0, 0, CurrentScreenResolutions.x, CurrentScreenResolutions.y), 0, 0);
    //  // Actually apply all previous SetPixel and SetPixels changes.
    //  ResultTex2.Apply();



    //  for (int y = 0; y < CurrentScreenResolutions.y; y += 5) {
    //    for (int x = 0; x < CurrentScreenResolutions.x; x += 5) {
    //      int idx = y * CurrentScreenResolutions.x + x;


    //      //Debug.Log("Target[" + x + "," + y + "]=");
    //      //Debug.Log(_resultTexture.GetPixel(x, y));
    //      Debug.Log("DebugRWTexture(index)[" + x + "," + y + "]=");
    //      Debug.Log(ResultTex2.GetPixel(x, y));

    //    }
    //  }

    //  RenderTexture.active = null; // added to avoid errors 

    //}   // DebugRenderTextures()

    void DebugLogOfRWBuffers()
    {

        //  // for debugging: print the buffer

        //  //_vertexBufferRW.GetData(mVertexArray);


        //  //RenderTexture.active = _PredistortedImage;
        //  //////RenderTexture.active = _mainScreenRT;

        //  //////RenderTexture.active = _Target;

        //  ////// Read pixels  from the currently active render texture
        //  //_resultTexture.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
        //  //////Actually apply all previous SetPixel and SetPixels changes.
        //  //_resultTexture.Apply();

        //  //save the active renderTexture
        //  var savedTarget = RenderTexture.active;

        //  RenderTexture.active = ResultRenderTex;
        //  ////RenderTexture.active = _mainScreenRT;

        //  ////RenderTexture.active = _Target;

        //  //// Read pixels  from the currently active render texture, _Target
        //  ResultTex2.ReadPixels(new Rect(0, 0, CurrentScreenResolutions.x, CurrentScreenResolutions.y), 0, 0);
        //  ////Actually apply all previous SetPixel and SetPixels changes.
        //  ResultTex2.Apply();

        //  RenderTexture.active = savedTarget;

        //  //RenderTexture.active = _DebugRWTexture;
        //  //////RenderTexture.active = _mainScreenRT;

        //  //////RenderTexture.active = _Target;

        //  ////// Read pixels  from the currently active render texture
        //  //_resultTexture3.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
        //  //////Actually apply all previous SetPixel and SetPixels changes.
        //  //_resultTexture3.Apply();




        // debugging for the ray 0
        // mRayDirectionBuffer.GetData(mRayDirectionArray);
        // mIntersectionBuffer.GetData(mIntersectionArray);

        Dbg_AccumRayEnergyRWBuf.GetData(Dbg_AccumulatedRayEnergyArr);
        Dbg_EmissionRWBuf.GetData(Dbg_EmissionArr);
        //mSpecularBuffer.GetData(mSpecularArray);           

        //for (int y = 0; y < CurrentScreenResolutions.y; y += 5)
        for (int y = CurrentScreenResolutions.y - 1; y >= 0; y--)
        {
            for (int x = 0; x <= (CurrentScreenResolutions.x - 1); x++)
            {
                int idx = y * CurrentScreenResolutions.x + x;


                //var myRayDir = mRayDirectionArray[idx];
                // var intersection = mIntersectionArray[idx];
                Vector4 emission = Dbg_EmissionArr[idx];
                Vector4 accumRayEnergy = Dbg_AccumulatedRayEnergyArr[idx];

                //var specular = mSpecularArray[idx];


                //for debugging


                // Debug.Log("(" + x + "," + y + "):" + "incoming ray direction=" + myRayDir.ToString("F6"));
                // Debug.Log("(" + x + "," + y + "):" + "hit point=" + intersection.ToString("F6"));

                //float4(hemisphere.notUsedHeightRatio,
                //                                               hemisphere.usedHeight,
                //                                               penetrationDistIntoMirror,
                //                                               penetrationDistIntoMirror / hemisphere.usedHeight);

                // //sr.WriteLine ("Time and Vector3 coordinates are: {0},{1},{2} and {3}", t, x, y, z);
                float hit = accumRayEnergy[3];
                if (hit != 0.0f)
                {
                    //m_writer.WriteLine("Coord: ({0},{1}), RayVector: ({2} , {3}, {4} )", x, y,
                    //                   emission[0], emission[1], emission[2]);
                    // m_writer.WriteLine("Coord: ({0},{1}), RayVector&PeneLen={2}", x, y, emission);
                    m_writer.WriteLine("Coord: ({0},{1}), u =  {2} , v = {3}, hit={4}", x, y,
                                         accumRayEnergy[0], accumRayEnergy[1], accumRayEnergy[3]);
                    m_writer.WriteLine("Coord: ({0},{1}), emission color at uv =({2} , {3}, {4} )", x, y,
                                       emission[0], emission[1], emission[2]);

                }

                else
                {
                    //m_writer.WriteLine("Coord: ({0},{1}), u =  {2} , v = {3}, hit={4}", x, y,
                    //                     accumRayEnergy[0], accumRayEnergy[1], accumRayEnergy[3]);
                    //m_writer.WriteLine("Coord: ({0},{1}), emission color at uv =({2} , {3}, {4} )", x, y,
                    //                   emission[0], emission[1], emission[2]);

                }
                //else
                //{
                //    m_writer.WriteLine("Coord: ({0},{1}), RayDirection: {2} ", x, y, emission);
                //    m_writer.WriteLine("Coord: ({0},{1}), _PassedCameraDirection ={2} )", x, y, accumRayEnergy);
                //}

                // Debug.Log("RayHitInfo(" + x + "," + y + ")=" + accumRayEnergy.ToString("F6"));
                //Debug.Log("(" + x + "," + y + "):" + "unTex.xy +id.xy=" + emission.ToString("F6"));
                //Debug.Log("(" + x + "," + y + "):" + "reflected direction=" + specular.ToString("F6"));
                // Debug.Log("Predistorted[" + x + "," + y + "]=" + _resultTexture.GetPixel(x, y));
                //Debug.Log("Target[" + x + "," + y + "]=" + ResultTex2.GetPixel(x, y));
                //Debug.Log("DebugRWTexture(index) [" + x + "," + y + "]=" + ResTex3.GetPixel(x, y));

            } // for x
        }  // for y

        //  // RenderTexture.active = null;  

    } //    void DebugLogOfRWBuffers()
    #endregion

    void ClearRenderTexture(RenderTexture target)
    {
        var savedTarget = RenderTexture.active;
        // save the active renderTexture  (currently null,  that is, the framebuffer

        RenderTexture.active = target;
        //GL.Clear(clearDepth, clearColor, backgroundColor, depth=1.0f);    // 1.0 means the far plane
        //        Parameters
        //clearDepth  Should the depth buffer be cleared?
        //clearColor Should the color buffer be cleared?
        //backgroundColor The color to clear with, used only if clearColor is true.
        //depth The depth to clear the z - buffer with, used only if clearDepth is true.
        //The valid range is from 0(near plane) to 1(far plane).The value is graphics API agnostic: the abstraction layer will convert the value to match the convention of the current graphics API.
        //Description
        //Clear the current render buffer.

        //This clears the screen or the active RenderTexture you are drawing into. The cleared area is limited by the active viewport.This operation might alter the model/ view / projection matrices.

        //In most cases, a Camera will already be configured to clear the screen or RenderTexture and you will not need to perform this operation manually.


        GL.Clear(true, true, Color.clear);
        //// Clears the screen or the active RenderTexture  (which is target) you are drawing into.

        RenderTexture.active = savedTarget; // restore the active renderTexture

    }     //ClearRenderTexture



    public void OnCreateDistortedImage_Btn()
    {

        // Build the mesh Objects; is supposed to be called when the mesh information is modified
        RebuildObjectBuffers();

        Debug.Log("Create Button is pressed and make the rendering state of Projector InProgress");


        // Initialize the process of rendering the predistorted image

        InitCreateDistortedImage(targetPanoramaTex0, targetPanoramaTex1,
                                   targetPanoramaTex2, targetPanoramaTex3);

        // Initialize the projector parameters

        projector.CurrentSamplingCountForRendering = 0;   // value
        projector.RenderingState = EDanbiRenderingState.InProgress;  // value

        projector.ResultRenderTex = ResultRenderTex;  // reference
        projector.ConvergedRenderTexForNewImage = ConvergedRenderTexForNewImage; // reference
        projector.RTShader = RTShader;  // reference
        projector.CurrentScreenResolutions = CurrentScreenResolutions;  // value

        projector.MaxSamplingCountForRendering = MaxSamplingCountForRendering;       // value

    }


    public void InitCreateDistortedImage(Texture2D panoramaTex0, Texture2D panoramaTex1,
                                         Texture2D panoramaTex2, Texture2D panoramaTex3)

    {
        /// <summary>
        /// This must be directly called on the script.
        /// </summary>


        // Make sure we have a current render target
        InitRenderTextureForCreateImage();
        // create _Target, _converge, _ProjectedImage renderTexture   (only once)

        // Find the compute shader kernel depending on the scene setup.

        if (GeoConeMirrorBuf != null)
        {
            if (PanoramaScreenBuf != null)
            {

                Danbi.DanbiKernelDict.AddKernalIndexWithKey(
                         (Danbi.EDanbiKernelKey.GeoconeMirror_Img_With_Lens_Distortion,
                          RTShader.FindKernel("CreateImageGeoConeMirror")
                          )
                );

                Danbi.DanbiKernelDict.CurrentKernelIndex = Danbi.DanbiKernelDict.GetKernalIndex(EDanbiKernelKey.GeoconeMirror_Img_With_Lens_Distortion);

                RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_GeoConedMirrors", GeoConeMirrorBuf);
                RTShader.SetInt("_MirrorMode", (int)EDanbiMirrorMode.E_ConeMirror);
                RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
            }
            else
            {
                Utils.StopPlaying();
            }
        }
        else if (ParaboloidMirrorBuf != null)
        {
            if (PanoramaScreenBuf != null)
            {

                Danbi.DanbiKernelDict.AddKernalIndexWithKey(
                         (Danbi.EDanbiKernelKey.ParaboloidMirror_Img_With_Lens_Distortion,
                          RTShader.FindKernel("CreateImageParaboloidMirror")
                          ));


                Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.ParaboloidMirror_Img);

                RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_ParaboloidMirrors", ParaboloidMirrorBuf);
                RTShader.SetInt("_MirrorMode", (int)EDanbiMirrorMode.E_ParabolaMirror);

                RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
            }
            else
            {
                //Debug.LogError("A panorama mesh should be defined");
                Utils.StopPlaying();
            }

        }
        else if (HemisphereMirrorBuf != null)
        {
            if (PanoramaScreenBuf != null)
            {

                Danbi.DanbiKernelDict.AddKernalIndexWithKey(
                    (Danbi.EDanbiKernelKey.HemisphereMirror_Img_With_Lens_Distortion,
                     RTShader.FindKernel("CreateImageHemisphereMirror")
                    )
                );


                // CurrentKernelIndex: Auto Property
                Danbi.DanbiKernelDict.CurrentKernelIndex = Danbi.DanbiKernelDict.GetKernalIndex(Danbi.EDanbiKernelKey.HemisphereMirror_Img_With_Lens_Distortion);

                RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_HemisphereMirrors", HemisphereMirrorBuf);
                RTShader.SetInt("_MirrorMode", (int)EDanbiMirrorMode.E_HemisphereMirror);
                RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
            }
            else
            {
                //Debug.LogError("A panorama mesh should be defined");
                Utils.StopPlaying();
            }

        }
        else
        {
            Debug.LogError("A mirror should be defined in the scene");
            Utils.StopPlaying();
        }

        //Vector3 l = DirectionalLight.transform.forward;
        //RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        // !COMMENTED OUT! -> FOV is already embedded into the projectio matrix of the current main camera.
        //RayTracingShader.SetFloat("_FOV", Mathf.Deg2Rad * _cameraMain.fieldOfView);

        //Debug.Log("_FOV" + Mathf.Deg2Rad * MainCamera.fieldOfView);
        //Debug.Log("aspectRatio" + MainCamera.aspect + ":" + CurrentScreenResolutions.x / (float)CurrentScreenResolutions.y);

        RTShader.SetInt("_MaxBounce", MaxNumOfBounce);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_Vertices", VerticesBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_Indices", IndicesBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_UVs", TexcoordsBuf);
        //RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_VertexBufferRW", Dbg_VerticesRWBuf);


        if (MainCamera != null)
        {
            // You need to set the lens distortion parameters to the computer shader, because it mentions them.
            // You need to define them whether the shader uses them or not.

            // Set the lens distortion parameters to the shader 
            RTShader.SetBuffer(Danbi.DanbiKernelDict.CurrentKernelIndex, "_CameraLensDistortionParams", CameraParamsForUndistortImageBuf);
            RTShader.SetVector("_ThresholdIterative", new Vector2(ThresholdIterative, ThresholdIterative));
            RTShader.SetInt("_SafeCounter", SafeCounter);
            RTShader.SetVector("_ThresholdNewton", new Vector2(ThresholdNewton, ThresholdNewton));

            // for debugging
            if (!UseCalibratedCamera)
            {
                // if we don't use the camera calibration.
                //https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html
                // Unity uses the OpenGL convention for the projection matrix.
                //The required z-flipping is done by the cameras worldToCameraMatrix (V). 
                //So the projection matrix (P) should look the same as in OpenGL. x_clip = P * V * M * v_obj

                // Reset the field of view of the camera to that of the calibrated camera
                //  \[ \alpha_v = 2 \tan^{-1}  \frac{h}{2f} \]
                //https://stackoverflow.com/questions/39992968/how-to-calculate-field-of-view-of-the-camera-from-camera-intrinsic-matrix
                //aspectRatio – f_y/f_x

                //*fovy = 2 * atan(imgHeight / (2 * alphay)) * 180.0 / CV_PI;


                //                Computes useful camera characteristics from the camera matrix.

                //C++: void calibrationMatrixValues(InputArray cameraMatrix, Size imageSize, double apertureWidth, double apertureHeight, double&fovx, double&fovy, double&focalLength, Point2d & principalPoint, double&aspectRatio)
                //Python: cv2.calibrationMatrixValues(cameraMatrix, imageSize, apertureWidth, apertureHeight) → fovx, fovy, focalLength, principalPoint, aspectRatio
                //Parameters:	
                //cameraMatrix – Input camera matrix that can be estimated by calibrateCamera() or stereoCalibrate() .
                //imageSize – Input image size in pixels.
                //apertureWidth – Physical width in mm of the sensor.
                //apertureHeight – Physical height in mm of the sensor.
                //fovx – Output field of view in degrees along the horizontal sensor axis.
                //fovy – Output field of view in degrees along the vertical sensor axis.
                //focalLength – Focal length of the lens in mm.
                //principalPoint – Principal point in mm.
                //aspectRatio – f_y / f_x


                float width = (float)CurrentScreenResolutions.x; // width = 3840 =  Projector Width
                float height = (float)CurrentScreenResolutions.y; // height = 2160 = Projector Height


                float scaleFactorX = CameraInternalParameters.FocalLength.y;
                float scaleFactorY = CameraInternalParameters.FocalLength.y;

               

               // Debug.Log($"Field of view 1 ={MainCamera.fieldOfView}, aspect = {MainCamera.aspect}");
                //  aspect = fx/fy in our case, but is alphay / alphax in opencv;

               // FrustumPlanes frustumPlanes = MainCamera.projectionMatrix.decomposeProjection;

               // Debug.Log("Decomposition of Perpsective Matrix Unity 1=");
               // Debug.Log($"left={frustumPlanes.left}, right={frustumPlanes.right},bottom={frustumPlanes.bottom}," +
               //    $"top={frustumPlanes.top}, near={frustumPlanes.zNear},  far={frustumPlanes.zFar}");

               // Debug.Log($"Original Perpsective Matrix Unity 1=\n{MainCamera.projectionMatrix}");


                MainCamera.fieldOfView = 2 * Mathf.Atan(height / (2 * scaleFactorY)) *  180/ Mathf.PI;

                MainCamera.aspect = 16.0f / 9.0f;

                // Debug.Log($"Field of view 2 ={MainCamera.fieldOfView}, aspect = {MainCamera.aspect}");


                // frustumPlanes = MainCamera.projectionMatrix.decomposeProjection;

                //Debug.Log("Decomposition of Perpsective Matrix Unity 2=");
                //Debug.Log($"left={frustumPlanes.left}, right={frustumPlanes.right},bottom={frustumPlanes.bottom}," +
                //   $"top={frustumPlanes.top}, near={frustumPlanes.zNear},  far={frustumPlanes.zFar}");

                //Debug.Log($"Original Perpsective Matrix Unity 2=\n{MainCamera.projectionMatrix}");

                //////https://m.blog.naver.com/PostView.nhn?blogId=techshare&logNo=221362240987&proxyReferer=https:%2F%2Fwww.google.com%2F

                //Vector4 posOfNearInClipSpace = MainCamera.projectionMatrix *
                //                            new Vector4(0.0f, 0.0f, -MainCamera.nearClipPlane, 1.0f);



                //Debug.Log($"Appy Projection to near plane1={posOfNearInClipSpace.ToString("F6")}");

                //Vector4 posOfFarInClipSpace = MainCamera.projectionMatrix *
                //                            new Vector4(0.0f, 0.0f, -MainCamera.farClipPlane, 1.0f);
                //Debug.Log($"Appy Projection to far plane={posOfFarInClipSpace.ToString("F6")}");


                ////reconstruct the perspective matrix

                ////  Matrix4x4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column3);
                //Vector4 column0 = new Vector4(2.273191f, 0f, 0f, 0f);
                //Vector4 column1 = new Vector4(0f, 2.845355f, 0f, 0f);
                //Vector4 column2 = new Vector4(0f, 0f, -1.000667f, -1.0f);
                //Vector4 column3 = new Vector4(0f, 0f, -0.1000333f, 0f);

                //Matrix4x4 constructedPersp = new Matrix4x4(column0, column1, column2, column3);
                //Debug.Log("Reconstructed Perpsective Matrix=");
                //MyIO.DebugLogMatrix(constructedPersp);



                //posOfNearInClipSpace = constructedPersp *
                //                            new Vector4(0.0f, 0.0f, -0.1f, 1.0f);

                //Debug.Log("**************************************************");
                ////Debug.Log($"Appy Reconstructed Projection to near plane1={posOfNearInClipSpace.ToString("F6")}");
                //Debug.Log($"Appy Reconstructed Projection to near plane1={posOfNearInClipSpace.x},{posOfNearInClipSpace.y},{posOfNearInClipSpace.z},{posOfNearInClipSpace.w}");



                //posOfFarInClipSpace = constructedPersp*
                //                            new Vector4(0.0f, 0.0f, -100f, 1.0f);
                //Debug.Log($"Appy Reconstructed Projection to far plane={posOfFarInClipSpace.ToString("F6")}");



                //Vector4 nearPlaneVectorZero = MainCamera.projectionMatrix.inverse* new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                //Vector4 nearPlaneVectorMinusOne = MainCamera.projectionMatrix.inverse * new Vector4(0.0f, 0.0f, -1.0f, 1.0f);
                //Vector4 nearPlaneVectorPlusOne = MainCamera.projectionMatrix.inverse * new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                //Debug.Log($"Inverse Projection: nearPlaneZero={nearPlaneVectorZero.ToString("F6")}");
                //Debug.Log($"Inverse Projection: nearPlaneMinusOne={nearPlaneVectorMinusOne.ToString("F6")}");
                //Debug.Log($"Inverse Projection: nearPlanePluseOne={nearPlaneVectorPlusOne.ToString("F6")}");


                //Debug.Log("**************************************************");

                //Matrix4x4 perspMat = PerspectiveOffCenter(frustumPlanes.left, frustumPlanes.right, frustumPlanes.bottom, frustumPlanes.top,
                //                     frustumPlanes.zNear, frustumPlanes.zFar);

                //Debug.Log("Perpsective Matrix Constructed=");
                //MyIO.DebugLogMatrix(perspMat);

                //frustumPlanes = perspMat.decomposeProjection;

                //Debug.Log("Decomposition of the constructed projection matrix");

                //Debug.Log($"left={frustumPlanes.left}, right={frustumPlanes.right},bottom={frustumPlanes.bottom}," +
                //    $"top={frustumPlanes.top}, near={frustumPlanes.zNear},  far={frustumPlanes.zFar}");


                //nearPlaneVectorZero = perspMat.inverse * new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                //nearPlaneVectorMinusOne = perspMat.inverse * new Vector4(0.0f, 0.0f, -1.0f, 1.0f);
                //nearPlaneVectorPlusOne = perspMat.inverse * new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                //Debug.Log("Constructed Projection Matrix");

                //Debug.Log($"Inverse Projection: nearPlaneZero={nearPlaneVectorZero.ToString("F6")}");
                //Debug.Log($"Inverse Projection: nearPlaneMinusOne={nearPlaneVectorMinusOne.ToString("F6")}");
                //Debug.Log($"Inverse Projection: nearPlanePluseOne={nearPlaneVectorPlusOne.ToString("F6")}");


                RTShader.SetMatrix("_Projection", MainCamera.projectionMatrix);
                RTShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);


                RTShader.SetBool("_UseCalibratedCamera", false );

            }
          //  else
            {
                RTShader.SetBool("_UseCalibratedCamera", true);

                // .. Construct the projection matrix from the calibration parameters
                //    and the field-of-view of the current main camera.        
                // Here we reconstruct the GL projection matrix from the assumed calibrated parameters
                // of the OPENGL camera.

                //This discussion of camera-scaling shows that there are an infinite number of pinhole cameras 
                //that produce the same image. The intrinsic matrix is only concerned with the relationship 
                //    between camera coordinates and image coordinates, so the absolute camera dimensions are irrelevant. 
                //    Using pixel units for focal length and principal point offset allows us to represent
                //    the relative dimensions of the camera,
                //namely, the film's position relative to its size in pixels.
                //Another way to say this is that the intrinsic camera transformation is invariant to uniform scaling
                //of the camera geometry. By representing dimensions in pixel units, 
                //we naturally capture this invariance.


                //https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html
                // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/
                //http://ksimek.github.io/2012/08/14/decompose/

                //http://ksimek.github.io/2013/08/13/intrinsic/



                //You've calibrated your camera. You've decomposed it into intrinsic and extrinsic camera matrices.
                //Now you need to use it to render a synthetic scene in OpenGL. 
                //You know the extrinsic matrix corresponds to the modelview matrix
                //and the intrinsic is the projection matrix, but beyond that you're stumped.

                //In reality, glFrustum does two things: first it performs perspective projection, 
                //    and then it converts to normalized device coordinates(NDC). 
                //    The former is a common operation in projective geometry, 
                //    while the latter is OpenGL arcana, an implementation detail.

                // THe main Point: Proj = NDC × Persp

                // the actual projection matrix representation inside the GPU might be different
                //from the representation you use in Unity. 
                //However you don't have to worry about that since Unity handles this automatically. 
                //The only case where it does matter when you directly pass a matrix from your code to a shader.
                //In that case Unity offers the method GL.GetGPUProjectionMatrix which converts 
                //the given projection matrix into the right format used by the GPU.

                //So to sum up how the MVP matrix is composed:
                //https://m.blog.naver.com/PostView.nhn?blogId=techshare&logNo=221362240987&proxyReferer=https:%2F%2Fwww.google.com%2F

                //M = transform.localToWorld of the object
                //V = camera.worldToCameraMatrix
                //P = GL.GetGPUProjectionMatrix(camera.projectionMatrix)  // camera.projectionMatrix follows OpenGL
                //  MVP = P V M                                           // GL.GetGPUProjectionMatrix() follows DX11
                // NDC(normalized device coordinates) are the coordinates after the perspective divide
                //    which is performed by the GPU. The Projection matrix actually outputs homogenous clipspace coordinates
                //    which are similar to NDC but before the normalization.

                // Specifically, you should pass the pixel coordinates of the left, right, bottom, and top of the window 
                // you used when performing calibration. For example, lets assume you calibrated using a 640×480 image.
                // If you used a pixel coordinate system whose origin is at the top - left, with the y - axis
                //    increasing in the downward direction, you would call glOrtho(0, 640, 480, 0, near, far). 
                //    If you calibrated with an origin at zero and normal leftward / upward x,y axis,
                //    you would call glOrtho(-320, 320, -240, 240, near, far).

                //http://www.songho.ca/opengl/gl_projectionmatrix.html

                //If you used a pixel coordinate system whose origin is at the top-left (OpenCV), 
                // with the y-axis increasing in the  downward direction, call:
                // Matrix4x4 openGLNDCMatrix = GetOrthoMatOpenGL(0, width, 0, height, near, far); 


                //Camera Calibration (Very Good with a good picture): 
                //https://docs.opencv.org/2.4/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html

                //(cx, cy) is a principal point that is usually at the image center; It is measured with resepct to the
                // top left corner of the iamge space:  x' = x_e/Z-e, y'=y_e/z_e; u= fx * x' + c_x; v= fy * y' + c_y;
                // x' = 0 when x_e =0; y'=0 when y_e =0; 

                // The left-top corner of the 'image" is away from the principal point
                // (which the z-axis of the camera intersects) by   (c_x, c_y).


                //    Radial Coefficient  -0.00987701 0.22019886 - 0.56139517
                //    Tangential Coefficient  -0.00093723 - 0.00275611
                //    Principal Point 1922.94259  1089.44916
                //    Focal Length    3242.25507  3240.55697


                //     R(Rotation Matrix) = [[-9.99984648e-01, -5.54101900e-03, 3.07308875e-06],
                //     [5.54067185e-03, -9.99927907e-01, -1.06527964e-02],
                //     [6.21002143e-05, -1.06526158e-02, 9.99943257e-01]]
                //     T(Translation Vector) = [611.53383621, 407.5504153, 2118.60708594]


                // 1) 코드
                //fovx, fovy, focalLength, principalPoint, aspectRatio = cv2.calibrationMatrixValues(proj_int, proj_shape, 16.4, 10.2)

                //proj_int = 프로젝터 내부 파라미터 명시된 매트릭스(캘리브레이션으로 구한 매트릭스)
                //proj_shape = 프로젝터 해상도 = (proj_height, proj_width) = (2160, 3840)
                //16.4[mm] = 프로젝터 sensor width
                //10.2[mm] = 프로젝터 Sensor height

                //2) 결과
                
//(3840, 2160) 으로 변경하여 코드 실행시켰더니

//fovx: 61.266343684968184
//fovy: 36.863725892803764
//focalLength: 13.847131021228204
//principalPoint: (8.21256731975761, 5.144621018523849)
//aspectRatio: 0.9994762599607733
//이렇게 나옵니다.

//유림 학생 Yurim, [08.11.20 12:23]
//(cx, cy) = (1922.94259, 1089.44916)[pixel] 단위를[mm] 단위로 변경하면(8.268, 5.120) 이 나오는데 얼추 비슷한 것 같습니다.

//cx * (16.4 / 3840) = 8.268
//cy * (10.2 / 2160) = 5.120

                float width = (float)CurrentScreenResolutions.x; // width = 3840 =  Projector Width
                float height = (float)CurrentScreenResolutions.y; // height = 2160 = Projector Height

                float left = 0;
                float right = width;
                float bottom = 0;
                float top = height;


                float near = MainCamera.nearClipPlane;      // near: positive
                float far = MainCamera.farClipPlane;        // far: positive

               

              

               
                float aspectRatio = width / height;
               // float scaleFactorX = 1 / (aspectRatio * Mathf.Tan(fieldOfView));
               // float scaleFactorY = 1 / Mathf.Tan(fieldOfView);

                float scaleFactorX = CameraInternalParameters.FocalLength.y;
                float scaleFactorY = CameraInternalParameters.FocalLength.y;

            
                // MainCamera.fieldOfView = 2 * Mathf.Atan(height / (2 * scaleFactorY)) * 180/  Mathf.PI;

                float cx = CameraInternalParameters.PrincipalPoint.x;
                float cy = CameraInternalParameters.PrincipalPoint.y;

               // float cx = width / 2;
               // float cy = height /2;

                // Method 1: the most rigourous
                //Matrix4x4 NDCMatrixOpenGL1 = GetOrthoMat(left, right, top, bottom, near, far);

                Matrix4x4 NDCMatrix_OpenGL = GetOrthoMat(left, right, bottom, top, near, far);
                //Matrix4x4 openGLPerspMatrix1 = OpenCV_KMatrixToOpenGLPerspMatrix(CameraInternalParameters.FocalLength.x, CameraInternalParameters.FocalLength.y,
                //                                              CameraInternalParameters.PrincipalPoint.x, CameraInternalParameters.PrincipalPoint.y,
                //                                              near, far);

                // refer to to   //http://ksimek.github.io/2012/08/14/decompose/ 
                // understand the following code
                Matrix4x4 KMatrixFromOpenCVToOpenGL = OpenCVKMatrixToOpenGLKMatrix(scaleFactorX, scaleFactorY,   cx, cy,  near, far);


                //we can think of the perspective transformation as converting 
                // a trapezoidal-prism - shaped viewing volume
                //    into a rectangular - prism - shaped viewing volume,
                //    which glOrtho() scales and translates into the 2x2x2 cube in Normalized Device Coordinates.

                // Invert the direction of y axis and translate by height along the inverted direction.

                //                Until now, our discussion of 2D coordinate conventions have referred to the coordinates used during calibration.
                //                    If your application uses a different 2D coordinate convention,
                //                    you'll need to transform K using 2D translation and reflection.

                //               For example, consider a camera matrix that was calibrated with the origin in the top-left 
                //                    and the y - axis pointing downward, but you prefer a bottom-left origin with the y-axis pointing upward.
                //                    To convert, you'll first negate the image y-coordinate and then translate upward by the image height, h. 
                //                    The resulting intrinsic matrix K' is given by:

                // K' = [ 1 0 0; 0 1 h; 0 0 1] *  [ 1 0 0; 0 -1 0; 0 0 1] * K

                // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/

                Vector4 column0 = new Vector4(1f, 0f, 0f, 0f);
                Vector4 column1 = new Vector4(0f, -1f, 0f, 0f);
                Vector4 column2 = new Vector4(0f, 0f, 1f, 0f);
                Vector4 column3 = new Vector4(0f, height, 0f, 1f);

                Matrix4x4 OpenCVCameraToOpenGLCamera = new Matrix4x4(column0, column1, column2, column3);

                Matrix4x4 projectionMatrixGL1 = NDCMatrix_OpenGL * OpenCVCameraToOpenGLCamera * KMatrixFromOpenCVToOpenGL;


               // Matrix4x4 projectionMatrixGL1 = NDCMatrixOpenGL * openGLPerspMatrix1;
                Debug.Log($"Reconstructed projection matrix, method 1:\n{projectionMatrixGL1}");

                // MainCamera.projectionMatrix = projectionMatrixGL; 


                // Debug.Log($"NDCMatrixOpenCV=\n {NDCMatrixOpenCV}");

                // Debug.Log($"OpenCVtoOpenGL=\n{OpenCVtoOpenGL}");


                //// Method 2: shitfing orthogonal with the central projection.

                //left = CameraInternalParameters.PrincipalPoint.x;
                //right = width - left;

                //top = CameraInternalParameters.PrincipalPoint.y;
                //bottom = -(height - top);

                //Matrix4x4 NDCMatrixOpenGL2 = GetOrthoMat(left, right, top, bottom, near, far);

                //Matrix4x4 NDCMatrixOpenCV2 = GetOrthoMat(left, right, bottom, top, near, far);
                //Matrix4x4 openGLPerspMatrix2 = OpenCVKMatrixToOpenGLKMatrix(CameraInternalParameters.FocalLength.x, CameraInternalParameters.FocalLength.y,
                //                                              0.0f, 0.0f,
                //                                              near, far);



                ////we can think of the perspective transformation as converting 
                //// a trapezoidal-prism - shaped viewing volume
                ////    into a rectangular - prism - shaped viewing volume,
                ////    which glOrtho() scales and translates into the 2x2x2 cube in Normalized Device Coordinates.

                //// Invert the direction of y axis and translate by height along the inverted direction.

                //column0 = new Vector4(1f, 0f, 0f, 0f);
                //column1 = new Vector4(0f, -1f, 0f, 0f);
                //column2 = new Vector4(0f, 0f, 1f, 0f);
                //column3 = new Vector4(0f, height, 0f, 1f);

                //Matrix4x4 OpenCVtoOpenGL2 = new Matrix4x4(column0, column1, column2, column3);

                //// Matrix4x4 projectionMatrixGL1 = NDCMatrixOpenCV * OpenCVtoOpenGL * openGLPerspMatrix;


                //Matrix4x4 projectionMatrixGL2 = NDCMatrixOpenGL2 * openGLPerspMatrix2;

                //Debug.Log($"projection matrix, method 2:\n{projectionMatrixGL2}");


                ////Debug.Log($"NDC  Matrix: Using GLOrtho directly=\n {NDCMatrixOpenGL}");

                ////Matrix4x4 NDCMatrixOpenGL2 = NDCMatrixOpenCV * OpenCVtoOpenGL;
                ////Debug.Log($"NDC  Matrix:Frame Transform Approach=\n{NDCMatrixOpenGL2}");




                RTShader.SetMatrix("_Projection", projectionMatrixGL1);
                RTShader.SetMatrix("_CameraInverseProjection", projectionMatrixGL1.inverse);
                // check if you use the projector lens distortion
                if (!UseLensDistortion)
                {
                     // Do not use Undistortion when you do not use
                                                                 // the calibrated camera
                    RTShader.SetInt("_UndistortMode", -1);
                }
                else
                {
                    RTShader.SetInt("_UndistortMode", (int)UndistortMode); // UndistortMode is set in the inspector

                }
            }  // else of if (!UseCalibratedCamera)

            RTShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);
            Vector4 cameraDirection = new Vector4(MainCamera.transform.forward.x, MainCamera.transform.forward.y,
                                            MainCamera.transform.forward.z, 0f);
            //RTShader.SetVector(" _CameraViewDirectionInUnitySpace", MainCamera.transform.forward);
            RTShader.SetVector("_CameraForwardDirection", cameraDirection);
            // Vector4.

            //Debug
            Debug.Log("_CameraForwardDirection DebugLog=" + MainCamera.transform.forward.x + "," +
                             MainCamera.transform.forward.y + "," + MainCamera.transform.forward.z);


            //m_writer.WriteLine("_CameraToWorld = ", MainCamera.cameraToWorldMatrix);
            //m_writer.WriteLine("_Camera = ", MainCamera);
            // Debug.Log("Camera in InitCreateDistortedImage=", MainCamera);

            //Debug.Log("_gameObject of Camera component", gameObject);
            //m_writer.WriteLine("_CameraViewDirectionInUnitySpace=", gameObject.transform.forward);

        }   // if   (MainCamera != null)
        else
        {
            Debug.LogError("MainCamera should be activated");
            Utils.StopPlaying();
        }

        //// used the result of the rendering (raytracing shader)
        ////Hint the GPU driver that the contents of the RenderTexture will not be used.
        //// _Target.DiscardContents();
        // Clear the target render Texture _Target

        // clear the framebuffer (screen)

        GL.Clear(true, true, Color.clear);

        ClearRenderTexture(ConvergedRenderTexForNewImage);
        ClearRenderTexture(ResultRenderTex);

        RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_Result", ResultRenderTex);  // used always      

        // set the textures TargetPanoramaTexFromImage
        //CurrentRayTracerShader.SetTexture(mKernelToUse, "_SkyboxTexture", SkyboxTex);

        //if (panoramaTex == null)
        //{
        //    Debug.Log($"<color=red>panoramaTex cannot be null!</color>", this);
        //}
        //else
        //{
        //    RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture0", panoramaTex0);
        //}

        RTShader.SetInt("_NumOfTargetTextures", NumOfTargetTextures);

        if (NumOfTargetTextures == 1)
        {
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture0", panoramaTex0);
        }
        else if (NumOfTargetTextures == 2)
        {
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture0", panoramaTex0);
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture1", panoramaTex1);
        }
        else if (NumOfTargetTextures == 3)
        {
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture0", panoramaTex0);
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture1", panoramaTex1);
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture2", panoramaTex2);
        }
        else
        {
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture0", panoramaTex0);
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture1", panoramaTex1);
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture2", panoramaTex2);
            RTShader.SetTexture(Danbi.DanbiKernelDict.CurrentKernelIndex, "_RoomTexture3", panoramaTex3);
        }

        //#region debugging
        //if (UseGPUDebugging)
        //{
        //    SetDbgBufsToShader();
        //}
        //#endregion
    }   // InitCreateDistortedImage

    // 
    static Matrix4x4 OpenCVKMatrixToOpenGLKMatrix(float alpha, float beta, float x0, float y0,
                                                        float near, float far)
    {

        //Our 3x3 intrinsic camera matrix K needs two modifications before it's ready to use in OpenGL.
        //    First, for proper clipping, the (3,3) element of K must be -1. OpenGL's camera looks down the negative z - axis, 
        //    so if K33 is positive, vertices in front of the camera will have a negative w coordinate after projection. 
        //    In principle, this is okay, but because of how OpenGL performs clipping, all of these points will be clipped.

        // If K33 isn't -1, your intrinsic and extrinsic matrices need some modifications. 
        //    Getting the camera decomposition right isn't trivial,
        //    so I'll refer the reader to my earlier article on camera decomposition,
        //    which will walk you through the steps.
        //    Part of the result will be the negation of the third column of the intrinsic matrix, 
        //    so you'll see those elements negated below.



        //   K= \alpha 0  u_0 
        //      \beta 0  v_0
        //       0    0    1  


        //     u0, v0 are the image principle point ,  with f being the focal length and 
        //     being scale factors relating pixels to distance.
        //     Multiplying a point  
        //     by this matrix and dividing by resulting z-coordinate then gives the point projected into the image.
        //The OpenGL parameters are quite different.  Generally the projection is set using the glFrustum command,
        //    which takes the left, right, top, bottom, near and far clip plane locations as parameters
        //    and maps these into "normalized device coordinates" which range from[-1, 1].
        //    The normalized device coordinates are then transformed by the current viewport, 
        //    which maps them onto the final image plane.Because of the differences,
        //    obtaining an OpenGL projection matrix which matches a given set of intrinsic parameters 
        //   is somewhat complicated.


        // construct a projection matrix, this is identical to the 
        // projection matrix computed for the intrinsicx, except an
        // additional row is inserted to map the z-coordinate to
        // OpenGL. 

        //https://github.com/Emerix/AsymFrustum
        //https://answers.unity.com/questions/1359718/what-do-the-values-in-the-matrix4x4-for-cameraproj.html
        // Set an off-center projection, where perspective's vanishing
        // point is not necessarily in the center of the screen.
        //
        // left/right/top/bottom define near plane size, i.e.
        // how offset are corners of camera's near plane.
        // Tweak the values and you can see camera's frustum change.
        //https://stackoverflow.com/questions/2286529/why-does-sign-matter-in-opengl-projection-matrix
        //https://docs.microsoft.com/en-us/windows/win32/opengl/glfrustum?redirectedfrom=MSDN
        //
        //        -The intersection of the optical axis with the image place is called principal point or
        //image center.
        //(note: the principal point is not always the "actual" center of the image)

        //Less commonly, we may wish to translate the 2D normalized device coordinates by
        //[cx, cy]. This can be modeled in the projection matrix as   in p. 95 in Foundations of Computer Graphics
        // In a shifted camera, we translate the normalized device coordinates and
        // keep the[−1..1] region in these shifted coordinates, as shown in Fig. 10.7 in the above book.
        // The [shifted] 3D frustum is defined by specifying an image rectangle on the near
        // plane as in Fig. 10.9 of the book.

        //left, right, top and bottom actually specify the boundary / size of the near-clipping plane. 
        // The "near" distance defines how far away from the camera origin the clipping plane is located.

        //The normalized device coordinates uses a left - handed system
        //    while OpenGL(and mathematics in general) uses a right - handed system.
        //    Unity however already uses a left-handed system.
        //    But since the projection matrix should be compatible with all sorts of APIs, they define it the usual way.
        //    That's why Unity's "camera / view matrix" artifically inverts the z-axis.
        //    That means inside the shader after the model and view transformation the z values are actually negative.

        Matrix4x4 PerspK = new Matrix4x4();

        Debug.Log($"Print the initially created matrix=\n {PerspK}");

        float A = (near + far);
        float B = near * far;

        //http://ksimek.github.io/2012/08/14/decompose/

// Starting from an all-positive diagonal, follow these four steps:

//If the image x-axis and your camera x - axis point in opposite directions, negate the first column of K and the first row of R.
//If the image y-axis and uour camera y - axis point in opposite directions, negate the second column of K and the second row of R.
//If the camera looks down the negative - z axis, negate the third column of K. 
//Also negate the third column of R.
//If the determinant of R is -1, negate it.

  


        PerspK[0, 0] = alpha;   // scaling factor in x
        PerspK[1, 1] = beta;    // scaling factor in y;

        PerspK[0, 2] = - x0;   // negate the third column of openCV K, because the camera looks down the negative z axis                                
        PerspK[1, 2] = - y0;    // negate the third column of openCV K
        PerspK[2, 2] = A;
        PerspK[2, 3] = B;
        PerspK[3, 2] = -1.0f; // // negate the third column of openCV K

        //Notice that element(3, 2) of the projection matrix is ‘-1’. 
        // This is because the camera looks in the negative-z direction, 
        //  which is the opposite of the convention used by Hartley and Zisserman.
        return PerspK;
    }

    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);   // 
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }


// Based On the Foundation of 3D Computer Graphics (book)
static Matrix4x4 GetOpenCVToUnity()
{
    var FrameTransform = new Matrix4x4();   // member fields are init to zero

    FrameTransform[0, 0] = 1.0f;
    FrameTransform[1, 1] = -1.0f;
    FrameTransform[2, 2] = 1.0f;
    FrameTransform[3, 3] = 1.0f;

    return FrameTransform;
}



// Based On the Foundation of 3D Computer Graphics (book)
static Matrix4x4 GetOrthoMat(float left, float right, float bottom, float top, float near, float far)
{
    // construct an orthographic matrix which maps from projected
    // coordinates to normalized device coordinates in the range
    // [-1, 1]^3. 

    // Translate the box view volume so that its center is at the origin of the frame
    float tx = -(left + right) / (right - left);
    float ty = -(bottom + top) / (top - bottom);
    float tz = -(near + far) / (far - near);

    // Then scale the translated view volume into the normalized coordinates; The sign of the z coordinate
    // is changed so that the negative z (In openGL space) becomes positive (in the NDC space). 
    float m00 = 2.0f / (right - left);
    float m11 = 2.0f / (top - bottom);
    float m22 = -2.0f / (far - near);


    Matrix4x4 Ortho = new Matrix4x4();   // member fields are init to zero
    Ortho[0, 0] = m00;
    Ortho[1, 1] = m11;
    Ortho[2, 2] = m22;
    Ortho[0, 3] = tx;
    Ortho[1, 3] = ty;
    Ortho[2, 3] = tz;
    Ortho[3, 3] = 1.0f;

    return Ortho;
}


    public void OnValidate()
    {
        Debug.Log("OnValidate() is called in RayTracingMaster.cs");

        // 1. Calculate Current screen resolutions by the screen aspects and the screen resolutions.
        //CurrentScreenResolutions = DanbiScreenHelper.GetScreenResolution(TargetScreenAspect, TargetScreenResolution);
        //CurrentScreenResolutions *= SizeMultiplier;

        // 2. Set the panorama material automatically by changing the texture.
        //https://docs.unity3d.com/ScriptReference/GameObject-activeInHierarchy.html
        //Unlike GameObject.activeSelf, gameObject.activeInHierarchy also checks 
        //if any parent GameObjects affect the GameObject’s currently active state.
        if (this.enabled && this.gameObject.activeInHierarchy && this.gameObject.activeSelf)
        {
            Debug.Log("the current gameobject and its component are active; apply the texture in RayTracingMaster.cs");

            // 3. Apply the new target texture onto the scene and DanbiController both.

            // Set the panorama material automatically by changing the texture.
            //  PanoramaScreenObject panoramaScreen = (PanoramaScreenObject)Object.FindObjectOfType<PanoramaScreenObject>();
            // this.gameObject is the main Camera object
            GameObject fullCubeScreenObject = this.gameObject.transform.parent.GetChild(1).gameObject;
            PanoramaScreenObject panoramaScreen = fullCubeScreenObject.transform.GetChild(2).GetComponent<PanoramaScreenObject>();

            Material targetTexMat = panoramaScreen.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

            targetTexMat.SetInt("_NumOfTargetTextures", NumOfTargetTextures);
            targetTexMat.SetTexture("_MainTex0", targetPanoramaTex0);
            targetTexMat.SetTexture("_MainTex1", targetPanoramaTex1);
            targetTexMat.SetTexture("_MainTex2", targetPanoramaTex2);

            targetTexMat.SetTexture("_MainTex3", targetPanoramaTex3);

            // OnValiate() is also called when the main camera transform is updated

            Debug.Log($"MainCamera displaced:\n {Camera.main.transform.localToWorldMatrix}");
            // rotation.eulerAngles()
            // currentRotation.eulerAngles = eulerAngles => Quaternion currentRotation;
            // transform.rotation = currentRotation
            // CameraInternalParameters

            if (UseCalibratedCamera)
            {

                // Vector4.Vector3: implicit conversion, w discarded
                //A Vector2 can be implicitly converted into a Vector3. (The z is set to zero in the result).
                //https://medium.com/dev-genius/implicit-and-explicit-operators-c-30d28fb573e0




                float4x4 ViewTransform_OpenCV = new float4x4(new float4(CameraExternalParameters.xAxis, 0),
                                                             new float4(CameraExternalParameters.yAxis, 0),
                                                             new float4(CameraExternalParameters.zAxis, 0),
                                                             new float4(CameraExternalParameters.translation, 1)
                                                             );


                //float4x4 ViewTransform_OpenCV = new float4x4(new float4(1,0,0, 0),
                //                                             new float4(0,1,0,0),
                //                                             new float4(0,0,1, 0),
                //                                             new float4(CameraExternalParameters.translation, 1)
                //                                             );

                Debug.Log($"ViewTransform =\n{  ViewTransform_OpenCV }");


                //https://en.wikibooks.org/wiki/Cg_Programming/Vector_and_Matrix_Operations
                //GLSL:

                //    mat3 m(column0, column1, column2);
                //    m[0]; // returs the first column
                //HLSL:

                //    float3x3 m = float3x3(row0, row1, row2); // sets rows of matrix n
                //    m[0]; // Returns first row.

                // But In Unity, float3x3 behaves similarly as Matrix4x4 by 
                // constructing matrix from column vectors:

                //https://github.com/Unity-Technologies/Unity.Mathematics/blob/master/src/Unity.Mathematics/math_unity_conversion.cs


                //Handedness and matrices and quaternion:
                //https://stackoverflow.com/questions/1274936/flipping-a-quaternion-from-right-to-left-handed-coordinates/39519536#39519536
                // By Paul de Bois:

                //https://en.wikipedia.org/wiki/Matrix_similarity
                // Change of basis => a simpler form of the same transformation
                // In the changed frame (OpenCV) , y' = Sx'; In the original frame, y = Tx, where vectors x and y, and the unknown transform
                // T are in the original basis (Unity). To write T in terms of simpler matrix S:
                // y' = Sx' => y' = Py, x' = Px by change of basis P.
                // Py = SPx => y = P^{-1}SP x = Tx => T = P^{-1}SP


                //Conversion from quaternion to 3x3 matrix does not involve handedness of any sort. It is purely "solve for the matrix M such that Mv = qv" (assuming you're using column vectors).
                //See euclideanspace.com/maths/geometry/rotations/conversions/… for the derivation. – Paul Du Bois Aug 29 '17 at 18:57


                //quaternions don't have handedness (*). Handedness (or what I'll call "axis conventions") is a property 
                //  that humans apply;  it's how we map our concepts of "forward, right, up" to the X, Y, Z axes.

                //These things are true:

                //            (1)    Pure - rotation matrices(orthogonal, determinant 1, etc) can be converted to a unit quaternion and back, 
                //                    recovering the original matrix.
                //            (2)          Matrices that are not pure rotations(ones that have determinant -1, 
                // for example matrices that flip a single axis)
                //                    are also called "improper rotations", and cannot be converted to a unit quaternion and back. 
                //                    Your mat_to_quat() routine may not blow up, but it won't give you the right answer 
                //                    (in the sense that quat_to_mat(mat_to_quat(M)) == M).
                //            (3) A change-of - basis that swaps handedness has determinant - 1.It is an improper rotation: equivalent to a rotation(maybe identity) 
                //                    composed with a mirroring about the origin.

                //         Now, To change the basis of a quaternion, say from ROS(right - handed) to Unity(left-handed), we can use the method of .

                //              mat3x3 ros_to_unity = /* construct this by hand */;
                //                mat3x3 unity_to_ros = ros_to_unity.inverse();
                //                quat q_ros = ...;
                //                mat3x3 m_unity = ros_to_unity * mat3x3(q_ros) * unity_to_ros;
                //                quat q_unity = mat_to_quat(m_unity);
                //                Lines 1 - 4 are simply the method of https://stackoverflow.com/a/39519079/194921: 
                //"How do you perform a change-of-basis on a matrix?"

                //             Line 5 is interesting.We know mat_to_quat() only works on pure-rotation matrices.
                //How do we know that m_unity is a pure rotation? It's certainly conceivable that it's not,
                //    because unity_to_ros and ros_to_unity both have determinant -1(as a result of the handedness switch).

                //             The hand-wavy answer is that the handedness is switching twice, so the result has no handedness switch.
                //             The deeper answer has to do with the fact that similarity transformations preserve certain aspects of the operator,
                //                    but I don't have enough math to make the proof.


                //                The problem you ask about arises even if the two coordinate systems are same - handed; 
                //                it turns out that handedness flips don't make the problem significantly harder.
                //                    Here is how to do it in general. To change the basis of a quaternion,
                //                    say from ROS (right-handed, Z up) to Unity (left-handed, Y up):

                //mat3x3 ros_to_unity = /* construct this by hand by mapping input axes to output axes */;
                //                mat3x3 unity_to_ros = ros_to_unity.inverse();
                //                quat q_ros = ...;
                //                mat3x3 m_unity = ros_to_unity * mat3x3(q_ros) * unity_to_ros;
                //                quat q_unity = mat_to_quat(m_unity);
                //                Lines 1 - 4 are simply the method of https://stackoverflow.com/a/39519079/194921: "How do you perform a change-of-basis on a matrix?"

                //Line 5 is interesting; not all matrices convert to quats, but if ros_to_unity is correct, then this conversion will succeed.


                //Pure - rotation matrices(orthogonal, determinant 1, etc) can be converted to a unit quaternion and back, 
                //                    recovering the original matrix.
                //Matrices that are not pure rotations(ones that have determinant -1, for example matrices that flip a single axis) 
                //                    are also called "improper rotations", and cannot be converted to a unit quaternion and back. 
                //                    Your mat_to_quat() routine may not blow up, but it won't give you the right answer 
                //                    (in the sense that quat_to_mat(mat_to_quat(M)) == M).
                //A change-of - basis that swaps handedness has determinant - 1.It is an improper rotation:
                //                equivalent to a rotation(maybe identity) composed with a mirroring about the origin.


                //Camera external parameters: http://ksimek.github.io/2012/08/22/extrinsic/

                //Let C be a column vector describing the location of the camera-center in world coordinates, 
                //    and let Rc be the rotation matrix describing the camera's orientation 
                //    with respect to the world coordinate axes. 
                //    The transformation matrix that describes the camera's pose is then[Rc | C]
                // R =   worldToCameraRotation;  Rc = tranpose(R)

                // t =  -R^{T} * C, where C is the camera position in the world
                // t =  CameraExternalParameters.translation;
                // R^{T} * t = = -C
                //float4 cameraOrigionOpenCVWorld = - math.mul(openCVWorldToCameraMat,
                //                                             new float4(CameraExternalParameters.translation, 1));


                float3x3 ViewTransform_Rot_OpenCV = new float3x3(
                    ViewTransform_OpenCV.c0.xyz, ViewTransform_OpenCV.c1.xyz, ViewTransform_OpenCV.c2.xyz);

                float3 ViewTransform_Trans_OpenCV =  ViewTransform_OpenCV.c3.xyz;

                float3x3 CameraTransformation_Rot_OpenCV = math.transpose(ViewTransform_Rot_OpenCV);

                float4x4 CameraTransformation_OpenCV = new float4x4(CameraTransformation_Rot_OpenCV,
                                                        -math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV));

                Debug.Log($"CameraTransformation_OpenCV (obtained by transpose) =\n{ CameraTransformation_OpenCV }");


                float4x4 CameraTransform_OpenCV = math.inverse(ViewTransform_OpenCV);
                Debug.Log($" CameraTransform_OpenCV (obtained by inverse)=\n{  CameraTransform_OpenCV }");

           // https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system
              

                // UnityToOpenMat is a change of basis matrix, a swap of axes, with a determinmant -1, which is
                // improper rotation, and so a well-defined quaternion does not exist for it.

                float4 column0 = new float4(UnityToOpenCVMat.c0,0);
                float4 column1 = new float4(UnityToOpenCVMat.c1,0); 
                float4 column2 = new float4(UnityToOpenCVMat.c2,0); 
                float4 column3 = new float4(0, 0, 0, 1);


                float4x4 UnityToOpenCV = new float4x4(column0, column1, column2, column3);

                float3x3 UnityToOpenCV_Rot = new float3x3(column0.xyz, column1.xyz, column2.xyz);
                float3x3 OpenCVToUnity_Rot = math.transpose(UnityToOpenCV_Rot);
                float3 UnityToOpenCV_Trans = column3.xyz;

                float4x4 OpenCVToUnity = new float4x4(OpenCVToUnity_Rot, -math.mul(OpenCVToUnity_Rot, UnityToOpenCV_Trans));

                Debug.Log($" UnityToOpenCV inverse = \n {math.inverse(UnityToOpenCV)} ");

                Debug.Log($" UnityToOpenCV transpose  = \n {OpenCVToUnity}");

                // Camera Transformation in Unity Frame is defined in terms of the camera transformation in 
                // the new auxiliary frame, which is the OpenCV frame relative to which the camera transformation
                // is already determined. 


                // Change of basis => a simpler form of the same transformation
                // In the changed frame (OpenCV) , y' = Sx'; In the original frame, y = Tx, where vectors x and y, and the unknown transform
                // T are in the original basis (Unity). To write T in terms of simpler matrix S:
                // y' = Sx' => y' = Py, x' = Px by change of basis P.
                // Py = SPx => y = P^{-1}SP x = Tx => T = P^{-1}SP

                // P = UnityToOpenCV

                // CameraTransform_AuxFrame_Unity  = inverse(UnityToOpenCV) * CameraTransform_OpenCV * UnityToOpenCV

                // O^{t} = W{t} O , where o^{t} is the camera frame relative to the original (Unity) frame
                // The camera transformation is specified relative to an auxilary frame a^{t}, a^{t}= w^{t}A
                // o^{t} = a^{t}A^{-1}: Transform o^{t} by M relative to a^{t} (OpenCV frame):
                // o^{t} => a^{t} M A^{-1} O = w^{t}AMA^{-1}O. => Camera transform from O to AMA^{-1}O,

                // A = UnityToOpenCV, M =   CameraTransform_OpenCV


                float4x4 MatForObjectFrame = new float4x4(
                                            new float4(1, 0, 0, 0),
                                            new float4(0, 0, 1, 0),
                                            new float4(0, -1, 0, 0),
                                            new float4(0, 0, 0, 1) );

                float4x4 CameraTransform_Unity = math.mul(
                                                     math.mul(
                                                        math.mul(
                                                            UnityToOpenCV,
                                                            CameraTransform_OpenCV
                                                         ),
                                                      OpenCVToUnity //math.inverse(UnityToOpenCV)
                                                      ),

                                                     MatForObjectFrame
                                                     );
                


               // Debug.Log($"Determinimant of OpenCVToUnity\n{( (Matrix4x4)OpenCVToUnity).determinant}");
                

                Matrix4x4 CameraTransform_Unity_Mat4x4 = (Matrix4x4)CameraTransform_Unity;
                Debug.Log($"Determinimant of CameraTransform_Unity_Mat4x4=\n{CameraTransform_Unity_Mat4x4.determinant}");


                // Camera.main.gameObject.transform.position = GetPosition(CameraTransform_Unity_Mat4x4); 

                Camera.main.gameObject.transform.position  = new Vector3(0, mCameraHeight, 0);

                Debug.Log($"Quaternion = CameraTransform_Unity_Mat4x4.rotation=  \n {CameraTransform_Unity_Mat4x4.rotation}");
                Debug.Log($"QuaternionFromMatrix(MatForUnityCameraFrameMat4x4)\n{QuaternionFromMatrix(CameraTransform_Unity_Mat4x4)}");


                 Camera.main.gameObject.transform.rotation = GetRotation(CameraTransform_Unity_Mat4x4);
                // Camera.main.gameObject.transform.rotation = MatForUnityCameraFrameMat4x4.rotation;
                // quaternion quat = new quaternion(MatForUnityCameraFrame);

                // Camera.main.gameObject.transform.rotation = quat;                       

                //https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html?_ga=2.218542876.407438402.1604700797-1561115542.1585633305

          
                Debug.Log($"CameraTransform_Unity_Mat4x4= \n {CameraTransform_Unity_Mat4x4}");
                            

                Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
                    $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation}, " +
                   
                   // $"\nquaternion quat =\n{quat})" +
                    $"\neulerAngles ={ Camera.main.gameObject.transform.eulerAngles}");

            }

            else
            {   // use the default graphics camera
                Camera.main.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
                Camera.main.gameObject.transform.position = new Vector3(0, mCameraHeight, 0);

                Debug.Log($"camera position={  Camera.main.gameObject.transform.position.y}");
                Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
                   $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation} ");
                

            }


        }
        else
        {
            Debug.Log("the current gameobject and its component not are active; do not apply the texture in RayTracingMaster.cs");
        }

    }

    ////https://answers.unity.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
    //public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    //{
    //    // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
    //    Quaternion q = new Quaternion();
    //    q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
    //    q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
    //    q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
    //    q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
    //    q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
    //    q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
    //    q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
    //    return q;
    //}

    //It should be noted that if you're attempting to convert a valid worldToCameraMatrix to a quaternion rotation you'll need to adjust for the reversed z on camera and call something like:

    //Camera.main.transform.rotation = Quaternion.QuaternionFromMatrix(matrixToSet.inverse* Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1,1,-1)));

    public void OnSaveImage()
{
    Debug.Log($"FileName={CurrentInputField.textComponent.text}");

    DanbiImage.CaptureScreenToFileName(//currentSimulatorMode: SimulatorMode,
                                       convergedRT: ConvergedRenderTexForNewImage,
                                       //distortedResult: out DistortedResultImage,
                                       name: CurrentInputField.textComponent.text);
    #region    unused
    //mInputFieldObj.SetActive(true);
    //mInputFieldObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,0.5f);
    //mInputFieldObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0.5f)
    // mInputFieldObj.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

    // the position of the pivot of the rectform relative to its anchors (the center of the canvas)
    //mInputFieldObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);

    // InputField Input Caret is automatically added in front of placeHolder
    // so that placeHolder becomes the second child of InputFieldObj
    //GameObject placeHolder = mInputFieldObj.transform.GetChild(1).gameObject;
    //CurrentPlaceHolder.SetActive(true);
    #endregion
}
    //https://answers.unity.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html


    //https://answers.unity.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
    //static Quaternion LookRotation(Vector3 forward, [DefaultValue("Vector3.up")] Vector3 upwards);
    // The third column vector (z vector) (index 2)= forward, the second column vector (y vector) (index 1) = upward 

    // Extension methods should be defined in non-generic STATIC class
    //public static Vector3 ToVector3(this Vector4 parent)
    //{
    //    return new Vector3(parent.x, parent.y, parent.z);
    //    // Vector4.ToVector3()
    //}

    //public static Vector4 ToVector4Pos(this Vector3 parent)
    //{
    //    return new Vector4(parent.x, parent.y, parent.z,1);
    //}

    //public static Vector4 ToVector4Dir(this Vector3 parent)
    //{
    //    return new Vector4(parent.x, parent.y, parent.z, 0);
    //}


    public static Vector3 GetPosition(Matrix4x4 m)
    {
        return new Vector3(m[0, 3], m[1, 3], m[2, 3]);
    }

    public static Vector3 GetScale(Matrix4x4 m)
    {
        return new Vector3
            (m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
    }

    public static Quaternion GetRotation(Matrix4x4 m)
    {
        Vector3 s = GetScale(m);

        // Normalize Scale from Matrix4x4
        float m00 = m[0, 0] / s.x;
        float m01 = m[0, 1] / s.y;
        float m02 = m[0, 2] / s.z;
        float m10 = m[1, 0] / s.x;
        float m11 = m[1, 1] / s.y;
        float m12 = m[1, 2] / s.z;
        float m20 = m[2, 0] / s.x;
        float m21 = m[2, 1] / s.y;
        float m22 = m[2, 2] / s.z;

        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m00 + m11 + m22)) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m00 - m11 - m22)) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m00 + m11 - m22)) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m00 - m11 + m22)) / 2;
        q.x *= Mathf.Sign(q.x * (m21 - m12));
        q.y *= Mathf.Sign(q.y * (m02 - m20));
        q.z *= Mathf.Sign(q.z * (m10 - m01));

        // q.Normalize()
        float qMagnitude = Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
        q.w /= qMagnitude;
        q.x /= qMagnitude;
        q.y /= qMagnitude;
        q.z /= qMagnitude;

        return q;
    }

    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        Vector3 result;

        float test = q.x * q.y + q.z * q.w;
        // singularity at north pole
        if (test > 0.499)
        {
            result.x = 0;
            result.y = 2 * Mathf.Atan2(q.x, q.w);
            result.z = Mathf.PI / 2;
        }
        // singularity at south pole
        else if (test < -0.499)
        {
            result.x = 0;
            result.y = -2 * Mathf.Atan2(q.x, q.w);
            result.z = -Mathf.PI / 2;
        }
        else
        {
            result.x = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
            result.y = Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
            result.z = Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);

            if (result.x < 0) result.x += 360;
            if (result.y < 0) result.y += 360;
            if (result.z < 0) result.z += 360;
        }
        return result;
    }


}   // RayTracingMaster