//https://bitbucket.org/Daerst/gpu-ray-tracing-in-unity/src/master/

using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

using Danbi;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class RayTracingMaster : MonoBehaviour {

  protected bool bCaptureFinished;
  [SerializeField] protected bool bUseProjectionFromCameraCalibration = false;
  [SerializeField] protected EDanbiCalibrationMode UndistortMode;

  [SerializeField] protected float ThresholdIterative = 0.01f;
  [SerializeField] protected int SafeCounter = 5;

  [SerializeField] protected float ThresholdNewton = 0.1f;

  [SerializeField, Header("16:9 or 16:10")]
  protected EDanbiScreenAspects TargetScreenAspect = EDanbiScreenAspects.E_16_9;

  [SerializeField, Header("2K(2560 x 1440), 4K(3840 x 2160) or 8K(7680 x 4320)")]
  protected EDanbiScreenResolutions TargetScreenResolution = EDanbiScreenResolutions.E_4K;

  // TODO: Change this variable with Readonly Attribute.
  [SerializeField]
  protected Vector2Int CurrentScreenResolutions;

  protected int SizeMultiplier = 1;

  /// <summary>
  /// Panorama object of current simulation set
  /// </summary>
  protected List<PanoramaScreenObject> CurrentPanoramaList = new List<PanoramaScreenObject>();

  [SerializeField, Header("It affects to the Scene at editor-time and at run-time")]
  protected Texture2D TargetPanoramaTexFromImage; // It's set on the inspector.                               

  public Texture2D targetPanoramaTexFromImage { get { return TargetPanoramaTexFromImage; } set { TargetPanoramaTexFromImage = value; } }

  [SerializeField, Header("Ray-Tracer Compute Shader"), Space(10)]
  protected ComputeShader RTShader;

  [SerializeField, Header("2 by default for the best performance")]
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

  /// <summary>
  /// used to render the scene onto which the distorted image is projected.
  /// </summary>
  protected Camera UserCamera;

  [SerializeField, Header("Used for appending the name of the result image")]
  protected InputField SaveFileInputField;

  [SerializeField, Header("NONE, CATPTURE, PROJECTION, VIEW"), Space(20)]
  protected EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.CAPTURE;

  /// <summary>
  /// When this is true, then current renderTexture is transferred into the frame buffer.  
  /// </summary>    
  [SerializeField, Header("It toggled off to false after the image is saved.")]
  protected bool bPredistortedImageReady = false;

  // processing Button commands

  /// <summary>
  /// it's used to map the Result.
  /// </summary>
  protected RenderTexture ResultRenderTex;

  public RenderTexture ConvergedRenderTexForNewImage;
  //protected RenderTexture ConvergedRenderTexForProjecting;
  //protected RenderTexture ConvergedRenderTexForPresenting;

  //[SerializeField, Header("Result of current generated distorted image."), Space(20)]
  //protected Texture2D DistortedResultImage;

  //[SerializeField, Header("Result of current generated distorted image."), Header(20)]
  //Texture2D ProjectedResultImage;

  /// <summary>
  /// this refers to the result of projecting the distorted image
  /// </summary>
  protected RenderTexture Dbg_RWTex;

  //protected Texture2D ResultTex1;
  //protected Texture2D ResultTex2;
  //protected Texture2D ResTex3;


  /// <summary>
  /// this is supposed to be set in the inspector; 
  /// it would refer to the screen captured image
  /// of the process of creating the distorted image
  /// </summary>
  protected Material AddMaterial_WholeSizeScreenSampling;  // MJ: what is this? Remove all statements related to this.

  protected uint CurrentSamplingCountForRendering = 0;
  [SerializeField] protected uint MaxSamplingCountForRendering = 5;

  [SerializeField, Space(15)]
  DanbiCamAdditionalData ProjectedCamParams;
  protected ComputeBuffer CameraParamsForUndistortImageBuf;

  [SerializeField, Space(5)]
  Vector2Int ChessboardWidthHeight;

  protected List<Transform> TransformListToWatch = new List<Transform>();

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

  protected virtual void Start() {
    DanbiImage.ScreenResolutions = CurrentScreenResolutions;
    DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();

    CurrentInputField = SaveFileInputField.GetComponent<InputField>();

    CurrentInputField.onEndEdit.AddListener(
      val => {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
          bCaptureFinished = DanbiImage.CaptureScreenToFileName(currentSimulatorMode: SimulatorMode,
                                                                convergedRT: ConvergedRenderTexForNewImage,
                                                                //distortedResult: out DistortedResultImage,
                                                                name: CurrentInputField.textComponent.text);
        }
      }
    );

    Danbi.DanbiKernelHelper.AddKernalIndexWithKey((Danbi.EDanbiKernelKey.TriconeMirror_Img, RTShader.FindKernel("CreateImageTriConeMirror")),
                              //(Danbi.EDanbiKernelKey.GeoconeMirror_Img, RTShader.FindKernel("CreateImageGeoConeMirror")),
                              (Danbi.EDanbiKernelKey.ParaboloidMirror_Img, RTShader.FindKernel("CreateImageParaboloidMirror")),
                              (Danbi.EDanbiKernelKey.HemisphereMirror_Img, RTShader.FindKernel("CreateImageHemisphereMirror")),

                              (Danbi.EDanbiKernelKey.TriconeMirror_Proj, RTShader.FindKernel("ProjectImageTriConeMirror")),
                              (Danbi.EDanbiKernelKey.GeoconeMirror_Proj, RTShader.FindKernel("ProjectImageGeoConeMirror")),
                              (Danbi.EDanbiKernelKey.ParaboloidMirror_Proj, RTShader.FindKernel("ProjectImageParaboloidMirror")),
                              (Danbi.EDanbiKernelKey.HemisphereMirror_Proj, RTShader.FindKernel("ProjectImageHemisphereMirror")),

                              (Danbi.EDanbiKernelKey.PanoramaScreen_View, RTShader.FindKernel("ViewImageOnPanoramaScreen")),

                              (Danbi.EDanbiKernelKey.TriconeMirror_Img_With_Lens_Distortion, RTShader.FindKernel("CreateImageTriConeMirror_Img_With_Lens_Distortion")),
                              //(Danbi.EDanbiKernelKey.GeoconeMirror_Img_Undistorted, RTShader.FindKernel("CreateImageGeoConeMirror_Undistorted")),

                              (Danbi.EDanbiKernelKey.ParaboloidMirror_Img_With_Lens_Distortion, RTShader.FindKernel("CreateImageParaboloidMirror_Img_With_Lens_Distortion")),
                              (Danbi.EDanbiKernelKey.HemisphereMirror_Img_With_Lens_Distortion, RTShader.FindKernel("CreateImageHemisphereMirror_Img_With_Lens_Distortion")),
                              (Danbi.EDanbiKernelKey.HemisphereMirror_Img_RT, RTShader.FindKernel("CreateImageHemisphereMirror_RT"))
                              //, (KernalKey.TriconeMirror_Proj, CurrentRayTracerShader.FindKernel("ProjectImageTriConeMirror")),
                              //(KernalKey.GeoconeMirror_Proj, CurrentRayTracerShader.FindKernel("ProjectImageGeoConeMirror")),
                              //(KernalKey.ParaboloidMirror_Proj, CurrentRayTracerShader.FindKernel("ProjectImageParaboloidMirror")),
                              //(KernalKey.HemisphereMirror_Proj, CurrentRayTracerShader.FindKernel("ProjectImageHemisphereMirror")),
                              //(KernalKey.PanoramaScreen_View, CurrentRayTracerShader.FindKernel("ViewImageOnPanoramaScreen"))
                              );

    MainCamera = GetComponent<Camera>();
    TransformListToWatch.Add(transform);   // mainCamera

    //ResultTex1 = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);
    //ResultTex2 = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);
    //ResTex3 = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);


    RebuildObjectBuffers();

    #region unused
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
  }

  public void OnValidate() {
    // 1. Calculate Current screen resolutions by the screen aspects and the screen resolutions.
    //CurrentScreenResolutions = DanbiScreenHelper.GetScreenResolution(TargetScreenAspect, TargetScreenResolution);
    //CurrentScreenResolutions *= SizeMultiplier;

    // 2. Set the panorama material automatically by changing the texture.
    if (!enabled || !gameObject.activeInHierarchy || !gameObject.activeSelf) { return; }

    // 3. Apply the new target texture onto the scene and DanbiController both.
    ApplyNewTargetTexture(bCalledOnValidate: true, newTargetTex: TargetPanoramaTexFromImage);
  }

  protected virtual void OnDisable() {
    SphereBuf?.Release();
    MeshObjectBuf?.Release();
    VerticesBuf?.Release();
    IndicesBuf?.Release();

    Dbg_IntersectionRWBuf?.Release();
    Dbg_AccumRayEnergyRWBuf?.Release();
    Dbg_EmissionRWBuf?.Release();
    Dbg_SpecularRwBuf?.Release();
  }

  protected virtual void Update() {
    if (Input.GetKeyDown(KeyCode.Q)) {
      Utils.QuitEditorManually();
    }

    if (SimulatorMode == EDanbiSimulatorMode.PREPARE) { return; }

    foreach (var t in TransformListToWatch) {
      if (t.hasChanged) {
        CurrentSamplingCountForRendering = 0;
        // restart the ray tracing   when these transforms have been changed
        t.hasChanged = false;
      }
    }

    #region unused codes
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
  }

  #region Register|Unregister
  public static void RegisterObject(RayTracingObject obj) {
    Debug.Log("Raytracing Object registered");

    RayTracedObjectsList.Add(obj);
    bMeshObjectsNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }
  public static void UnregisterObject(RayTracingObject obj) {
    RayTracedObjectsList.Remove(obj);
    bMeshObjectsNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterTriangularConeMirror(TriangularConeMirrorObject obj) {
    Debug.Log("Triangular Cone Mirror registered");
    TriangularConeMirrorObjectsList.Add(obj);
    bConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterTriangularConeMirror(TriangularConeMirrorObject obj) {
    TriangularConeMirrorObjectsList.Remove(obj);
    bConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterPyramidMirror(PyramidMirrorObject obj) {
    Debug.Log("Pyramid Mirror registered");
    PyramidMirrorObjectsList.Add(obj);
    bPyramidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterPyramidMirror(PyramidMirrorObject obj) {
    PyramidMirrorObjectsList.Remove(obj);
    bPyramidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterParaboloidMirror(ParaboloidMirrorObject obj) {
    Debug.Log("Paraboloid Mirror registered");
    ParaboloidMirrorObjectsList.Add(obj);
    bParaboloidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterParaboloidMirror(ParaboloidMirrorObject obj) {
    ParaboloidMirrorObjectsList.Remove(obj);
    bParaboloidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterHemisphereMirror(HemisphereMirrorObject obj) {
    Debug.Log("Hemisphere Mirror registered");
    HemisphereMirrorObjectsList.Add(obj);
    bHemisphereMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterHemisphereMirror(HemisphereMirrorObject obj) {
    HemisphereMirrorObjectsList.Remove(obj);
    bHemisphereMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterGeoConeMirror(GeoConeMirrorObject obj) {
    Debug.Log("Geometric Cone Mirror registered");
    GeoConeMirrorObjectsList.Add(obj);
    bGeoConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterGeoConeMirror(GeoConeMirrorObject obj) {
    GeoConeMirrorObjectsList.Remove(obj);
    bGeoConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterPanoramaMesh(PanoramaScreenObject obj) {
    Debug.Log("panorama Mesh registered");
    PanoramaSreenObjectsList.Add(obj);
    bPanoramaMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterPanoramaMesh(PanoramaScreenObject obj) {
    PanoramaSreenObjectsList.Remove(obj);
    bPanoramaMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }
  #endregion

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

  /// <summary>
  /// need to transfer this to the dbg class.
  /// </summary>
  protected void CreateDebugBuffers() {
    //// for debugging
    //_meshObjectArray = new MeshObjectRW[_meshObjects.Count];

    //int meshObjRWStride = 16 * sizeof(float) + sizeof(float)
    //               + 3 * 3 * sizeof(float);

    //_meshObjectBufferRW = new ComputeBuffer(_meshObjects.Count, meshObjRWStride);

    ////ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.

    //Dbg_VerticesRWBuf = new ComputeBuffer(VerticesList.Count, 3 * sizeof(float), ComputeBufferType.Default);
    //Dbg_IntersectionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
    //Dbg_RayDirectionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
    //Dbg_IntersectionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
    //Dbg_AccumRayEnergyRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
    //Dbg_EmissionRWBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);
    //Dbg_SpecularRwBuf = new ComputeBuffer(CurrentScreenResolutions.x * CurrentScreenResolutions.y, 4 * sizeof(float), ComputeBufferType.Default);

    //Deb_VerticeArr = new Vector3[VerticesList.Count];
    //Dbg_RayDirectionArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
    //Dbg_IntersectionArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
    //Dbg_AccumulatedRayEnergyArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
    //Dbg_EmissionArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];
    //Dbg_SpecularArr = new Vector4[CurrentScreenResolutions.x * CurrentScreenResolutions.y];

    ////The static Array.Clear() method "sets a range of elements in the Array to zero, to false, or to Nothing, depending on the element type".If you want to clear your entire array, you could use this method an provide it 0 as start index and myArray.Length as length:
    //// Array.Clear(mUVMapArray, 0, mUVMapArray.Length);
    //// _meshObjectBufferRW.SetData(_meshObjectArray);

    //Dbg_VerticesRWBuf.SetData(Deb_VerticeArr);
    //Dbg_RayDirectionRWBuf.SetData(Dbg_RayDirectionArr);
    //Dbg_IntersectionRWBuf.SetData(Dbg_IntersectionArr);
    //Dbg_AccumRayEnergyRWBuf.SetData(Dbg_AccumulatedRayEnergyArr);
    //Dbg_EmissionRWBuf.SetData(Dbg_EmissionArr);
    //Dbg_SpecularRwBuf.SetData(Dbg_SpecularArr);
  }

  #region  Rebuild buffers
  void RebuildObjectBuffers() {
    if (!bObjectsNeedRebuild) {
      Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
      Debug.Log("The mesh objects are already built");
      Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");

      return;
    }


    bObjectsNeedRebuild = false;

    CurrentSamplingCountForRendering = 0;

    // Clear all lists
    //_meshObjects.Clear();
    VerticesList.Clear();
    IndicesList.Clear();
    TexcoordsList.Clear();

    // commented out by Moon Jung          

    // Only one mirror should be defined. Otherwise the order is defined as in the following code.

    //if (_pyramidMirrorObjects.Count != 0 && _triangularConeMirrorObjects.Count != 0
    //    || _pyramidMirrorObjects.Count != 0 && _geoConeMirrorObjects.Count != 0
    //    || _pyramidMirrorObjects.Count != 0 && _paraboloidMirrorObjects.Count != 0
    //    || _triangularConeMirrorObjects.Count != 0 && _geoConeMirrorObjects.Count != 0
    //    || _triangularConeMirrorObjects.Count != 0 && _paraboloidMirrorObjects.Count != 0
    //     || _geoConeMirrorObjects.Count != 0 && _paraboloidMirrorObjects.Count != 0)
    //{
    //    Debug.LogError("Only one mirror should be defined in the scene");
    //    StopPlay();
    //}

    bool mirrorDefined = false;

    if (bUseProjectionFromCameraCalibration) {
      CreateComputeBuffer<DanbiCamAdditionalData>(ref CameraParamsForUndistortImageBuf,
                                                new List<DanbiCamAdditionalData>() { ProjectedCamParams },
                                                40);
    }

    if (PyramidMirrorObjectsList.Count != 0) {
      RebuildPyramidMirrorBuffer();
      mirrorDefined = true;
    } else if (TriangularConeMirrorObjectsList.Count != 0) {
      RebuildTriangularConeMirrorBuffer();
      mirrorDefined = true;
    } else if (GeoConeMirrorObjectsList.Count != 0) {
      RebuildGeoConeMirrorBuffer();
      mirrorDefined = true;
    } else if (ParaboloidMirrorObjectsList.Count != 0) {
      RebuildParaboloidMirrorBuffer();
      mirrorDefined = true;
    } else if (HemisphereMirrorObjectsList.Count != 0) {
      RebuildHemisphereMirrorBuffer();
      mirrorDefined = true;
    }
    // Either panoramaScreenObject or panoramaMeshObject should be defined
    // so that the projector image will be projected onto it.

    if (!mirrorDefined) {
      Debug.LogError("A mirror should be defined");
      Utils.StopPlayManually();
    }

    if (PanoramaSreenObjectsList.Count != 0) {
      RebuildPanoramaMeshBuffer();
    } else {
      Debug.LogError(" panoramaMeshObject should be defined\n" +
                     "so that the projector image will be projected onto it.");
      Utils.StopPlayManually();
    }

    //if (_meshObjects.Count != 0)
    if (RayTracedObjectsList.Count != 0) {
      RebuildMeshObjectBuffer();
    }

    // create computeBuffers holding the vertices information about the various
    // objects created by the above RebuildXBuffer()'s
    CreateComputeBuffer(buffer: ref VerticesBuf, data: VerticesList, stride: 12);
    CreateComputeBuffer(buffer: ref IndicesBuf, data: IndicesList, stride: 4);
    CreateComputeBuffer(buffer: ref TexcoordsBuf, data: TexcoordsList, stride: 8);

    #region debug
    CreateDebugBuffers();
    #endregion
  }  // RebuildObjectBuffers()

  void RebuildObjectBuffersWithoutMirror() {
    if (!bObjectsNeedRebuild) {
      return;
    }

    bObjectsNeedRebuild = false;

    CurrentSamplingCountForRendering = 0;

    // Clear all lists
    //_meshObjects.Clear();
    VerticesList.Clear();
    IndicesList.Clear();
    TexcoordsList.Clear();


    // Either panoramaScreenObject or panoramaMeshObject should be defined
    // so that the projector image will be projected onto it.


    if (PanoramaSreenObjectsList.Count != 0) {
      RebuildPanoramaMeshBuffer();
    } else {
      Debug.LogError(" panoramaMeshObject should be defined\n" +
                     "so that the projector image will be projected onto it.");
      Utils.StopPlayManually();

    }



    if (RayTracedObjectsList.Count != 0) {
      RebuildMeshObjectBuffer();
    }


    // create computeBuffers holding the vertices information about the various
    // objects created by the above RebuildXBuffer()'s

    CreateComputeBuffer(ref VerticesBuf, VerticesList, 12);
    CreateComputeBuffer(ref IndicesBuf, IndicesList, 4);
    CreateComputeBuffer(ref TexcoordsBuf, TexcoordsList, 8);

    #region debug
    CreateDebugBuffers();
    #endregion


  }  // RebuildObjectBuffersWithoutMirror()

  void RebuildMeshObjectBuffer() {
    if (!bMeshObjectsNeedRebuild) {
      return;
    }


    bMeshObjectsNeedRebuild = false;
    // _currentSample = 0;

    //// Clear all lists

    RayTracedMeshObjectsList.Clear();
    //_vertices.Clear();
    //_indices.Clear();
    //_texcoords.Clear();


    // Loop over all objects and gather their data


    foreach (var obj in RayTracedObjectsList) {

      string objectName = obj.objectName;
      // Debug.Log("mesh object=" + objectName);

      var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

      //Debug.Log( (cnt++)  + "th mesh:");
      //for (int i = 0; i < mesh.vertices.Length; i++)
      //{
      //    Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

      //}
      // Ways to get other components (sibling components) of the gameObject to which 
      // this component is attached:
      // this.GetComponent<T>, where this is a component class
      // this.gameObject.GetComponent<T> does the same thing

      // Add vertex data
      // get the current number of vertices in the vertex list
      int firstVertex = VerticesList.Count;  // The number of vertices so far created; will be used
                                             // as the index of the first vertex of the newly created mesh
      VerticesList.AddRange(mesh.vertices);

      // Add index data - if the vertex buffer wasn't empty before, the
      // indices need to be offset
      int firstIndex = IndicesList.Count; // the current count of _indices  list; will be used
                                          // as the index offset in _indices for the newly created mesh
      int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                          // when the mesh topology is triangle;
                                          // indices will contain a multiple of three indices
                                          // our mesh is actually a triangular mesh.

      // show the local coordinates of the triangles
      //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 

      //  Debug.Log("triangle vertex (local) =(" + mesh.vertices[indices[i]].ToString("F6")
      //            + "," + mesh.vertices[indices[i + 1]].ToString("F6")
      //            + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");
      //}

      // Change the order of the vertex index in indices : DO NOT DO IT
      //for (int i = 0; i < indices.Length; i+=3)
      //{   // a triangle v0,v1,v2 => v2, v1, v0
      //    int intermediate = indices[i];   // indices[i+1] does not change
      //    indices[i] = indices[i + 2];
      //    indices[i + 2] = intermediate;

      //}
      IndicesList.AddRange(indices.Select(index => index + firstVertex));


      // Add Texcoords data.
      TexcoordsList.AddRange(mesh.uv);

      // Add the object itself
      RayTracedMeshObjectsList.Add(new MeshObject() {
        localToWorldMatrix = obj.transform.localToWorldMatrix,
        albedo = obj.MeshOpticalProp.albedo,

        specular = obj.MeshOpticalProp.specular,
        smoothness = obj.MeshOpticalProp.smoothness,
        emission = obj.MeshOpticalProp.emission,

        indices_offset = firstIndex,
        indices_count = indices.Length // set the index count of the mesh of the current obj
      }
      );

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

  void RebuildTriangularConeMirrorBuffer() {
    // if obj.mirrorType is the given mirrorType
    if (!bConeMirrorNeedRebuild) {
      return;
    }

    bConeMirrorNeedRebuild = false;

    // Clear all lists
    TriangularConeMirrorsList.Clear();
    //_triangularConeMirrorVertices.Clear();
    //_triangularConeMirrorIndices.Clear();


    // Clear all lists
    //_meshObjects.Clear();
    //_vertices.Clear();
    //_indices.Clear();
    //_texcoords.Clear();


    var obj = TriangularConeMirrorObjectsList[0];

    string objectName = obj.objectName;
    // _mirrorType = obj.mirrorType;

    //Debug.Log("mirror object=" + objectName);

    var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

    //Debug.Log((cnt++) + "th mesh:");
    //for (int i = 0; i < mesh.vertices.Length; i++) {
    //  Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

    //}
    // Ways to get other components (sibling components) of the gameObject to which 
    // this component is attached:
    // this.GetComponent<T>, where this is a component class
    // this.gameObject.GetComponent<T> does the same thing

    // Add vertex data
    // get the current number of vertices in the vertex list
    int firstVertexIndex = VerticesList.Count;  // The number of vertices so far created; will be used
                                                // as the index of the first vertex of the newly created mesh
    VerticesList.AddRange(mesh.vertices);
    // Add Texcoords data.
    TexcoordsList.AddRange(mesh.uv);

    // Add index data - if the vertex buffer wasn't empty before, the
    // indices need to be offset
    int countOfCurrentIndices = IndicesList.Count; // the current count of _indices  list; will be used
                                                   // as the index offset in _indices for the newly created mesh
    int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                        // when the mesh topology is triangle;
                                        // indices will contain a multiple of three indices
                                        // our mesh is actually a triangular mesh.

    // show the local coordinates of the triangles
    //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 
    //  Debug.Log("triangular Mirror: triangle indices (local) =(" + mesh.vertices[indices[i]].ToString("F6")
    //            + "," + mesh.vertices[indices[i + 1]].ToString("F6")
    //            + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");
    //}

    // Change the order of the vertex index in indices: DO NOT DO IT
    //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 => v2, v1, v0
    //  int intermediate = indices[i];   // indices[i+1] does not change
    //  indices[i] = indices[i + 2];
    //  indices[i + 2] = intermediate;
    //}
    //}
    IndicesList.AddRange(indices.Select(index => index + firstVertexIndex));


    // Add Texcoords data.
    //_texcoords.AddRange(mesh.uv);

    // Add the object itself
    TriangularConeMirrorsList.Add(new TriangularConeMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,

      distanceToOrigin = obj.mConeParam.distanceFromCamera,
      height = obj.mConeParam.height,
      radius = obj.mConeParam.radius,
      albedo = obj.MeshOpticalProp.albedo,

      specular = obj.MeshOpticalProp.specular,
      smoothness = obj.MeshOpticalProp.smoothness,
      emission = obj.MeshOpticalProp.emission,
      indices_offset = countOfCurrentIndices,
      indices_count = indices.Length // set the index count of the mesh of the current obj
    }
    );

    //      public struct TriangularConeMirror
    //{
    //    public Matrix4x4 localToWorldMatrix;

    //    public float distanceToOrigin;
    //    public float height;    
    //    public float radius;

    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public int indices_offset;
    //    public int indices_count;
    //}



    int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                 + 5 * sizeof(float) + 2 * sizeof(int) - 4;

    // create a computebuffer and set the data to it

    CreateComputeBuffer(ref TriangularConeMirrorBuf, TriangularConeMirrorsList, stride);
  }   // RebuildTriangularConeMirrorBuffer()

  void RebuildHemisphereMirrorBuffer() {
    // if obj.mirrorType is the given mirrorType
    if (!bHemisphereMirrorNeedRebuild) {
      return;
    }

    bHemisphereMirrorNeedRebuild = false;

    // Clear all lists
    HemisphereMirrorsList.Clear();
    //_triangularConeMirrorVertices.Clear();
    //_triangularConeMirrorIndices.Clear();

    // Clear all lists
    //_meshObjects.Clear();
    //_vertices.Clear();
    //_indices.Clear();
    //_texcoords.Clear();

    var obj = HemisphereMirrorObjectsList[0];

    string objectName = obj.objectName;
    // _mirrorType = obj.mirrorType;

    //Debug.Log("mirror object=" + objectName);

    var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

    //Debug.Log((cnt++) + "th mesh:");
    //for (int i = 0; i < mesh.vertices.Length; i++) {
    //  Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

    //}
    // Ways to get other components (sibling components) of the gameObject to which 
    // this component is attached:
    // this.GetComponent<T>, where this is a component class
    // this.gameObject.GetComponent<T> does the same thing

    // Add vertex data
    // get the current number of vertices in the vertex list
    int firstVertexIndex = VerticesList.Count;  // The number of vertices so far created; will be used
                                                // as the index of the first vertex of the newly created mesh
    VerticesList.AddRange(mesh.vertices);
    // Add Texcoords data.
    TexcoordsList.AddRange(mesh.uv);

    // Add index data - if the vertex buffer wasn't empty before, the
    // indices need to be offset
    int countOfCurrentIndices = IndicesList.Count; // the current count of _indices  list; will be used
                                                   // as the index offset in _indices for the newly created mesh
    int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                        // when the mesh topology is triangle;
                                        // indices will contain a multiple of three indices
                                        // our mesh is actually a triangular mesh.

    // show the local coordinates of the triangles
    //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 
    //  Debug.Log("triangular Mirror: triangle indices (local) =(" + mesh.vertices[indices[i]].ToString("F6")
    //            + "," + mesh.vertices[indices[i + 1]].ToString("F6")
    //            + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");
    //}

    // Change the order of the vertex index in indices: DO NOT DO IT
    //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 => v2, v1, v0
    //  int intermediate = indices[i];   // indices[i+1] does not change
    //  indices[i] = indices[i + 2];
    //  indices[i + 2] = intermediate;
    //}
    //}
    IndicesList.AddRange(indices.Select(index => index + firstVertexIndex));


    // Add Texcoords data.
    //_texcoords.AddRange(mesh.uv);

    // Add the object itself
    HemisphereMirrorsList.Add(new HemisphereMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,

      distanceToOrigin = obj.HemiSphereParam.distanceFromCamera,
      height = obj.HemiSphereParam.height,
      radius = obj.HemiSphereParam.radius,
      albedo = obj.MeshOpticalProp.albedo,

      specular = obj.MeshOpticalProp.specular,
      smoothness = obj.MeshOpticalProp.smoothness,
      emission = obj.MeshOpticalProp.emission,
      //indices_offset = countOfCurrentIndices,
      //indices_count = indices.Length // set the index count of the mesh of the current obj
    }
    );

    //      public struct TriangularConeMirror
    //{
    //    public Matrix4x4 localToWorldMatrix;

    //    public float distanceToOrigin;
    //    public float height;    
    //    public float radius;

    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public int indices_offset;
    //    public int indices_count;
    //}

    //int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
    //             + 5 * sizeof(float) + 2 * sizeof(int);

    int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                + 5 * sizeof(float) - 4;
    // create a compute buffer and set the data to it

    CreateComputeBuffer(ref HemisphereMirrorBuf,
                          HemisphereMirrorsList, stride);
  }   // RebuildHemisphereMirrorBuffer()

  void RebuildPyramidMirrorBuffer() {
    if (!bPyramidMeshObjectNeedRebuild) {
      return;
    }

    bPyramidMeshObjectNeedRebuild = false;


    // Clear all lists
    PyramidMirrorsList.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = PyramidMirrorObjectsList[0];
    // _mirrorType = obj.mMirrorType;



    // Add the object itself
    PyramidMirrorsList.Add(new PyramidMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,
      albedo = obj.MeshOpticalProp.albedo,

      specular = obj.MeshOpticalProp.specular,
      smoothness = obj.MeshOpticalProp.smoothness,
      emission = obj.MeshOpticalProp.emission,
      height = obj.mPyramidParam.height,
      width = obj.mPyramidParam.width,
      depth = obj.mPyramidParam.depth,
    }
    );



    //    public struct PyramidMirror
    //{
    //    public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public float height;
    //    public float width;  // the radius of the base of the cone
    //    public float depth;

    //};


    // stride = sizeof(Matrix4x4) + 4 * sizeof(float) + 3 * sizeof(Vector3) + sizeof(int)

    int pyramidMirrorStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                              + 4 * sizeof(float) - 4;

    CreateComputeBuffer(ref PyramidMirrorBuf, PyramidMirrorsList, pyramidMirrorStride);

  }   // RebuildPyramidMirrorObjectBuffer()

  void RebuildGeoConeMirrorBuffer() {
    if (!bGeoConeMirrorNeedRebuild) {
      return;
    }

    bGeoConeMirrorNeedRebuild = false;


    // Clear all lists
    GeoConeMirrorsList.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = GeoConeMirrorObjectsList[0];
    // _mirrorType = obj.mMirrorType;

    // Add the object itself
    GeoConeMirrorsList.Add(
      new GeoConeMirror() {
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
                                  + 5 * sizeof(float) - 4;

    CreateComputeBuffer(ref GeoConeMirrorBuf, GeoConeMirrorsList, geoConeMirrorStride);



  }    //RebuildGeoConeMirrorBuffer()

  void RebuildParaboloidMirrorBuffer() {
    if (!bParaboloidMeshObjectNeedRebuild) {
      return;
    }

    bParaboloidMeshObjectNeedRebuild = false;

    // Clear all lists
    ParaboloidMirrorsList.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = ParaboloidMirrorObjectsList[0];
    // _mirrorType = obj.mMirrorType;



    // Add the object itself
    ParaboloidMirrorsList.Add(
      new ParaboloidMirror() {
        localToWorldMatrix = obj.transform.localToWorldMatrix,
        distanceToOrigin = obj.mParaboloidParam.distanceFromCamera,
        height = obj.mParaboloidParam.height,
        albedo = obj.MeshOpticalProp.albedo,

        specular = obj.MeshOpticalProp.specular,
        smoothness = obj.MeshOpticalProp.smoothness,
        emission = obj.MeshOpticalProp.emission,

        coefficientA = obj.mParaboloidParam.coefficientA,
        coefficientB = obj.mParaboloidParam.coefficientB,

      }
    );

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
                                  + 6 * sizeof(float) - 4;
    CreateComputeBuffer(ref ParaboloidMirrorBuf, ParaboloidMirrorsList, paraboloidMirrorStride);

  }   // RebuildParaboloidMirrorObjectBuffer()

  void RebuildPanoramaMeshBuffer() {
    if (!bPanoramaMeshObjectNeedRebuild) {
      return;
    }

    bPanoramaMeshObjectNeedRebuild = false;

    foreach (var i in PanoramaSreenObjectsList) {
      // Loop over all objects and gather their data
      //foreach (RayTracingObject obj in _rayTracingObjects)
      var mesh = i.GetComponent<MeshFilter>().sharedMesh;

      //Debug.Log((cnt++) + "th mesh:");
      //for (int i = 0; i < mesh.vertices.Length; i++) {
      //  Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

      //}
      // Ways to get other components (sibling components) of the gameObject to which 
      // this component is attached:
      // this.GetComponent<T>, where this is a component class
      // this.gameObject.GetComponent<T> does the same thing

      // Add vertex data
      // get the current number of vertices in the vertex list
      int firstVertexIndex = VerticesList.Count;  // The number of vertices so far created; will be used
                                                  // as the index of the first vertex of the newly created mesh
      VerticesList.AddRange(mesh.vertices);
      // Add Texcoords data.
      TexcoordsList.AddRange(mesh.uv);

      // Add index data - if the vertex buffer wasn't empty before, the
      // indices need to be offset
      int countOfCurrentIndices = IndicesList.Count; // the current count of _indices  list; will be used
                                                     // as the index offset in _indices for the newly created mesh
      int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                          // when the mesh topology is triangle;
                                          // indices will contain a multiple of three indices
                                          // our mesh is actually a triangular mesh.

      //// show the local coordinates of the triangles
      //for (int i = 0; i < indices.Length; i += 3)
      //{   // a triangle v0,v1,v2 
      //    Debug.Log("Panorama: vertex (local) =(" + mesh.vertices[indices[i]].ToString("F6")
      //              + "," + mesh.vertices[indices[i + 1]].ToString("F6")
      //              + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");


      //    Debug.Log("panorama: uv coord =(" + _texcoords[indices[i]].ToString("F6")
      //            + "," + _texcoords[indices[i + 1]].ToString("F6")
      //            + "," + _texcoords[indices[i + 2]].ToString("F6") + ")");

      //}

      // Change the order of the vertex index in indices: DO NOT DO IT
      //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 => v2, v1, v0
      //  int intermediate = indices[i];   // indices[i+1] does not change
      //  indices[i] = indices[i + 2];
      //  indices[i + 2] = intermediate;
      //}
      //}
      IndicesList.AddRange(indices.Select(index => index + firstVertexIndex));




      // Add the object itself
      PanoramaScreensList.Add(
        new PanoramaScreen() {
          localToWorldMatrix = i.transform.localToWorldMatrix,
          highRange = i.panoramaParams.highRangeFromCamera,
          lowRange = i.panoramaParams.lowRangeFromCamera,
          albedo = i.meshMaterialProp.albedo,

          specular = i.meshMaterialProp.specular,
          smoothness = i.meshMaterialProp.smoothness,
          emission = i.meshMaterialProp.emission,

          indices_offset = countOfCurrentIndices,
          indices_count = indices.Length // set the index count of the mesh of the current obj
        }
      );


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
    }
  }   // RebuildPanoramaMeshBuffer()

  #endregion

  protected static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride) where T : struct {
    // Do we already have a compute buffer?
    if (!ReferenceEquals(buffer, null)) {
      // If no data or buffer doesn't match the given criteria, release it
      if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to begin with, create it
      if (ReferenceEquals(buffer, null)) {
        buffer = new ComputeBuffer(data.Count, stride);
      }

      // Set data on the buffer
      buffer.SetData(data);
    }
    // buffer is not created, and remains to be null
  }

  protected void SetShaderFrameParameters() {
    if (SimulatorMode == EDanbiSimulatorMode.PREPARE) { return; }

    var pixelOffset = new Vector2(Random.value, Random.value);
    RTShader.SetVector("_PixelOffset", pixelOffset);

    //Debug.Log("_PixelOffset =" + pixelOffset);
    //float seed = Random.value;
    //RayTracingShader.SetFloat("_Seed", seed);
    //Debug.Log("_Seed =" + seed);
  }   //SetShaderFrameParameters()

  protected void SetDbgBufsToShader() {
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_VertexBufferRW", Dbg_VerticesRWBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_RayDirectionBuffer", Dbg_RayDirectionRWBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_IntersectionBuffer", Dbg_IntersectionRWBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_AccumRayEnergyBuffer", Dbg_AccumRayEnergyRWBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_EmissionBuffer", Dbg_EmissionRWBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_SpecularBuffer", Dbg_SpecularRwBuf);
  }

  protected void InitRenderTextureForCreateImage() {

    //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
    // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)    

    if (ResultRenderTex == null) {
      // Create the camera's render target for Ray Tracing
      //_Target = new RenderTexture(Screen.width, Screen.height, 0,
      ResultRenderTex = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

      //MOON: Change CurrentScreenResolution to Projector Width and Height

      //Render Textures can also be written into from compute shaders,
      //if they have “random access” flag set(“unordered access view” in DX11).

      ResultRenderTex.enableRandomWrite = true;
      ResultRenderTex.Create();

    }
    if (ConvergedRenderTexForNewImage == null) {
      //_converged = new RenderTexture(Screen.width, Screen.height, 0,
      ConvergedRenderTexForNewImage = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
                                     RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      ConvergedRenderTexForNewImage.enableRandomWrite = true;
      ConvergedRenderTexForNewImage.Create();
    }

    //DistortedResultImage = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);
    // TODO: divide the dbg part from the used part.
    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    //Dbg_RWTex = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
    //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //Dbg_RWTex.enableRandomWrite = true;
    //Dbg_RWTex.Create();


    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    //_MainScreenRT = new RenderTexture(Screen.width, Screen.height, 0,
    //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //_MainScreenRT.enableRandomWrite = true;
    //_MainScreenRT.Create();

    // Reset sampling
    CurrentSamplingCountForRendering = 0;
  }  //InitRenderTextureForCreateImage()

  //protected void InitRenderTextureForProjectImage() {

  //  //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
  //  // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)



  //  if (ResultRenderTex == null) {

  //    // Create the camera's render target for Ray Tracing
  //    //_Target = new RenderTexture(Screen.width, Screen.height, 0,
  //    ResultRenderTex = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
  //                                 RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

  //    //Render Textures can also be written into from compute shaders,
  //    //if they have “random access” flag set(“unordered access view” in DX11).

  //    ResultRenderTex.enableRandomWrite = true;
  //    ResultRenderTex.Create();

  //  }

  //  if (ConvergedRenderTexForProjecting == null) {
  //    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
  //    ConvergedRenderTexForProjecting = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
  //                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //    ConvergedRenderTexForProjecting.enableRandomWrite = true;
  //    ConvergedRenderTexForProjecting.Create();

  //  }
  //  //_converged = new RenderTexture(Screen.width, Screen.height, 0,

  //  //ProjectedResultImage = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y, TextureFormat.RGBAFloat, false);
  //  //_PredistortedImage = new RenderTexture(ScreenWidth, ScreenHeight, 0,
  //  //                              RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

  //  ////Make _PredistortedImage to be a random access texture
  //  //_PredistortedImage.enableRandomWrite = true;
  //  //_PredistortedImage.Create();

  //  //_converged = new RenderTexture(Screen.width, Screen.height, 0,
  //  Dbg_RWTex = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
  //                                 RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //  Dbg_RWTex.enableRandomWrite = true;
  //  Dbg_RWTex.Create();

  //  //_converged = new RenderTexture(Screen.width, Screen.height, 0,
  //  //_MainScreenRT = new RenderTexture(Screen.width, Screen.height, 0,
  //  //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //  //_MainScreenRT.enableRandomWrite = true;
  //  //_MainScreenRT.Create();

  //  // Reset sampling
  //  CurrentSamplingCountForRendering = 0;

  //}  //InitRenderTextureForProjectImage()

  //protected void InitRenderTextureForViewImage() {

  //  //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
  //  // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)

  //  if (ResultRenderTex == null) {

  //    // Create the camera's render target for Ray Tracing
  //    //_Target = new RenderTexture(Screen.width, Screen.height, 0,
  //    ResultRenderTex = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
  //                                 RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

  //    //Render Textures can also be written into from compute shaders,
  //    //if they have “random access” flag set(“unordered access view” in DX11).

  //    ResultRenderTex.enableRandomWrite = true;
  //    ResultRenderTex.Create();
  //  }

  //  if (ConvergedRenderTexForPresenting == null) {
  //    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
  //    ConvergedRenderTexForPresenting = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
  //                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //    ConvergedRenderTexForPresenting.enableRandomWrite = true;
  //    ConvergedRenderTexForPresenting.Create();

  //  }


  //  //_ProjectedImage = new RenderTexture(ScreenWidth, ScreenHeight, 0,
  //  //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //  //_ProjectedImage.enableRandomWrite = true;
  //  //_ProjectedImage.Create();

  //  //_converged = new RenderTexture(Screen.width, Screen.height, 0,
  //  Dbg_RWTex = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 0,
  //                                 RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //  Dbg_RWTex.enableRandomWrite = true;
  //  Dbg_RWTex.Create();




  //  //_converged = new RenderTexture(Screen.width, Screen.height, 0,
  //  //_MainScreenRT = new RenderTexture(Screen.width, Screen.height, 0,
  //  //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
  //  //_MainScreenRT.enableRandomWrite = true;
  //  //_MainScreenRT.Create();

  //  // Reset sampling
  //  CurrentSamplingCountForRendering = 0;



  //}  //InitRenderTextureForViewImage()


  protected void OnRenderImage(RenderTexture source, RenderTexture destination) {
    if (SimulatorMode == EDanbiSimulatorMode.PREPARE) { return; }

    // SimulatorMode is changed when OnInitCreateDistortedImage() is called.
    // (the moment of which Parameters for Compute shader and Textures are prepared)
    if (SimulatorMode == EDanbiSimulatorMode.CAPTURE) {
      if (bPredistortedImageReady)  // bStopRender is true when a task is completed and another task is not selected (OnSaveImage())
                                    // In this situation, the frame buffer is not updated, but the same content is transferred to the framebuffer
                                    // to make the screen alive
      {
        //Debug.Log("current sample not incremented =" + CurrentSamplingCountForRendering);
        //Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

        // Ignore the target Texture of the camera in order to blit to the null target (which is
        // the frame buffer
        //the destination (frame buffer= null) has a resolution of Screen.width x Screen.height
        //Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);
        Graphics.Blit(ConvergedRenderTexForNewImage, destination);
      } else {
        //Debug.Log("current sample=" + _currentSample);

        int threadGroupsX = Mathf.CeilToInt(CurrentScreenResolutions.x * 0.125f); // same as (/ 8).
        int threadGroupsY = Mathf.CeilToInt(CurrentScreenResolutions.y * 0.125f);

        //Different mKernelToUse is used depending on the task, that is, on the value
        // of _CaptureOrProjectOrView

        RTShader.Dispatch(Danbi.DanbiKernelHelper.CurrentKernelIndex, threadGroupsX, threadGroupsY, 1);
        // This dispatch of the compute shader will set _Target TWTexure2D

        if (AddMaterial_WholeSizeScreenSampling == null) {
          AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
        }

        AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCountForRendering);

        // TODO: Upscale To 4K and downscale to 1k.
        //_Target is the RWTexture2D created by the compute shader
        // note that _cameraMain.targetTexture = _convergedForCreateImage by OnPreRender(); =>
        // not used right now.

        // Blit (source, dest, material) sets dest as the render target, and source as _MainTex property
        // on the material and draws a full-screen quad.
        //If  dest == null, the screen backbuffer is used as
        // the blit destination, EXCEPT if the Camera.main has a non-null targetTexture;
        // If the Camera.main has a non-null targetTexture, it will be the target even if 
        // dest == null.

        Graphics.Blit(ResultRenderTex, ConvergedRenderTexForNewImage, AddMaterial_WholeSizeScreenSampling);

        // to improve the resolution of the result image, We need to use Converged Render Texture (upscaled in float precision).
        //Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);
        Graphics.Blit(ConvergedRenderTexForNewImage, destination);

        // Ignore the target Texture of the camera in order to blit to the null target which it is the framebuffer.
        ++CurrentSamplingCountForRendering;
        // bStopRender becomes true in 2 cases.
        // this is the second case.
        if (CurrentSamplingCountForRendering > MaxSamplingCountForRendering) {
          Debug.Log($"Ready to finish distorted image!", this);
          bPredistortedImageReady = true;
          CurrentSamplingCountForRendering = 0;
        }

        // Each cycle of rendering, a new location within every pixel area is sampled 
        // for the purpose of  anti-aliasing.
      } // else of if (mPauseNewRendering)
    }

    #region 
    //else if (SimulatorMode == 1) {
    //  // used the result of the rendering (raytracing shader)
    //  //debug
    //  // RayTracingShader.SetTexture(mKernelToUse, "_Result", _Target);
    //  CurrentRayTracerShader.SetTexture(mKernelToUse, "_DebugRWTexture", Dbg_RWTex);

    //  if (bStopRender)  // PauseNewRendering is true during saving image                                  // to make the screen alive

    //  {
    //    Debug.Log("current sample not incremented =" + CurrentSamplingCount);
    //    // Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

    //    // Null the target Texture of the camera and blit to the null target (which is
    //    // the framebuffer

    //    // _cameraMain.targetTexture = null; // tells Blit to ignore the currently active target render texture
    //    //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
    //    Graphics.Blit(ConvergedRenderTexForProjecting, default(RenderTexture));
    //    return;
    //  }
    //  else {
    //    Debug.Log("current sample=" + CurrentSamplingCount);

    //    int threadGroupsX = Mathf.CeilToInt(CurrentScreenResolutions.x / 8.0f);
    //    int threadGroupsY = Mathf.CeilToInt(CurrentScreenResolutions.y / 8.0f);

    //    //Different mKernelToUse is used depending on the task, that is, on the value
    //    // of _CaptureOrProjectOrView

    //    CurrentRayTracerShader.Dispatch(mKernelToUse, threadGroupsX, threadGroupsY, 1);

    //    // This dispatch of the compute shader will set _Target TWTexure2D


    //    // Blit the result texture to the screen
    //    if (AddMaterial_WholeSizeScreenSampling == null) {
    //      AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
    //    }

    //    AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCount);

    //    // TODO: Upscale To 4K and downscale to 1k.
    //    //_Target is the RWTexture2D created by the computeshader
    //    // note that  _cameraMain.targetTexture = _convergedForProjectImage;

    //    //debug

    //    Graphics.Blit(TargetRenderTex, ConvergedRenderTexForProjecting, AddMaterial_WholeSizeScreenSampling);

    //    //debug


    //    // Null the target Texture of the camera and blit to the null target (which is
    //    // the framebuffer

    //    // _cameraMain.targetTexture = null;

    //    //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
    //    //Debug.Log("_Target: IsCreated?=");
    //    //Debug.Log(_Target.IsCreated());

    //    //debug
    //    //Graphics.Blit(_Target, null as RenderTexture);


    //    //debug


    //    //if (_currentSample == 0)
    //    //{
    //    //    DebugTexture(_PredistortedImage);
    //    //}


    //    // debug
    //    Graphics.Blit(ConvergedRenderTexForProjecting, null as RenderTexture);

    //    CurrentSamplingCount++;
    //    // Each cycle of rendering, a new location within every pixel area is sampled 
    //    // for the purpose of  anti-aliasing.


    //  }  // else of if (mPauseNewRendering)


    //}
    //else if (SimulatorMode == 2) {
    //  if (bStopRender)  // PauseNewRendering is true when a task is completed and another task is not selected
    //                    // In this situation, the framebuffer is not updated, but the same content is transferred to the framebuffer
    //                    // to make the screen alive

    //  {
    //    Debug.Log("current sample not incremented =" + CurrentSamplingCount);
    //    Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

    //    // Null the target Texture of the camera and blit to the null target (which is
    //    // the framebuffer

    //    //_cameraMain.targetTexture = null;  //// tells Blit that  the current active target render texture is null,
    //    // which refers to the framebuffer
    //    //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
    //    Graphics.Blit(ConvergedRenderTexForPresenting, null as RenderTexture);
    //    return;

    //  }
    //  else {
    //    Debug.Log("current sample=" + CurrentSamplingCount);


    //    int threadGroupsX = Mathf.CeilToInt(CurrentScreenResolutions.x / 8.0f);
    //    int threadGroupsY = Mathf.CeilToInt(CurrentScreenResolutions.y / 8.0f);

    //    //Different mKernelToUse is used depending on the task, that is, on the value
    //    // of _CaptureOrProjectOrView

    //    CurrentRayTracerShader.Dispatch(mKernelToUse, threadGroupsX, threadGroupsY, 1);
    //    // This dispatch of the compute shader will set _Target TWTexure2D


    //    // Blit the result texture to the screen
    //    if (AddMaterial_WholeSizeScreenSampling == null) {
    //      AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
    //    }

    //    AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCount);
    //    // TODO: Upscale To 4K and downscale to 1k.
    //    //_Target is the RWTexture2D created by the computeshader
    //    // note that  _cameraMain.targetTexture = _converged;

    //    Graphics.Blit(TargetRenderTex, ConvergedRenderTexForPresenting, AddMaterial_WholeSizeScreenSampling);

    //    // Null the target Texture of the camera and blit to the null target (which is
    //    // the framebuffer

    //    //_cameraMain.targetTexture = null;
    //    //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
    //    Graphics.Blit(ConvergedRenderTexForPresenting, null as RenderTexture);

    //    //if (_currentSample == 0)
    //    //{
    //    //    DebugLogOfRWBuffers();
    //    //}

    //    CurrentSamplingCount++;
    //    // Each cycle of rendering, a new location within every pixel area is sampled 
    //    // for the purpose of  anti-aliasing.


    //  }  // else of if (mPauseNewRendering)
    //}   // if (_CaptureOrProjectOrView == 2)
    #endregion
  } // OnRenderImage()

  #region DBG  
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

  //void Dbg_PrintRWBufs1() {
  //  // for debugging: print the buffer

  //  //_vertexBufferRW.GetData(mVertexArray);

  //  ////Debug.Log("Triangles");
  //  //////int meshObjectIndex = 0;
  //  //foreach (var meshObj in _panoramaScreens)
  //  //{


  //  //    int indices_count = meshObj.indices_count;
  //  //    int indices_offset = meshObj.indices_offset;

  //  //    int triangleIndex = 0;

  //  //    for (int i = indices_offset; i < indices_offset + indices_count; i += 3)
  //  //    {
  //  //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i]].ToString("F6"));
  //  //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i + 1]].ToString("F6"));
  //  //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i + 2]].ToString("F6"));

  //  //        //Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i]].ToString("F6"));
  //  //        //Debug.Log((triangleIndex) + "uv :" + _texcoords[_indices[i + 1]].ToString("F6"));
  //  //        //Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i + 2]].ToString("F6"));


  //  //        ++triangleIndex;
  //  //    }  // for each triangle


  //  //} // for each meshObj

  //  //Debug.Log("Panorama Cylinder");

  //  //foreach (var meshObj in _panoramaScreens)
  //  //{


  //  //    int indices_count = meshObj.indices_count;
  //  //    int indices_offset = meshObj.indices_offset;

  //  //    int triangleIndex = 0;

  //  //    for (int i = indices_offset; i < indices_offset + indices_count; i += 3)
  //  //    {
  //  //        Debug.Log((triangleIndex) + "th triangle:" + _vertices[_indices[i]].ToString("F6"));
  //  //        Debug.Log((triangleIndex) + "th triangle:" + _vertices[_indices[i + 1]].ToString("F6"));
  //  //        Debug.Log((triangleIndex) + "th triangle:" + _vertices[_indices[i + 2]].ToString("F6"));

  //  //        Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i]].ToString("F6"));
  //  //        Debug.Log((triangleIndex) + "uv :" + _texcoords[_indices[i + 1]].ToString("F6"));
  //  //        Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i + 2]].ToString("F6"));


  //  //        ++triangleIndex;
  //  //    }  // for each triangle


  //  //} // for each meshObj



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




  //  // debugging for the ray 0
  //  // mRayDirectionBuffer.GetData(mRayDirectionArray);
  //  // mIntersectionBuffer.GetData(mIntersectionArray);
  //  Dbg_AccumRayEnergyRWBuf.GetData(Dbg_AccumulatedRayEnergyArr);
  //  Dbg_EmissionRWBuf.GetData(Dbg_EmissionArr);
  //  //mSpecularBuffer.GetData(mSpecularArray);           

  //  for (int y = 0; y < CurrentScreenResolutions.y; y += 5) {
  //    for (int x = 0; x < CurrentScreenResolutions.x; x += 5) {
  //      int idx = y * CurrentScreenResolutions.x + x;


  //      //var myRayDir = mRayDirectionArray[idx];
  //      // var intersection = mIntersectionArray[idx];
  //      var accumRayEnergy = Dbg_AccumulatedRayEnergyArr[idx];
  //      // var emission = mEmissionArray[idx];
  //      //var specular = mSpecularArray[idx];


  //      //for debugging


  //      // Debug.Log("(" + x + "," + y + "):" + "incoming ray direction=" + myRayDir.ToString("F6"));
  //      // Debug.Log("(" + x + "," + y + "):" + "hit point=" + intersection.ToString("F6"));


  //      Debug.Log("PassedPredistortedImage(" + x + "," + y + ")=" + accumRayEnergy.ToString("F6"));
  //      //Debug.Log("(" + x + "," + y + "):" + "unTex.xy +id.xy=" + emission.ToString("F6"));
  //      //Debug.Log("(" + x + "," + y + "):" + "reflected direction=" + specular.ToString("F6"));
  //      // Debug.Log("Predistorted[" + x + "," + y + "]=" + _resultTexture.GetPixel(x, y));
  //      Debug.Log("Target[" + x + "," + y + "]=" + ResultTex2.GetPixel(x, y));
  //      Debug.Log("DebugRWTexture(index) [" + x + "," + y + "]=" + ResTex3.GetPixel(x, y));

  //    } // for x
  //  }  // for y

  //  // RenderTexture.active = null;  

  //} //    void DebugLogOfRWBuffers()
  #endregion

  void ClearRenderTexture(RenderTexture target) {
    var savedTarget = RenderTexture.active;
    // save the active renderTexture  (currently null,  that is, the framebuffer

    RenderTexture.active = target;
    //GL.Clear(clearDepth, clearColor, backgroundColor, depth=1.0f);    // 1.0 means the far plane

    GL.Clear(true, true, Color.clear);
    //// Clears the screen or the active RenderTexture  (which is target) you are drawing into.
    //// The cleared area will be limited by the active viewport.
    //// In most cases, a Camera will already be configured to  clear the screen or RenderTexture.


    RenderTexture.active = savedTarget; // restore the active renderTexture

  }

  /// <summary>
  /// This must be bound on the inspector (UI.Button.OnClick Event).
  /// </summary>
  public void OnInitCreateDistortedImage_Btn() {
    OnInitCreateDistortedImage(TargetPanoramaTexFromImage);
  }

  /// <summary>
  /// This must be directly called on the script.
  /// </summary>
  /// <param name="panoramaTex"></param>
  public void OnInitCreateDistortedImage(Texture2D panoramaTex) {
    // DanbiSimulatorMode (PREPARE -> CAPTURE).
    SimulatorMode = EDanbiSimulatorMode.CAPTURE;
    CurrentSamplingCountForRendering = 0;

    bPredistortedImageReady = false;
    // it means that the ray tracing process for obtaining
    // predistorted image is in progress
    // 
    // Make sure we have a current render target
    InitRenderTextureForCreateImage();
    // create _Target, _converge, _ProjectedImage renderTexture   (only once)

    // Set the parameters for the mirror object; 

    if (TriangularConeMirrorBuf != null) {
      if (PanoramaScreenBuf != null) {
        if (!bUseProjectionFromCameraCalibration) {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.TriconeMirror_Img);
        } else {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.TriconeMirror_Img_With_Lens_Distortion);

        }
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_TriangularConeMirrors", TriangularConeMirrorBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
      } else {
        Utils.StopPlayManually();
      }

    } else if (GeoConeMirrorBuf != null) {
      if (PanoramaScreenBuf != null) {
        if (!bUseProjectionFromCameraCalibration) {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.GeoconeMirror_Img);
        } else {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(EDanbiKernelKey.GeoconeMirror_Img_With_Lens_Distortion);
        }
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_GeoConedMirrors", GeoConeMirrorBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
      } else {
        Utils.StopPlayManually();
      }
    } else if (ParaboloidMirrorBuf != null) {
      if (PanoramaScreenBuf != null) {
        if (!bUseProjectionFromCameraCalibration) {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.ParaboloidMirror_Img);
        } else {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.ParaboloidMirror_Img_With_Lens_Distortion);
        }
        //Debug.Log("  kernelCreateImageParaboloidMirror is executed");

        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_ParaboloidMirrors", ParaboloidMirrorBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
      } else {
        //Debug.LogError("A panorama mesh should be defined");
        Utils.StopPlayManually();
      }

    } else if (HemisphereMirrorBuf != null) {
      if (PanoramaScreenBuf != null) {
        if (!bUseProjectionFromCameraCalibration) {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.HemisphereMirror_Img);
        } else {
          Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.HemisphereMirror_Img_With_Lens_Distortion);
        }
        //Debug.Log("  kernelCreateImageHemisphereMirror is executed");

        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_HemisphereMirrors", HemisphereMirrorBuf);
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
      } else {
        //Debug.LogError("A panorama mesh should be defined");
        Utils.StopPlayManually();
      }

    } else {
      Debug.LogError("A mirror should be defined in the scene");
      Utils.StopPlayManually();
    }

    //Vector3 l = DirectionalLight.transform.forward;
    //RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

    // !COMMENTED OUT! -> FOV is already embedded into the projectio matrix of the current main camera.
    //RayTracingShader.SetFloat("_FOV", Mathf.Deg2Rad * _cameraMain.fieldOfView);

    //Debug.Log("_FOV" + Mathf.Deg2Rad * MainCamera.fieldOfView);
    //Debug.Log("aspectRatio" + MainCamera.aspect + ":" + CurrentScreenResolutions.x / (float)CurrentScreenResolutions.y);

    RTShader.SetInt("_MaxBounce", MaxNumOfBounce);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_Vertices", VerticesBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_Indices", IndicesBuf);
    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_UVs", TexcoordsBuf);
    //RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_VertexBufferRW", Dbg_VerticesRWBuf);

    // The dispatched kernel mKernelToUse will do different things according to the value of _CaptureOrProjectOrView,    
    Debug.Log("_CaptureOrProjectOrView = " + SimulatorMode);

    RTShader.SetInt("_CaptureOrProjectOrView", (int)SimulatorMode);

    if (MainCamera != null) {
      if (!bUseProjectionFromCameraCalibration) {
        // if we don't use the camera calibration.
        RTShader.SetMatrix("_Projection", MainCamera.projectionMatrix);
        RTShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);
      } else {

        // .. Construct the projection matrix from the calibration parameters
        //    and the field-of-view of the current main camera.        
        float left = 0.0f;
        float right = (float)CurrentScreenResolutions.x; // MOON: change it to Projector Width
        float bottom = 0.0f;
        // MOON: change it to Projector Height
        float top = (float)CurrentScreenResolutions.y;
        // y axis goes downward.
        float near = MainCamera.nearClipPlane;
        float far = MainCamera.farClipPlane;

        // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/
        Matrix4x4 openGLNDCMatrix = GetOrthoMatOpenGLGPU(left, right, bottom, top, -near, -far);
        // OpenCV 함수를 이용하여 구한 카메라 켈리브레이션 K Matrix.
        Matrix4x4 openCVKMatrix = GetOpenCV_KMatrix(ProjectedCamParams.FocalLength.x, ProjectedCamParams.FocalLength.y,
                                                      ProjectedCamParams.PrincipalPoint.x, ProjectedCamParams.PrincipalPoint.y,
                                                      -near, -far);

        Matrix4x4 OpenCVToUnity = GetOpenCVToUnity();
        Matrix4x4 OpenGLToOpenCV = GetOpenGLToOpenCV((float)CurrentScreenResolutions.y);

        Matrix4x4 projectionMatrix = openGLNDCMatrix * OpenGLToOpenCV * openCVKMatrix * OpenCVToUnity;
        RTShader.SetMatrix("_Projection", projectionMatrix);
        RTShader.SetMatrix("_CameraInverseProjection", projectionMatrix.inverse);
        RTShader.SetInt("_UndistortMode", (int)UndistortMode);
        RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_CameraLensDistortionParams", CameraParamsForUndistortImageBuf);
        RTShader.SetVector("_ThresholdIterative", new Vector2(ThresholdIterative, ThresholdIterative));
        RTShader.SetInt("_SafeCounter", SafeCounter);
        RTShader.SetVector("_ThresholdNewton", new Vector2(ThresholdNewton, ThresholdNewton));
      }

      RTShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);

    } else {
      Debug.LogError("MainCamera should be activated");
      Utils.StopPlayManually();
    }

    // CameraUser should be active all the time
    //if (_cameraUser != null)
    //{
    //    Debug.Log("CameraUser will be deactivated");
    //    _cameraUser.enabled = false;
    //    //StopPlay();
    //}
    //// used the result of the rendering (raytracing shader)
    ////Hint the GPU driver that the contents of the RenderTexture will not be used.
    //// _Target.DiscardContents();
    // Clear the target render Texture _Target

    ClearRenderTexture(ResultRenderTex);
    RTShader.SetTexture(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_Result", ResultRenderTex);  // used always      

    // set the textures TargetPanoramaTexFromImage
    //CurrentRayTracerShader.SetTexture(mKernelToUse, "_SkyboxTexture", SkyboxTex);

    if (panoramaTex == null) {
      Debug.Log($"<color=red>panoramaTex cannot be null!</color>", this);
    } else {
      RTShader.SetTexture(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_RoomTexture", panoramaTex);
    }

    //bPredistortedImageReady = false;
    #region debugging
    //SetDbgBufsToShader();
    #endregion
  }   // OnCreatePreDistortedImage()


  //public void OnInitCreateDistortedImage2(RenderTexture panoramaTex) {
  //  SimulatorMode = EDanbiSimulatorMode.CAPTURE;
  //  bPredistortedImageReady = false;
  //  CurrentSamplingCountForRendering = 0;

  //  // it means that the raytracing process for obtaining
  //  // predistorted image is in progress
  //  // 
  //  // Make sure we have a current render target
  //  InitRenderTextureForCreateImage();
  //  // create _Target, _converge, _ProjectedImage renderTexture   (only once)

  //  // Set the parameters for the mirror object; 

  //  if (TriangularConeMirrorBuf != null) {
  //    if (PanoramaScreenBuf != null) {
  //      if (!bUseProjectionFromCameraCalibration) {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.TriconeMirror_Img);
  //      } else {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.TriconeMirror_Img_With_Lens_Distortion);

  //      }
  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_TriangularConeMirrors", TriangularConeMirrorBuf);
  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
  //    } else {
  //      Utils.StopPlayManually();
  //    }

  //  } else if (GeoConeMirrorBuf != null) {
  //    if (PanoramaScreenBuf != null) {
  //      if (!bUseProjectionFromCameraCalibration) {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.GeoconeMirror_Img);
  //      } else {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(EDanbiKernelKey.GeoconeMirror_Img_With_Lens_Distortion);
  //      }
  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_GeoConedMirrors", GeoConeMirrorBuf);
  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
  //    } else {
  //      Utils.StopPlayManually();
  //    }
  //  } else if (ParaboloidMirrorBuf != null) {
  //    if (PanoramaScreenBuf != null) {
  //      if (!bUseProjectionFromCameraCalibration) {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.ParaboloidMirror_Img);
  //      } else {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.ParaboloidMirror_Img_With_Lens_Distortion);
  //      }
  //      //Debug.Log("  kernelCreateImageParaboloidMirror is executed");

  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_ParaboloidMirrors", ParaboloidMirrorBuf);
  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
  //    } else {
  //      //Debug.LogError("A panorama mesh should be defined");
  //      Utils.StopPlayManually();
  //    }

  //  } else if (HemisphereMirrorBuf != null) {
  //    if (PanoramaScreenBuf != null) {
  //      if (!bUseProjectionFromCameraCalibration) {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.HemisphereMirror_Img);
  //      } else {
  //        Danbi.DanbiKernelHelper.CurrentKernelIndex = Danbi.DanbiKernelHelper.GetKernalIndex(Danbi.EDanbiKernelKey.HemisphereMirror_Img_With_Lens_Distortion);
  //      }
  //      //Debug.Log("  kernelCreateImageHemisphereMirror is executed");

  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_HemisphereMirrors", HemisphereMirrorBuf);
  //      RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_PanoramaMeshes", PanoramaScreenBuf);
  //    } else {
  //      //Debug.LogError("A panorama mesh should be defined");
  //      Utils.StopPlayManually();
  //    }

  //  } else {
  //    Debug.LogError("A mirror should be defined in the scene");
  //    Utils.StopPlayManually();
  //  }

  //  if (bUseProjectionFromCameraCalibration) {
  //    RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_CameraLensDistortionParams", CameraParamsForUndistortImageBuf);
  //  }

  //  //Vector3 l = DirectionalLight.transform.forward;
  //  //RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

  //  //RayTracingShader.SetFloat("_FOV", Mathf.Deg2Rad * _cameraMain.fieldOfView);


  //  Debug.Log("_FOV" + Mathf.Deg2Rad * MainCamera.fieldOfView);
  //  Debug.Log("aspectRatio" + MainCamera.aspect + ":" + CurrentScreenResolutions.x / (float)CurrentScreenResolutions.y);

  //  RTShader.SetInt("_MaxBounce", MaxNumOfBounce);
  //  RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_Vertices", VerticesBuf);
  //  RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_Indices", IndicesBuf);
  //  RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_UVs", TexcoordsBuf);
  //  //RTShader.SetBuffer(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_VertexBufferRW", Dbg_VerticesRWBuf);

  //  // The dispatched kernel mKernelToUse will do different things according to the value
  //  // of _CaptureOrProjectOrView,
  //  //Debug.Log("888888888888888888888888888888888888888888");
  //  //Debug.Log("888888888888888888888888888888888888888888");
  //  Debug.Log("_CaptureOrProjectOrView = " + SimulatorMode);
  //  //Debug.Log("888888888888888888888888888888888888888888");
  //  //Debug.Log("888888888888888888888888888888888888888888");

  //  RTShader.SetInt("_CaptureOrProjectOrView", (int)SimulatorMode);

  //  if (MainCamera != null) {
  //    if (bUseProjectionFromCameraCalibration) {
  //      float left = 0.0f;
  //      float right = (float)CurrentScreenResolutions.x;
  //      float bottom = 0.0f;
  //      float top = (float)CurrentScreenResolutions.y;
  //      float near = MainCamera.nearClipPlane;
  //      float far = MainCamera.farClipPlane;

  //      float focalLengthX = MainCamera.focalLength;
  //      float focalLengthY = MainCamera.focalLength;

  //      Matrix4x4 openGLNDCMatrix = GetOpenGL_KMatrix(left, right, bottom, top, near, far);
  //      Matrix4x4 openCVNDCMatrix = GetOpenCV_KMatrix(focalLengthX, focalLengthY,
  //                                                    CamParams.PrincipalPoint.x, CamParams.PrincipalPoint.y,
  //                                                    /*top, */near, far);

  //      Matrix4x4 projectionMatrix = openGLNDCMatrix * openCVNDCMatrix;
  //      RTShader.SetMatrix("_Projection", projectionMatrix);
  //      RTShader.SetMatrix("_CameraInverseProjection", projectionMatrix.inverse);
  //    } else {
  //      RTShader.SetMatrix("_Projection", MainCamera.projectionMatrix);
  //      RTShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);
  //    }
  //    RTShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);
  //  } else {
  //    Debug.LogError("MainCamera should be activated");
  //    Utils.StopPlayManually();
  //  }

  //  // CameraUser should be active all the time
  //  //if (_cameraUser != null)
  //  //{
  //  //    Debug.Log("CameraUser will be deactivated");
  //  //    _cameraUser.enabled = false;
  //  //    //StopPlay();
  //  //}
  //  //// used the result of the rendering (raytracing shader)
  //  ////Hint the GPU driver that the contents of the RenderTexture will not be used.
  //  //// _Target.DiscardContents();
  //  // Clear the target render Texture _Target

  //  ClearRenderTexture(ResultRenderTex);
  //  RTShader.SetTexture(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_Result", ResultRenderTex);  // used always      

  //  // set the textures TargetPanoramaTexFromImage
  //  //CurrentRayTracerShader.SetTexture(mKernelToUse, "_SkyboxTexture", SkyboxTex);
  //  RTShader.SetTexture(Danbi.DanbiKernelHelper.CurrentKernelIndex, "_RoomTexture_RT", panoramaTex);

  //  #region debugging
  //  //SetDbgBufsToShader();
  //  #endregion
  //}

  #region Bind target functions
  public void OnCreateDistortedImageForProjection() {
    #region 
    //CurrentSamplingCount = 0;
    //SimulatorMode = EDanbiSimulatorMode.PROJECTION;
    //bStopRender = false;  // ready to render

    //// it means that the raytracing process for projecting the predistorted
    //// image onto the scene is in progress.
    ////  _CaptureOrProjectOrView = 2 means that the raytracing process of viewing
    //// the projected image is in progress.

    //// RebuildObjectBuffers();

    //// Make sure we have a current render target
    //InitRenderTextureForProjectImage();
    //// create _Target, _converge,  renderTexture   (only once)


    //// Set the parameters for the mirror object; 

    //// determine the kernel to use:
    //if (TriangularConeMirrorBuf != null) {
    //  if (PanoramaBuf != null) {
    //    mKernelToUse = kernelProjectImageTriConeMirror;
    //    Debug.Log("  kernelProjectImageTriConeMirror is executed");


    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_TriangularConeMirrors",
    //                              TriangularConeMirrorBuf);
    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", PanoramaBuf);



    //  }
    //  else {
    //    Debug.LogError("A panorama  mesh should be defined");
    //    Utils.StopPlayManually();
    //  }

    //}
    //else if (GeoConeMirrorBuf != null) {
    //  if (PanoramaBuf != null) {
    //    mKernelToUse = kernelProjectImageGeoConeMirror;
    //    Debug.Log(" kernelProjectImageGeoConeMirror is executed");
    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_GeoConedMirrors", GeoConeMirrorBuf);
    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", PanoramaBuf);
    //  }
    //  else {
    //    Debug.LogError("A panorama  mesh should be defined");
    //    Utils.StopPlayManually();
    //  }


    //}
    //else if (ParaboloidMirrorBuf != null) {
    //  if (PanoramaBuf != null) {
    //    mKernelToUse = kernelProjectImageParaboloidMirror;
    //    Debug.Log("  kernelProjectImageParaboloidMirror is executed");

    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_ParaboloidMirrors", ParaboloidMirrorBuf);
    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", PanoramaBuf);
    //  }
    //  else {
    //    Debug.LogError("A panorama  mesh should be defined");
    //    Utils.StopPlayManually();
    //  }

    //}
    //else if (HemisphereMirrorBuf != null) {
    //  if (PanoramaBuf != null) {
    //    mKernelToUse = kernelProjectImageHemisphereMirror;
    //    Debug.Log("  kernelProjectImageHemisphereMirror is executed");

    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_HemisphereMirrors", HemisphereMirrorBuf);
    //    CurrentRayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", PanoramaBuf);
    //  }
    //  else {
    //    Debug.LogError("A panorama  mesh should be defined");
    //    Utils.StopPlayManually();
    //  }

    //}
    //else {
    //  Debug.LogError("A mirror should be defined in the scene");
    //  Utils.StopPlayManually();
    //}


    ////var l = DirectionalLight.transform.forward;
    ////CurrentRayTracerShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

    //// Added by Moon Jung, 2020/1/21
    ////CurrentRayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamera.fieldOfView);

    //// Added by Moon Jung, 2020/1/29
    //CurrentRayTracerShader.SetInt("_MaxBounce", MaxNumOfBounce);
    //// RayTracingShader.SetInt("_MirrorType", _mirrorType);  // _mirrorType should be set
    //// in the inspector of this script component which is attached to the camera gameObject

    ////SetComputeBuffer("_Spheres", _sphereBuffer); //  commented out by Moon Jung
    ////RayTracingShader.SetBuffer  

    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_Vertices", VerticesBuf);
    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_Indices", IndicesBuf);
    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_UVs", TexcoordsBuf);

    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_VertexBufferRW", Dbg_VerticesRWBuf);


    //// The disptached kernel mKernelToUse will do different things according to the value
    //// of _CaptureOrProjectOrView,
    //Debug.Log("888888888888888888888888888888888888888888");
    //Debug.Log("888888888888888888888888888888888888888888");
    //Debug.Log("_CaptureOrProjectOrView = " + SimulatorMode);
    //Debug.Log("888888888888888888888888888888888888888888");
    //Debug.Log("888888888888888888888888888888888888888888");

    //CurrentRayTracerShader.SetInt("_CaptureOrProjectOrView", (int)SimulatorMode);

    //if (MainCamera != null) {
    //  CurrentRayTracerShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);

    //  CurrentRayTracerShader.SetMatrix("_Projection", MainCamera.projectionMatrix);

    //  CurrentRayTracerShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);

    //}
    //else {
    //  Debug.LogError("MainCamera should be activated");
    //  Utils.StopPlayManually();
    //}

    //// CameraUser should be active all the time to show the menu
    ////if (_cameraUser != null)
    ////{
    ////    Debug.Log("CameraUser will be deactivated");
    ////    _cameraUser.enabled = false;
    ////    //StopPlay();
    ////}





    //// use the result of the rendering (raytracing shader)
    ////_PredistortedImage = _convergedForCreateImage;

    //if (DistortedResultImage == null) {
    //  Debug.LogError("_PredistortedImage [RenderTexture] should be created by Create predistorted image");

    //  Utils.StopPlayManually();
    //}
    //else {
    //  CurrentRayTracerShader.SetTexture(mKernelToUse, "_PredistortedImage", DistortedResultImage);
    //}




    ////_Target.DiscardContents();
    //// Clear the target render Texture _Target

    //ClearRenderTexture(TargetRenderTex);

    //CurrentRayTracerShader.SetTexture(mKernelToUse, "_Result", TargetRenderTex);


    //#region debugging

    //SetDbgBufsToShader();


    //#endregion

    #endregion
  } //OnProjectPreDistortedImage()

  public void OnCreatePanoramaForView() {
    #region 
    //UserCamera = GameObject.FindWithTag("CameraUser").GetComponent<Camera>();
    //SimulatorMode = EDanbiSimulatorMode.VIEW;

    //bStopRender = false;  // ready to render

    //CurrentSamplingCount = 0;
    ////  _CaptureOrProjectOrView = 2 means that the raytracing process of viewing
    //// the projected image is in progress.


    //// RebuildObjectBuffersWithoutMirror();

    //// Make sure we have a current render target
    //InitRenderTextureForViewImage();
    //// create _Target, _converge, _ProjectedImage renderTexture   (only once)


    //// determine the kernel for viewing image on the scene          

    //if (PanoramaBuf != null) {
    //  mKernelToUse = kernelViewImageOnPanoramaScreen;
    //  Debug.Log("   kernelViewImageOnPanoramaScreen kernel is executed");

    //}
    //else {
    //  Debug.LogError("A panorama  mesh should be defined");
    //  Utils.StopPlayManually();
    //}

    //// The disptached kernel mKernelToUse will do different things according to the value
    //// of _CaptureOrProjectOrView,
    //Debug.Log("888888888888888888888888888888888888888888");
    //Debug.Log("888888888888888888888888888888888888888888");
    //Debug.Log("_CaptureOrProjectOrView = " + SimulatorMode);
    //Debug.Log("888888888888888888888888888888888888888888");
    //Debug.Log("888888888888888888888888888888888888888888");

    //CurrentRayTracerShader.SetInt("_CaptureOrProjectOrView", (int)SimulatorMode);

    //// CameraMain should be enabled all the time
    ////if (_cameraMain != null)
    ////{
    ////    _cameraMain.enabled = false;
    ////    Debug.Log("CameraMain will be deactivated");
    ////}


    //if (UserCamera != null) {

    //  //MyIO.DebugLogMatrix(_cameraMain.worldToCameraMatrix);
    //  // "forward" in OpenGL is "-z".In Unity forward is "+z".Most hand - rules you might know from math are inverted in Unity
    //  //    .For example the cross product usually uses the right hand rule c = a x b where a is thumb, b is index finger and c is the middle
    //  //    finger.In Unity you would use the same logic, but with the left hand.

    //  //    However this does not affect the projection matrix as Unity uses the OpenGL convention for the projection matrix.
    //  //    The required z - flipping is done by the cameras worldToCameraMatrix.
    //  //    So the projection matrix should look the same as in OpenGL.


    //  CurrentRayTracerShader.SetMatrix("_Projection", UserCamera.projectionMatrix);

    //  CurrentRayTracerShader.SetMatrix("_CameraInverseProjection", UserCamera.projectionMatrix.inverse);

    //  CurrentRayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamera.fieldOfView);


    //}
    //else {
    //  Debug.LogError("CameraUser should be defined");

    //  Utils.StopPlayManually();
    //}


    ////var l = DirectionalLight.transform.forward;
    ////CurrentRayTracerShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));


    //CurrentRayTracerShader.SetInt("_MaxBounce", MaxNumOfBounce);
    //// RayTracingShader.SetInt("_MirrorType", _mirrorType);  // _mirrorType should be set
    //// in the inspector of this script component which is attached to the camera gameObject

    ////SetComputeBuffer("_Spheres", _sphereBuffer);   commented out by Moon Jung

    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_Vertices", VerticesBuf);
    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_Indices", IndicesBuf);
    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_UVs", TexcoordsBuf);

    //CurrentRayTracerShader.SetBuffer(mKernelToUse, "_VertexBufferRW", Dbg_VerticesRWBuf);




    //// use the result of the rendering (raytracing shader)
    //// _ProjectedImage = _convergedForProjectImage;

    ////if (ProjectedResultImage == null) {
    ////  Debug.LogError("_ProjectedImage [RenderTexture] should be created by Project Predistorted image");

    ////  StopPlay();
    ////}
    ////else {
    ////  CurrentRayTracerShader.SetTexture(mKernelToUse, "_ProjectedImage", ProjectedResultImage);
    ////}

    ////_Target.DiscardContents();
    ////So what do DiscardContents do:
    ////1.it marks appropriate render buffers(there are bools in there) 
    ////              to be forcibly cleared next time you activate the RenderTexture
    //// 2.IF you call it on active RT it will discard when you will set another RT as active.

    //// Clear the target render Texture _Target

    //ClearRenderTexture(TargetRenderTex);

    //CurrentRayTracerShader.SetTexture(mKernelToUse, "_Result", TargetRenderTex);


    //#region debugging

    //SetDbgBufsToShader();


    //#endregion
    #endregion

  } //OnViewPanoramaScene()

  public void OnSaveImage() {
    // bStopRender becomes true in 2 cases.
    // this is the first case.
    bPredistortedImageReady = true;
    DanbiImage.CaptureScreenToFileName(currentSimulatorMode: SimulatorMode,
                                       convergedRT: ConvergedRenderTexForNewImage,
                                       //distortedResult: out DistortedResultImage,
                                       name: CurrentInputField.textComponent.text);
    #region 
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

  public void ApplyNewTargetTexture(bool bCalledOnValidate, Texture2D newTargetTex) {
    // Set the panorama material automatically by changing the texture.
    CurrentPanoramaList.AddRange(FindObjectsOfType<PanoramaScreenObject>());
    foreach (var panorama in CurrentPanoramaList) {
      panorama.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", newTargetTex);
    }

    //if (!bCalledOnValidate && DanbiController.bWindowOpened) {
    //  DanbiController.OnTargetTexChanged?.Invoke(TargetPanoramaTexFromImage);
    //}
  }
  #endregion

  #region Image Undistortion
  void UndistortImage() {

  }
  //
  // | |
  // | |
  // | |
  // | |
  // 
  static Matrix4x4 GetOpenCV_KMatrix(float alpha, float beta, float x0, float y0,/* float imgHeight,*/ float near, float far) {
    Matrix4x4 PerspK = new Matrix4x4();
    float A = -(near + far);
    float B = near * far;

    PerspK[0, 0] = alpha;
    PerspK[1, 1] = beta;
    PerspK[0, 2] = -x0;
    PerspK[1, 2] = -y0;/*-(imgHeight - y0);*/
    PerspK[2, 2] = A;
    PerspK[2, 3] = B;
    PerspK[3, 2] = -1.0f;

    return PerspK;
  }

  // Based On the Foundation of 3D Computer Graphics (book)
  static Matrix4x4 GetOpenCVToUnity() {
    var FrameTransform = new Matrix4x4();   // member fields are init to zero

    FrameTransform[0, 0] = 1.0f;
    FrameTransform[1, 1] = -1.0f;
    FrameTransform[2, 2] = 1.0f;
    FrameTransform[3, 3] = 1.0f;

    return FrameTransform;
  }

  // Based On the Foundation of 3D Computer Graphics (book)
  static Matrix4x4 GetOpenGLToOpenCV(float ScreenHeight) {
    var FrameTransform = new Matrix4x4();   // member fields are init to zero

    FrameTransform[0, 0] = 1.0f;
    FrameTransform[1, 1] = -1.0f;
    FrameTransform[1, 3] = ScreenHeight;
    FrameTransform[2, 2] = 1.0f;
    FrameTransform[3, 3] = 1.0f;

    return FrameTransform;
  }


  // Based On the Foundation of 3D Computer Graphics (book)
  static Matrix4x4 GetOrthoMatOpenGLGPU(float left, float right, float bottom, float top, float near, float far) {
    float m00 = 2.0f / (right - left);
    float m11 = 2.0f / (top - bottom);
    float m22 = -2.0f / (far - near);

    float tx = -(left + right) / (right - left);
    float ty = -(bottom + top) / (top - bottom);
    //float tz = -(near + far) / (far - near);
    float tz = (near + far) / (far - near);

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
  #endregion
};

