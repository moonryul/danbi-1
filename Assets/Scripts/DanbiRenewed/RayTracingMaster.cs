//https://bitbucket.org/Daerst/gpu-ray-tracing-in-unity/src/master/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;



public enum eScreenWidthHeight {
  E_1K, E_2K, E_4K
};

public class RayTracingMaster : MonoBehaviour {
  // public InputField mInputField;

  //public int FindKernel(string name);
  int mKernelToUse = -1; // -1 means that mKernel is not defined yet

  int kernelCreateImageTriConeMirror;
  int kernelCreateImageGeoConeMirror;
  int kernelCreateImageParaboloidMirror;
  int kernelCreateImageHemisphereMirror;


  int kernelProjectImageTriConeMirror;
  int kernelProjectImageGeoConeMirror;
  int kernelProjectImageParaboloidMirror;
  int kernelProjectImageHemisphereMirror;

  int kernelViewImageOnPanoramaScreen;

  // TODO: Enumerization
  [SerializeField, Header("It affects the result images")]
  int TargetScreenWidth = 3840;
  [SerializeField]
  int TargetScreenHeight = 2160;

  int SizeMultiplier = 1;
  // 3840 x 2160

  [SerializeField, Header("Panorama object of current simulation set"), Space(10)]
  GameObject CurrentPanorama;

  [SerializeField, Header("It affects for to the Scene and at Runtime")]
  Texture PanoramaTex;

  [SerializeField, Header("Ray-Tracer Compute Shader"), Space(10)]
  ComputeShader RayTracerShader;

  [SerializeField, Header("2 for good performance")]
  int MaxNumOfBounce = 8;

  [SerializeField, Space(10)]
  Texture SkyboxTex;

  [SerializeField]
  Light DirectionalLight;

  //[Header("Spheres")]
  //public int SphereSeed;
  //public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
  //public uint SpheresMax = 100;
  //public float SpherePlacementRadius = 100.0f;

  Camera MainCamera;     // used to raytrace to obtain  pre-distorted image
                         // and to project the pre-distorted image onto the scene

  Camera UserCamera;  // used to render the scene onto which the predistorted image
                      // is projected.
  GameObject CurrentCanvas;

  public int CaptureOrProjectOrView = -1; // o == capture 1 == project 2= view
                                          //   CaptureOrProjectOrView = -1 means that the task is not selected

  // int mRenderingIteration = 0;
  bool bStopRender = false; // PauseNewRendering is true  when the user is saving image;
                              // In that case, the current renderTexture is transferred to the framebuffer rather than
                              // rendering a new content. The framebuffer should be updated continuously to make it alive??

  // processing Button commands

  RenderTexture TargetRenderTex;   // This is mapped to Result in ComputeShader and used
                                   // store the result of creating the distorted image

  RenderTexture ConvergedRenderTexForNewImage, ConvergedRenderTexForProjecting, ConvergedRenderTexForPresenting;

  [SerializeField] 
  Texture2D DistortedResultImage;

  [SerializeField] 
  Texture2D ProjectedResultImage;

  RenderTexture Dbg_RWTex;
  // this refers to the result of projecting the distorted image

  Texture2D ResultTex1, ResultTex2, ResTex3;


  // this is supposed to be set in the inspector; 
  // it would refer to the screen captured image
  // of the process of creating the distorted image


  Material AddMaterial_WholeSizeScreenSampling;
  uint CurrentSamplingCount = 0;
  ComputeBuffer SphereRTObjectBuf;
  List<Transform> TransformListToWatch = new List<Transform>();


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
  static List<PyramidMirror> _pyramidMirrors = new List<PyramidMirror>();
  static List<PyramidMirrorObject> _pyramidMirrorObjects = new List<PyramidMirrorObject>();

  static List<ParaboloidMirror> _paraboloidMirrors = new List<ParaboloidMirror>();
  static List<ParaboloidMirrorObject> _paraboloidMirrorObjects = new List<ParaboloidMirrorObject>();

  static List<GeoConeMirror> _geoConeMirrors = new List<GeoConeMirror>();
  static List<GeoConeMirrorObject> _geoConeMirrorObjects = new List<GeoConeMirrorObject>();

  static List<HemisphereMirrorObject> _hemisphereMirrorObjects = new List<HemisphereMirrorObject>();
  static List<HemisphereMirror> _hemisphereMirrors = new List<HemisphereMirror>();

  static List<PanoramaMesh> _panoramaMeshes = new List<PanoramaMesh>();
  static List<PanoramaMeshObject> _panoramaMeshObjects = new List<PanoramaMeshObject>();



  static List<TriangularConeMirrorObject> _triangularConeMirrorObjects = new List<TriangularConeMirrorObject>();
  static List<TriangularConeMirror> _triangularConeMirrors = new List<TriangularConeMirror>();

  public GameObject SaveFileInputField;


  ComputeBuffer _meshObjectBuffer;

  ComputeBuffer _vertexBuffer;
  ComputeBuffer _indexBuffer;
  ComputeBuffer _texcoordsBuffer;

  ComputeBuffer _vertexBufferRW;

  ComputeBuffer _pyramidMirrorBuffer;
  ComputeBuffer _paraboloidMirrorBuffer;
  ComputeBuffer _geoConeMirrorBuffer;

  ComputeBuffer _panoramaScreenBuffer;
  ComputeBuffer _panoramaMeshBuffer;


  ComputeBuffer _triangularConeMirrorBuffer;
  ComputeBuffer _hemisphereMirrorBuffer;

  // for debugging

  public ComputeBuffer mRayDirectionBuffer;
  public ComputeBuffer mIntersectionBuffer;
  public ComputeBuffer mAccumRayEnergyBuffer;
  public ComputeBuffer mEmissionBuffer;
  public ComputeBuffer mSpecularBuffer;
  //
  // ComputeBuffer(int count, int stride, ComputeBufferType type);
  // 
  Vector3[] mVertexArray;
  Vector4[] mRayDirectionArray;
  Vector4[] mIntersectionArray, mAccumRayEnergyArray, mEmissionArray, mSpecularArray;
  //

  //-PYRAMID MIRROR------------------------------------
  public struct PyramidMirror {
    public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public float height;
    public float width;  // the radius of the base of the cone
    public float depth;

  };

  public struct TriangularConeMirror {
    public Matrix4x4 localToWorldMatrix;

    public float distanceToOrigin;
    public float height;
    public float notUseRatio;
    public float radius;

    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
  }

  public struct GeoConeMirror {
    public Matrix4x4 localToWorldMatrix; // the world frame of the cone
    public float distanceToOrigin;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public float height;
    public float notUseRatio;
    public float radius;  // the radius of the base of the cone
  };

  public struct HemisphereMirror {
    public Matrix4x4 localToWorldMatrix;

    public float distanceToOrigin;
    public float height;
    public float notUseRatio;
    public float radius;

    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    // public int indices_offset;
    //public int indices_count;
  };

  struct ParaboloidMirror {
    public Matrix4x4 localToWorldMatrix; // the world frame of the cone
    public float distanceToOrigin; // distance from the camera to the origin of the paraboloid
    public float height;
    public float notUseRatio; //
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public float coefficientA;  // z = - ( x^2/a^2 + y^2/b^2)
    public float coefficientB;
  };

  struct PanoramaMesh {
    public Matrix4x4 localToWorldMatrix;
    public float highRange;
    public float lowRange;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
  };

  struct MeshObject {
    public Matrix4x4 localToWorldMatrix;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
  };

  struct MeshObjectRW {
    public Matrix4x4 localToWorldMatrix;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  };

  struct Sphere {
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  };

  Text m_textComponent;
  GameObject mInputFieldObj;
  InputField mInputField;
  GameObject mPlaceHolder;
  bool bInitiated = false;

  public void OnSaveImageButtonClicked() {
    Debug.Log("End edit on enter");
    CaptureScreenToFileName(mInputField.textComponent.text);
  }

  void Awake() {
    DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
    DirectionalLight = GameObject.Find("Sun").GetComponent<Light>();
    var root = transform.parent;
    // this.gameObject is the CameraMain gameObject to which the current script "this"
    // is attached. 

    // The 3rd child of the root gameObject is Canvas whose 4th child is InputField
    // You can get references to gameObjects/their components even though they
    // are activated/enabled.

    mInputFieldObj = SaveFileInputField.gameObject;
    mInputField = mInputFieldObj.GetComponent<InputField>();
    mPlaceHolder = mInputFieldObj.transform.GetChild(0).gameObject;

    mInputField.onEndEdit.AddListener(
      val => {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
          CaptureScreenToFileName(mInputField.textComponent.text);
        }
      }
    );

    //Deactivate the inputField Obj so that it will be popped up when relevant

    //mInputFieldObj.SetActive(false);

    //GameObject placeHolder = mInputFieldObj.transform.GetChild(0).gameObject;
    mPlaceHolder.SetActive(false);
    // disable Canvas gameObject so that it will show relevant menu items when 
    // the  scene objects are registered.  It will be activated in Start() method
    // which is called after all Awake() methods of the object registering scripts are 
    // executed; The objects are registered by the Awake() methods of these scripts

    //mCanvasObj = GameObject.FindWithTag("Canvas");
    //Debug.Log("Canvas =" + mCanvasObj);

    ////mCanvasObj.SetActive(false);  // deactivate the canvas gameobject initially
    //Canvas canvas = mCanvasObj.GetComponent<Canvas>();
    //canvas.enabled = false;

    kernelCreateImageTriConeMirror = RayTracerShader.FindKernel("CreateImageTriConeMirror");
    kernelCreateImageGeoConeMirror = RayTracerShader.FindKernel("CreateImageGeoConeMirror");
    kernelCreateImageParaboloidMirror = RayTracerShader.FindKernel("CreateImageParaboloidMirror");
    kernelCreateImageHemisphereMirror = RayTracerShader.FindKernel("CreateImageHemisphereMirror");

    kernelProjectImageTriConeMirror = RayTracerShader.FindKernel("ProjectImageTriConeMirror");
    kernelProjectImageGeoConeMirror = RayTracerShader.FindKernel("ProjectImageGeoConeMirror");
    kernelProjectImageParaboloidMirror = RayTracerShader.FindKernel("ProjectImageParaboloidMirror");

    kernelProjectImageHemisphereMirror = RayTracerShader.FindKernel("ProjectImageHemisphereMirror");

    kernelViewImageOnPanoramaScreen = RayTracerShader.FindKernel("ViewImageOnPanoramaScreen");

    MainCamera = GetComponent<Camera>();     // gameObject is the camera  gameObject

    // static GameObject FindWithTag(string tag);
    //_cameraUser = GameObject.FindWithTag("CameraUser").GetComponent<Camera>();

    TransformListToWatch.Add(transform);   // mainCamera
    TransformListToWatch.Add(DirectionalLight.transform);
    //Validator = GetComponent<RTRayDirectionValidator>();    

    ResultTex1 = new Texture2D(TargetScreenWidth, TargetScreenHeight, TextureFormat.RGBAFloat, false);
    ResultTex2 = new Texture2D(TargetScreenWidth, TargetScreenHeight, TextureFormat.RGBAFloat, false);
    ResTex3 = new Texture2D(TargetScreenWidth, TargetScreenHeight, TextureFormat.RGBAFloat, false);
  }

  void OnValidate() {
    if (!enabled || !gameObject.activeInHierarchy || !gameObject.activeSelf) { return; }
    // Set the panorama material.
    if (!ReferenceEquals(CurrentPanorama, null)) {
      CurrentPanorama.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", PanoramaTex);
    }
  }

  //   https://answers.unity.com/questions/372752/does-finction-start-or-awake-run-when-the-object-o.html
  // The start/awake function of a script can be called mid-game by another script
  // Different situations:
  // In general there is no differences between the behavior of objects in a scene and those
  // created at runtime with Instantiate:

  // 1) The gameobject is active but the script is disabled => only Awake() is called
  // 2) The gameObject is deactivated but the script is enabled  => nothing is callled when the object is
  // created.
  // 3) The gameObject is deactivated and the script is disabled
  // 4) The gameObject is active and the script is enabled.   => Start() and OnEnable() called

  // SUM: Awake/Start is called only once;  OnEnable/OnDisable is called everytime it is enabled/disabled
  // OnEnable is called if (gameObject.active && enabled)
  void Start() {
    RebuildObjectBuffers();
    CurrentSamplingCount = 0;
  }

  void OnDisable() {
    SphereRTObjectBuf?.Release();
    _meshObjectBuffer?.Release();
    _vertexBuffer?.Release();
    _indexBuffer?.Release();

    mIntersectionBuffer?.Release();
    mAccumRayEnergyBuffer?.Release();
    mEmissionBuffer?.Release();
    mSpecularBuffer?.Release();
  }

  void QuitJob() {
#if UNITY_EDITOR
    // Application.Quit() does not work in the editor so
    //UnityEditor.EditorApplication.isPlaying = false; // delays until all the script code  has been
    // completed for this frame.

    UnityEditor.EditorApplication.Exit(0);
    // Calling this function will exit right away, without asking to save changes.
    // Useful  for exiting out of a commandline process with a specific error
#else
        Application.Quit();
#endif
  }

  void StopPlay() {
#if UNITY_EDITOR
    // Application.Quit() does not work in the editor so

    UnityEditor.EditorApplication.isPlaying = false;

    // delays until all the script code  has been   completed for this frame.

    //UnityEditor.EditorApplication.Exit(0);
    // Calling this function will exit right away, without asking to save changes.
    // Useful  for exiting out of a commandline process with a specific error
#else
        Application.Quit();
#endif
  }   //StopPlay()

  //string  GetFileName()
  //{
  //    // get the string from the keyboard Input
  //    foreach (char c in Input.inputString)
  //    {
  //        if (c == '\b')   // backspace/delete
  //        {
  //            if (mText.text.Length != 0)
  //            {
  //                mText.text = mText.text.Substring(0, mText.text.Length - 1);
  //            }
  //        }
  //        else if ( (c == '\n') || (c == '\r') ) // enter/return
  //        {
  //            print("User entered the fileName: " + mText.text);
  //           // break;
  //        }
  //        else
  //        {
  //            mText.text += c;
  //        }
  //    } //foreach c

  //    return mText.text;

  //} // GetFileName()

  public Texture2D LoadPNG(string filePath) {

    Texture2D tex = null;
    byte[] fileData;

    if (File.Exists(filePath)) {
      fileData = File.ReadAllBytes(filePath);
      tex = new Texture2D(TargetScreenWidth, TargetScreenHeight);
      tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
    }
    return tex;
  }   // LoadPNG()

  public void SaveRenderTexture(RenderTexture rt, string FilePath) {
    byte[] bytes = ToTexture2D(rt).EncodeToPNG();
    System.IO.File.WriteAllBytes(FilePath, bytes);
  }   //SaveRenderTexture()

  Texture2D ToTexture2D(RenderTexture rt) {
    var tex = new Texture2D(TargetScreenWidth, TargetScreenHeight, TextureFormat.RGB24, false);

    var savedRT = RenderTexture.active;
    RenderTexture.active = rt;
    //ReadPixels(Rect source, int destX, int destY);
    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
    // read from the active  renderTexture to tex
    tex.Apply();

    RenderTexture.active = savedRT;

    return tex;

  }  //ToTexture2D

  public void CaptureScreenToFileName(string name) {
    // if (Input.GetKeyDown(KeyCode.F12)) {
    //if (Input.GetKeyDown(KeyCode.Space))
    //{

    // If the resolution of gameview = 1920 x 1080, superSize =2 ==>
    // 2 x2 larger image is obtained => 3840 x 2160
    // if the resolution of gamevew = 960 x 540, supersize = 4 => 4K
    // The compression ratio of TIFF= 2:1, That of png = 2.7:1 (lossless?)

    if (CaptureOrProjectOrView == -1) {
      return;
    }

    if (CaptureOrProjectOrView == 0) {
      // print("Enter the name of the predistorted image");
      // dataPath = unity project folder/Assets

      string filePath = Application.dataPath + "/Resources/RenderedImages/";
      string fileName = filePath + name + /*CurrentSamplingCount + "_" + Time.time + */ ".png";

      // save the renderTexture _converged which holds the result of cameraMain's rendering
      SaveRenderTexture(ConvergedRenderTexForNewImage, fileName);

      DistortedResultImage = LoadPNG(fileName);

      // Dump the current renderTexture to _PredistortedImage renderTexture which was set in the
      // Inspector.

      // Opens a file selection dialog for a PNG file and saves a selected texture to the file.
      // import System.IO;

      //function Attive()
      // {

      //var texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

      //var textures = new Texture2D(128, 128, TextureFormat.ARGB32, false);
      //if (textures.Length == 0)
      //{
      //    EditorUtility.DisplayDialog("Select Textures",
      //                    "The selection must contain at least one texture!", "Ok");
      //    return;
      //}
      //  var path = UnityEditor.EditorUtility.SaveFolderPanel("Save textures to directory", "", "");
      //if (path.Length != 0)
      //{
      //    // for( var texture : Texture2D in textures) {
      //    // Convert the texture to a format compatible with EncodeToPNG
      //    if (texture.format != TextureFormat.DXT1 && texture.format != TextureFormat.RGB24)
      //    {
      //        var newTexture = Texture2D(texture.width, texture.height);
      //        newTexture.SetPixels(texture.GetPixels(0), 0);
      //        texture = newTexture;
      //    }
      //    var pngData = texture.EncodeToPNG();
      //    if (pngData != null)
      //        File.WriteAllBytes(path + "/" + nameTexture.text + ".png", pngData);
      //    else
      //        Debug.Log("Could not convert " + texture.name + " to png. Skipping saving texture");
      //}

      // UnityEditor.AssetDatabase.Refresh();

      //}


      // SaveTexture(_convergedForCreateImage, _PredistortedImage);
      // _PredistortedImage will be used by "Project Predistorted Image" task.


      //ScreenCapture.CaptureScreenshot(fileName, superSize);
      Debug.Log("The PredistortedImage Screen Captured to the Folder=" + filePath);


      // _CaptureOrProjectOrView = -1; // "-1" means no process is in progress

      CaptureOrProjectOrView = -1;
      // Now that the screen image has been saved, enable Rendering


      mPlaceHolder.SetActive(false);  // clean the path name box

      //StopPlay(); // stop the play of the current task and be ready for the next button command
      //mPauseNewRendering = true;

    }
    else if (CaptureOrProjectOrView == 1) {
      //string fileName = name + _currentSample + Time.time + ".png";

      //string fileName = Application.persistentDataPath + name + _currentSample + Time.time + ".png";
      string filePath = Application.dataPath + "/Resources/RenderedImages/";
      string fileName = filePath + name + CurrentSamplingCount + "_" + Time.time + ".png";

      // save the renderTexture _converged which holds the result of cameraMain's rendering
      SaveRenderTexture(ConvergedRenderTexForProjecting, fileName);

      // Dump the current renderTexture to _ProjectedImage renderTexture which was set in the
      // Inspector.

      ProjectedResultImage = LoadPNG(fileName);
      // _ProjectedImage will be used by "View Panorama Image" task.

      //ScreenCapture.CaptureScreenshot(fileName, superSize);
      Debug.Log("The Projected Image Screen Captured (View Independent Image to Folder="
                      + filePath);

      // _CaptureOrProjectOrView = -1; // "-1" means no process is in progress
      CaptureOrProjectOrView = -1;
      // Now that the screen image has been saved, enable Rendering


      mPlaceHolder.SetActive(false);  // clean the path name box

      //StopPlay(); // stop the play of the current task and be ready for the next button command
      // mPauseNewRendering = true;


    }
    else if (CaptureOrProjectOrView == 2) {
      //string fileName = name + _currentSample + Time.time + ".png";

      //string fileName = Application.persistentDataPath + name + _currentSample + Time.time + ".png";

      string filePath = Application.dataPath + "/Resources/RenderedImages/";
      string fileName = filePath + name + CurrentSamplingCount + "_" + Time.time + ".png";

      // save the renderTexture _converged which holds the result of cameraMain's rendering
      SaveRenderTexture(ConvergedRenderTexForPresenting, fileName);

      //ScreenCapture.CaptureScreenshot(fileName, superSize);
      Debug.Log("The Projected Image Screen Captured (View Dependent Image) to Folder="
                 + filePath);

      //_CaptureOrProjectOrView = -1; // "-1" means no process is in progress
      CaptureOrProjectOrView = -1;
      // Now that the screen image has been saved, enable Rendering


      mPlaceHolder.SetActive(false);  // clean the path name box

      //StopPlay(); // stop the play of the current task and be ready for the next button command
      // mPauseNewRendering = true;
    }
    else {
      Debug.LogError("_CaptureOrProjectOrView should be 0, 1, 2:" + CaptureOrProjectOrView);
      StopPlay();
    }
  } // CaptureScreenToFileName

  void Update() {
    if (Input.GetKeyDown(KeyCode.Q)) {
      QuitJob();
    }

    //if (_cameraMain.fieldOfView != _lastFieldOfView)
    //{
    //    _currentSample = 0;
    //    _lastFieldOfView = _cameraMain.fieldOfView;
    //}

    foreach (var t in TransformListToWatch) {
      if (t.hasChanged) {
        CurrentSamplingCount = 0;
        // restart to raytrace   when these transforms have been changed
        t.hasChanged = false;
      }
    }
    //RebuildObjectBuffersWithoutMirror();
    //InitCreatePreDistortedImage();
    //SetShaderFrameParameters();  // parameters need to be set every frame
  }   // Update()

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
    _triangularConeMirrorObjects.Add(obj);
    bConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterTriangularConeMirror(TriangularConeMirrorObject obj) {
    _triangularConeMirrorObjects.Remove(obj);
    bConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterPyramidMirror(PyramidMirrorObject obj) {
    Debug.Log("Pyramid Mirror registered");
    _pyramidMirrorObjects.Add(obj);
    bPyramidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterPyramidMirror(PyramidMirrorObject obj) {
    _pyramidMirrorObjects.Remove(obj);
    bPyramidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterParaboloidMirror(ParaboloidMirrorObject obj) {
    Debug.Log("Paraboloid Mirror registered");
    _paraboloidMirrorObjects.Add(obj);
    bParaboloidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterParaboloidMirror(ParaboloidMirrorObject obj) {
    _paraboloidMirrorObjects.Remove(obj);
    bParaboloidMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterHemisphereMirror(HemisphereMirrorObject obj) {
    Debug.Log("Hemisphere Mirror registered");
    _hemisphereMirrorObjects.Add(obj);
    bHemisphereMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterHemisphereMirror(HemisphereMirrorObject obj) {
    _hemisphereMirrorObjects.Remove(obj);
    bHemisphereMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterGeoConeMirror(GeoConeMirrorObject obj) {
    Debug.Log("Geometric Cone Mirror registered");
    _geoConeMirrorObjects.Add(obj);
    bGeoConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterGeoConeMirror(GeoConeMirrorObject obj) {
    _geoConeMirrorObjects.Remove(obj);
    bGeoConeMirrorNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void RegisterPanoramaMesh(PanoramaMeshObject obj) {
    Debug.Log("panorama Mesh registered");
    _panoramaMeshObjects.Add(obj);
    bPanoramaMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

  public static void UnregisterPanoramaMesh(PanoramaMeshObject obj) {
    _panoramaMeshObjects.Remove(obj);
    bPanoramaMeshObjectNeedRebuild = true;
    bObjectsNeedRebuild = true;
  }

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


  void CreateDebugBuffers() {
    //// for debugging
    //_meshObjectArray = new MeshObjectRW[_meshObjects.Count];

    //int meshObjRWStride = 16 * sizeof(float) + sizeof(float)
    //               + 3 * 3 * sizeof(float);


    //_meshObjectBufferRW = new ComputeBuffer(_meshObjects.Count, meshObjRWStride);



    ////ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.

    _vertexBufferRW = new ComputeBuffer(VerticesList.Count, 3 * sizeof(float), ComputeBufferType.Default);
    mIntersectionBuffer = new ComputeBuffer(TargetScreenWidth * TargetScreenHeight, 4 * sizeof(float), ComputeBufferType.Default);
    mRayDirectionBuffer = new ComputeBuffer(TargetScreenWidth * TargetScreenHeight, 4 * sizeof(float), ComputeBufferType.Default);
    mIntersectionBuffer = new ComputeBuffer(TargetScreenWidth * TargetScreenHeight, 4 * sizeof(float), ComputeBufferType.Default);
    mAccumRayEnergyBuffer = new ComputeBuffer(TargetScreenWidth * TargetScreenHeight, 4 * sizeof(float), ComputeBufferType.Default);
    mEmissionBuffer = new ComputeBuffer(TargetScreenWidth * TargetScreenHeight, 4 * sizeof(float), ComputeBufferType.Default);
    mSpecularBuffer = new ComputeBuffer(TargetScreenWidth * TargetScreenHeight, 4 * sizeof(float), ComputeBufferType.Default);


    mVertexArray = new Vector3[VerticesList.Count];

    mRayDirectionArray = new Vector4[TargetScreenWidth * TargetScreenHeight];
    mIntersectionArray = new Vector4[TargetScreenWidth * TargetScreenHeight];

    mAccumRayEnergyArray = new Vector4[TargetScreenWidth * TargetScreenHeight];

    mEmissionArray = new Vector4[TargetScreenWidth * TargetScreenHeight];
    mSpecularArray = new Vector4[TargetScreenWidth * TargetScreenHeight];

    //The static Array.Clear() method "sets a range of elements in the Array to zero, to false, or to Nothing, depending on the element type".If you want to clear your entire array, you could use this method an provide it 0 as start index and myArray.Length as length:
    // Array.Clear(mUVMapArray, 0, mUVMapArray.Length);


    // _meshObjectBufferRW.SetData(_meshObjectArray);


    _vertexBufferRW.SetData(mVertexArray);

    mRayDirectionBuffer.SetData(mRayDirectionArray);

    mIntersectionBuffer.SetData(mIntersectionArray);

    mAccumRayEnergyBuffer.SetData(mAccumRayEnergyArray);

    mEmissionBuffer.SetData(mEmissionArray);
    mSpecularBuffer.SetData(mSpecularArray);


  }
  void RebuildObjectBuffers() {
    if (!bObjectsNeedRebuild) {
      Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
      Debug.Log("The mesh objects are already built");
      Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");

      return;
    }


    bObjectsNeedRebuild = false;

    CurrentSamplingCount = 0;

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

    if (_pyramidMirrorObjects.Count != 0) {
      RebuildPyramidMirrorBuffer();
      mirrorDefined = true;
    }
    else if (_triangularConeMirrorObjects.Count != 0) {
      RebuildTriangularConeMirrorBuffer();
      mirrorDefined = true;
    }
    else if (_geoConeMirrorObjects.Count != 0) {
      RebuildGeoConeMirrorBuffer();
      mirrorDefined = true;
    }
    else if (_paraboloidMirrorObjects.Count != 0) {
      RebuildParaboloidMirrorBuffer();
      mirrorDefined = true;
    }
    else if (_hemisphereMirrorObjects.Count != 0) {
      RebuildHemisphereMirrorBuffer();
      mirrorDefined = true;
    }
    // Either panoramaScreenObject or panoramaMeshObject should be defined
    // so that the projector image will be projected onto it.

    if (!mirrorDefined) {
      Debug.LogError("A mirror should be defined");
      StopPlay();
    }

    if (_panoramaMeshObjects.Count != 0) {
      RebuildPanoramaMeshBuffer();
    }
    else {
      Debug.LogError(" panoramaMeshObject should be defined\n" +
                     "so that the projector image will be projected onto it.");
      StopPlay();

    }


    //if (_meshObjects.Count != 0)
    if (RayTracedObjectsList.Count != 0) {
      RebuildMeshObjectBuffer();
    }


    // create computeBuffers holding the vertices information about the various
    // objects created by the above RebuildXBuffer()'s

    CreateComputeBuffer(ref _vertexBuffer, VerticesList, 12);
    CreateComputeBuffer(ref _indexBuffer, IndicesList, 4);
    CreateComputeBuffer(ref _texcoordsBuffer, TexcoordsList, 8);

    #region debug
    CreateDebugBuffers();
    #endregion


  }  // RebuildObjectBuffers()

  void RebuildObjectBuffersWithoutMirror() {
    if (!bObjectsNeedRebuild) {
      return;
    }

    bObjectsNeedRebuild = false;

    CurrentSamplingCount = 0;

    // Clear all lists
    //_meshObjects.Clear();
    VerticesList.Clear();
    IndicesList.Clear();
    TexcoordsList.Clear();


    // Either panoramaScreenObject or panoramaMeshObject should be defined
    // so that the projector image will be projected onto it.


    if (_panoramaMeshObjects.Count != 0) {
      RebuildPanoramaMeshBuffer();
    }
    else {
      Debug.LogError(" panoramaMeshObject should be defined\n" +
                     "so that the projector image will be projected onto it.");
      StopPlay();

    }



    if (RayTracedObjectsList.Count != 0) {
      RebuildMeshObjectBuffer();
    }


    // create computeBuffers holding the vertices information about the various
    // objects created by the above RebuildXBuffer()'s

    CreateComputeBuffer(ref _vertexBuffer, VerticesList, 12);
    CreateComputeBuffer(ref _indexBuffer, IndicesList, 4);
    CreateComputeBuffer(ref _texcoordsBuffer, TexcoordsList, 8);

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
        albedo = obj.mMeshOpticalProperty.albedo,

        specular = obj.mMeshOpticalProperty.specular,
        smoothness = obj.mMeshOpticalProperty.smoothness,
        emission = obj.mMeshOpticalProperty.emission,

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
                    + 3 * 3 * sizeof(float) + sizeof(float) + 2 * sizeof(int);

    // create a computebuffer and set the data to it
    // If _meshObjects.Count ==0 ==> the computeBuffer is not created.

    CreateComputeBuffer(ref _meshObjectBuffer, RayTracedMeshObjectsList, meshObjStride);

    //CreateComputeBuffer(ref _vertexBuffer, _vertices, 12);
    //CreateComputeBuffer(ref _indexBuffer, _indices, 4);
    //CreateComputeBuffer(ref _texcoordsBuffer, _texcoords, 8);




  }   // RebuildMeshObjectBuffer()


  void RebuildTriangularConeMirrorBuffer() {


    // // if obj.mirrorType is the given mirrorType

    if (!bConeMirrorNeedRebuild) {
      return;
    }

    bConeMirrorNeedRebuild = false;

    // Clear all lists
    _triangularConeMirrors.Clear();
    //_triangularConeMirrorVertices.Clear();
    //_triangularConeMirrorIndices.Clear();


    // Clear all lists
    //_meshObjects.Clear();
    //_vertices.Clear();
    //_indices.Clear();
    //_texcoords.Clear();


    var obj = _triangularConeMirrorObjects[0];

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
    _triangularConeMirrors.Add(new TriangularConeMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,

      distanceToOrigin = obj.mConeParam.distanceFromCamera,
      height = obj.mConeParam.height,
      notUseRatio = obj.mConeParam.notUseRatio,
      radius = obj.mConeParam.radius,
      albedo = obj.mMeshOpticalProperty.albedo,

      specular = obj.mMeshOpticalProperty.specular,
      smoothness = obj.mMeshOpticalProperty.smoothness,
      emission = obj.mMeshOpticalProperty.emission,
      indices_offset = countOfCurrentIndices,
      indices_count = indices.Length // set the index count of the mesh of the current obj
    }
    );

    //      public struct TriangularConeMirror
    //{
    //    public Matrix4x4 localToWorldMatrix;

    //    public float distanceToOrigin;
    //    public float height;
    //    public float notUseRatio;
    //    public float radius;

    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public int indices_offset;
    //    public int indices_count;
    //}



    int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                 + 5 * sizeof(float) + 2 * sizeof(int);

    // create a computebuffer and set the data to it

    CreateComputeBuffer(ref _triangularConeMirrorBuffer, _triangularConeMirrors, stride);

    //CreateComputeBuffer(ref _triangularConeMirrorVertexBuffer,
    //                   _triangularConeMirrorVertices, 12);
    //CreateComputeBuffer(ref _triangularConeMirrorIndexBuffer,
    //                   _triangularConeMirrorIndices, 4);




  }   // RebuildTriangularConeMirrorBuffer()


  void RebuildHemisphereMirrorBuffer() {


    // // if obj.mirrorType is the given mirrorType

    if (!bHemisphereMirrorNeedRebuild) {
      return;
    }

    bHemisphereMirrorNeedRebuild = false;

    // Clear all lists
    _hemisphereMirrors.Clear();
    //_triangularConeMirrorVertices.Clear();
    //_triangularConeMirrorIndices.Clear();


    // Clear all lists
    //_meshObjects.Clear();
    //_vertices.Clear();
    //_indices.Clear();
    //_texcoords.Clear();


    var obj = _hemisphereMirrorObjects[0];

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
    _hemisphereMirrors.Add(new HemisphereMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,

      distanceToOrigin = obj.mHemisphereParam.distanceFromCamera,
      height = obj.mHemisphereParam.height,
      notUseRatio = obj.mHemisphereParam.notUseRatio,
      radius = obj.mHemisphereParam.radius,
      albedo = obj.mMeshOpticalProperty.albedo,

      specular = obj.mMeshOpticalProperty.specular,
      smoothness = obj.mMeshOpticalProperty.smoothness,
      emission = obj.mMeshOpticalProperty.emission,
      //indices_offset = countOfCurrentIndices,
      //indices_count = indices.Length // set the index count of the mesh of the current obj
    }
    );

    //      public struct TriangularConeMirror
    //{
    //    public Matrix4x4 localToWorldMatrix;

    //    public float distanceToOrigin;
    //    public float height;
    //    public float notUseRatio;
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
                + 5 * sizeof(float);
    // create a computebuffer and set the data to it

    CreateComputeBuffer(ref _hemisphereMirrorBuffer,
                          _hemisphereMirrors, stride);

    //CreateComputeBuffer(ref _triangularConeMirrorVertexBuffer,
    //                   _triangularConeMirrorVertices, 12);
    //CreateComputeBuffer(ref _triangularConeMirrorIndexBuffer,
    //                   _triangularConeMirrorIndices, 4);




  }   // RebuildHemisphereMirrorBuffer()


  // Build the vertices and the indices of the mesh for the mirror object within the script
  void RebuildPyramidMirrorBuffer() {


    if (!bPyramidMeshObjectNeedRebuild) {
      return;
    }

    bPyramidMeshObjectNeedRebuild = false;


    // Clear all lists
    _pyramidMirrors.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = _pyramidMirrorObjects[0];
    // _mirrorType = obj.mMirrorType;



    // Add the object itself
    _pyramidMirrors.Add(new PyramidMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,
      albedo = obj.mMeshOpticalProperty.albedo,

      specular = obj.mMeshOpticalProperty.specular,
      smoothness = obj.mMeshOpticalProperty.smoothness,
      emission = obj.mMeshOpticalProperty.emission,
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
                              + 4 * sizeof(float);

    CreateComputeBuffer(ref _pyramidMirrorBuffer, _pyramidMirrors, pyramidMirrorStride);

  }   // RebuildPyramidMirrorObjectBuffer()

  // Build the vertices and the indices of the mesh for the mirror object within the script
  void RebuildGeoConeMirrorBuffer() {

    if (!bGeoConeMirrorNeedRebuild) {
      return;
    }

    bGeoConeMirrorNeedRebuild = false;


    // Clear all lists
    _geoConeMirrors.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = _geoConeMirrorObjects[0];
    // _mirrorType = obj.mMirrorType;

    // Add the object itself
    _geoConeMirrors.Add(
      new GeoConeMirror() {
        localToWorldMatrix = obj.transform.localToWorldMatrix,
        distanceToOrigin = obj.mConeParam.distanceFromCamera,
        height = obj.mConeParam.height,
        notUseRatio = obj.mConeParam.notUseRatio,
        radius = obj.mConeParam.radius,
        albedo = obj.mMeshOpticalProperty.albedo,

        specular = obj.mMeshOpticalProperty.specular,
        smoothness = obj.mMeshOpticalProperty.smoothness,
        emission = obj.mMeshOpticalProperty.emission,



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
    //    public float notUseRatio;
    //    public float radius;  // the radius of the base of the cone



    //};




    int geoConeMirrorStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                                  + 5 * sizeof(float);

    CreateComputeBuffer(ref _geoConeMirrorBuffer, _geoConeMirrors, geoConeMirrorStride);



  }    //RebuildGeoConeMirrorBuffer()

  void RebuildParaboloidMirrorBuffer() {
    if (!bParaboloidMeshObjectNeedRebuild) {
      return;
    }

    bParaboloidMeshObjectNeedRebuild = false;

    // Clear all lists
    _paraboloidMirrors.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = _paraboloidMirrorObjects[0];
    // _mirrorType = obj.mMirrorType;



    // Add the object itself
    _paraboloidMirrors.Add(
      new ParaboloidMirror() {
        localToWorldMatrix = obj.transform.localToWorldMatrix,
        distanceToOrigin = obj.mParaboloidParam.distanceFromCamera,
        height = obj.mParaboloidParam.height,
        notUseRatio = obj.mParaboloidParam.notUseRatio,
        albedo = obj.mMeshOpticalProperty.albedo,

        specular = obj.mMeshOpticalProperty.specular,
        smoothness = obj.mMeshOpticalProperty.smoothness,
        emission = obj.mMeshOpticalProperty.emission,

        coefficientA = obj.mParaboloidParam.coefficientA,
        coefficientB = obj.mParaboloidParam.coefficientB,

      }
    );

    //        struct ParaboloidMirror
    //{
    //    public Matrix4x4 localToWorldMatrix; // the world frame of the cone
    //    public float distanceToOrigin; // distance from the camera to the origin of the paraboloid
    //    public float height;
    //    public float notUseRatio; //
    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public float coefficientA;  // z = - ( x^2/a^2 + y^2/b^2)
    //    public float coefficientB;

    //};

    int paraboloidMirrorStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                                  + 6 * sizeof(float);
    CreateComputeBuffer(ref _paraboloidMirrorBuffer, _paraboloidMirrors, paraboloidMirrorStride);

  }   // RebuildParaboloidMirrorObjectBuffer()


  void RebuildPanoramaMeshBuffer() {

    //if (!_panoramaMeshObjectsNeedRebuilding) {
    //  return;
    //}

    //_panoramaMeshObjectsNeedRebuilding = false;


    // Clear all lists
    _panoramaMeshes.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = _panoramaMeshObjects[0];


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
    _panoramaMeshes.Add(
      new PanoramaMesh() {
        localToWorldMatrix = obj.transform.localToWorldMatrix,
        highRange = obj.panoramaParams.highRangeFromCamera,
        lowRange = obj.panoramaParams.lowRangeFromCamera,
        albedo = obj.meshMaterialProp.albedo,

        specular = obj.meshMaterialProp.specular,
        smoothness = obj.meshMaterialProp.smoothness,
        emission = obj.meshMaterialProp.emission,

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

    CreateComputeBuffer(ref _panoramaMeshBuffer, _panoramaMeshes, panoramaMeshStride);


  }   // RebuildPanoramaMeshBuffer()



  private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
     where T : struct {
    // Do we already have a compute buffer?
    if (buffer != null) {
      // If no data or buffer doesn't match the given criteria, release it
      if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to
      // begin with, create it
      if (buffer == null) {

        buffer = new ComputeBuffer(data.Count, stride);
      }

      // Set data on the buffer
      buffer.SetData(data);
    }
    // buffer is not created, and remains to be null


  }   //CreateComputeBuffer


  void SetShaderFrameParameters() {

    var pixelOffset = new Vector2(Random.value, Random.value);

    RayTracerShader.SetVector("_PixelOffset", pixelOffset);

    //Debug.Log("_PixelOffset =" + pixelOffset);

    //float seed = Random.value;

    //RayTracingShader.SetFloat("_Seed", seed);

    //Debug.Log("_Seed =" + seed);



  }   //SetShaderFrameParameters()



  void SetBuffersToShaderForDebugging() {
    RayTracerShader.SetBuffer(mKernelToUse, "_VertexBufferRW", _vertexBufferRW);
    RayTracerShader.SetBuffer(mKernelToUse, "_RayDirectionBuffer", mRayDirectionBuffer);

    RayTracerShader.SetBuffer(mKernelToUse, "_IntersectionBuffer", mIntersectionBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_AccumRayEnergyBuffer", mAccumRayEnergyBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_EmissionBuffer", mEmissionBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_SpecularBuffer", mSpecularBuffer);

  }

  void InitRenderTextureForCreateImage() {

    //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
    // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)    

    if (TargetRenderTex == null) {
      // Create the camera's render target for Ray Tracing
      //_Target = new RenderTexture(Screen.width, Screen.height, 0,
      TargetRenderTex = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

      //Render Textures can also be written into from compute shaders,
      //if they have “random access” flag set(“unordered access view” in DX11).

      TargetRenderTex.enableRandomWrite = true;
      TargetRenderTex.Create();

    }
    if (ConvergedRenderTexForNewImage == null) {
      //_converged = new RenderTexture(Screen.width, Screen.height, 0,
      ConvergedRenderTexForNewImage = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                     RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      ConvergedRenderTexForNewImage.enableRandomWrite = true;
      ConvergedRenderTexForNewImage.Create();
    }

    DistortedResultImage = new Texture2D(TargetScreenWidth, TargetScreenHeight, TextureFormat.RGBAFloat, false);
    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    Dbg_RWTex = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    Dbg_RWTex.enableRandomWrite = true;
    Dbg_RWTex.Create();


    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    //_MainScreenRT = new RenderTexture(Screen.width, Screen.height, 0,
    //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //_MainScreenRT.enableRandomWrite = true;
    //_MainScreenRT.Create();

    // Reset sampling
    CurrentSamplingCount = 0;




  }  //InitRenderTextureForCreateImage()

  void InitRenderTextureForProjectImage() {

    //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
    // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)



    if (TargetRenderTex == null) {

      // Create the camera's render target for Ray Tracing
      //_Target = new RenderTexture(Screen.width, Screen.height, 0,
      TargetRenderTex = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

      //Render Textures can also be written into from compute shaders,
      //if they have “random access” flag set(“unordered access view” in DX11).

      TargetRenderTex.enableRandomWrite = true;
      TargetRenderTex.Create();

    }

    if (ConvergedRenderTexForProjecting == null) {
      //_converged = new RenderTexture(Screen.width, Screen.height, 0,
      ConvergedRenderTexForProjecting = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                     RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      ConvergedRenderTexForProjecting.enableRandomWrite = true;
      ConvergedRenderTexForProjecting.Create();

    }
    //_converged = new RenderTexture(Screen.width, Screen.height, 0,

    ProjectedResultImage = new Texture2D(TargetScreenWidth, TargetScreenHeight, TextureFormat.RGBAFloat, false);
    //_PredistortedImage = new RenderTexture(ScreenWidth, ScreenHeight, 0,
    //                              RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

    ////Make _PredistortedImage to be a random access texture
    //_PredistortedImage.enableRandomWrite = true;
    //_PredistortedImage.Create();

    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    Dbg_RWTex = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    Dbg_RWTex.enableRandomWrite = true;
    Dbg_RWTex.Create();

    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    //_MainScreenRT = new RenderTexture(Screen.width, Screen.height, 0,
    //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //_MainScreenRT.enableRandomWrite = true;
    //_MainScreenRT.Create();

    // Reset sampling
    CurrentSamplingCount = 0;

  }  //InitRenderTextureForProjectImage()
  void InitRenderTextureForViewImage() {

    //if (_Target == null || _Target.width != Screen.width || _Target.height != Screen.height)
    // if (_Target == null || _Target.width != ScreenWidth || _Target.height != ScreenHeight)



    if (TargetRenderTex == null) {

      // Create the camera's render target for Ray Tracing
      //_Target = new RenderTexture(Screen.width, Screen.height, 0,
      TargetRenderTex = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

      //Render Textures can also be written into from compute shaders,
      //if they have “random access” flag set(“unordered access view” in DX11).

      TargetRenderTex.enableRandomWrite = true;
      TargetRenderTex.Create();
    }

    if (ConvergedRenderTexForPresenting == null) {
      //_converged = new RenderTexture(Screen.width, Screen.height, 0,
      ConvergedRenderTexForPresenting = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                     RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      ConvergedRenderTexForPresenting.enableRandomWrite = true;
      ConvergedRenderTexForPresenting.Create();

    }


    //_ProjectedImage = new RenderTexture(ScreenWidth, ScreenHeight, 0,
    //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //_ProjectedImage.enableRandomWrite = true;
    //_ProjectedImage.Create();

    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    Dbg_RWTex = new RenderTexture(TargetScreenWidth, TargetScreenHeight, 0,
                                   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    Dbg_RWTex.enableRandomWrite = true;
    Dbg_RWTex.Create();




    //_converged = new RenderTexture(Screen.width, Screen.height, 0,
    //_MainScreenRT = new RenderTexture(Screen.width, Screen.height, 0,
    //                               RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //_MainScreenRT.enableRandomWrite = true;
    //_MainScreenRT.Create();

    // Reset sampling
    CurrentSamplingCount = 0;



  }  //InitRenderTextureForViewImage()

  void OnRenderImage(RenderTexture source, RenderTexture destination) {

    if (CaptureOrProjectOrView == -1) {
      return; // if the task is not yet selected, do not render and return;
    }

    if (CaptureOrProjectOrView == 0) {
      if (bStopRender)  // PauseNewRendering is true when a task is completed and another task is not selected
                          // In this situation, the framebuffer is not updated, but the same content is transferred to the framebuffer
                          // to make the screen alive

      {
        Debug.Log("current sample not incremented =" + CurrentSamplingCount);
        Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

        // Ignore  the target Texture of the camera in order to blit to the null target (which is
        // the framebuffer

        //_cameraMain.targetTexture = null;
        //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
        Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);
        return;

      }
      else {
        //Debug.Log("current sample=" + _currentSample);


        int threadGroupsX = Mathf.CeilToInt(TargetScreenWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(TargetScreenHeight / 8.0f);

        //Different mKernelToUse is used depending on the task, that is, on the value
        // of _CaptureOrProjectOrView

        RayTracerShader.Dispatch(mKernelToUse, threadGroupsX, threadGroupsY, 1);
        // This dispatch of the compute shader will set _Target TWTexure2D


        // Blit the result texture to the screen
        if (AddMaterial_WholeSizeScreenSampling == null) {
          AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
        }

        AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCount);
        // TODO: Upscale To 4K and downscale to 1k.
        //_Target is the RWTexture2D created by the computeshader
        // note that  _cameraMain.targetTexture = _convergedForCreateImage by OnPreRender();   =>
        // not used right now.

        // Blit (source, dest, material) sets dest as the render target, and source as _MainTex property
        // on the material and draws a full-screen quad.
        //If  dest == null, the screen backbuffer is used as
        // the blit destination, EXCEPT if the Camera.main has a non-null targetTexture;
        // If the Camera.main has a non-null targetTexture, it will be the target even if 
        // dest == null.

        Graphics.Blit(TargetRenderTex, ConvergedRenderTexForNewImage, AddMaterial_WholeSizeScreenSampling);

        // Ignore the target Texture of the camera in order to blit to the null target (which is
        // the framebuffer

        //_cameraMain.targetTexture = null;  // tells Blit to ignore the currently active target render texture
        //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
        Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);


        CurrentSamplingCount++;
        // Each cycle of rendering, a new location within every pixel area is sampled 
        // for the purpose of  anti-aliasing.


      }  // else of if (mPauseNewRendering)

    }   // _CaptureOrProjectOrView

    else if (CaptureOrProjectOrView == 1) {
      // used the result of the rendering (raytracing shader)
      //debug
      // RayTracingShader.SetTexture(mKernelToUse, "_Result", _Target);
      RayTracerShader.SetTexture(mKernelToUse, "_DebugRWTexture", Dbg_RWTex);

      if (bStopRender)  // PauseNewRendering is true during saving image                                  // to make the screen alive

      {
        Debug.Log("current sample not incremented =" + CurrentSamplingCount);
        // Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

        // Null the target Texture of the camera and blit to the null target (which is
        // the framebuffer

        // _cameraMain.targetTexture = null; // tells Blit to ignore the currently active target render texture
        //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
        Graphics.Blit(ConvergedRenderTexForProjecting, default(RenderTexture));
        return;
      }
      else {
        Debug.Log("current sample=" + CurrentSamplingCount);

        int threadGroupsX = Mathf.CeilToInt(TargetScreenWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(TargetScreenHeight / 8.0f);

        //Different mKernelToUse is used depending on the task, that is, on the value
        // of _CaptureOrProjectOrView

        RayTracerShader.Dispatch(mKernelToUse, threadGroupsX, threadGroupsY, 1);

        // This dispatch of the compute shader will set _Target TWTexure2D


        // Blit the result texture to the screen
        if (AddMaterial_WholeSizeScreenSampling == null) {
          AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
        }

        AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCount);

        // TODO: Upscale To 4K and downscale to 1k.
        //_Target is the RWTexture2D created by the computeshader
        // note that  _cameraMain.targetTexture = _convergedForProjectImage;

        //debug

        Graphics.Blit(TargetRenderTex, ConvergedRenderTexForProjecting, AddMaterial_WholeSizeScreenSampling);

        //debug


        // Null the target Texture of the camera and blit to the null target (which is
        // the framebuffer

        // _cameraMain.targetTexture = null;

        //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
        //Debug.Log("_Target: IsCreated?=");
        //Debug.Log(_Target.IsCreated());

        //debug
        //Graphics.Blit(_Target, null as RenderTexture);


        //debug


        //if (_currentSample == 0)
        //{
        //    DebugTexture(_PredistortedImage);
        //}


        // debug
        Graphics.Blit(ConvergedRenderTexForProjecting, null as RenderTexture);

        CurrentSamplingCount++;
        // Each cycle of rendering, a new location within every pixel area is sampled 
        // for the purpose of  anti-aliasing.


      }  // else of if (mPauseNewRendering)


    }
    else if (CaptureOrProjectOrView == 2) {
      if (bStopRender)  // PauseNewRendering is true when a task is completed and another task is not selected
                          // In this situation, the framebuffer is not updated, but the same content is transferred to the framebuffer
                          // to make the screen alive

      {
        Debug.Log("current sample not incremented =" + CurrentSamplingCount);
        Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

        // Null the target Texture of the camera and blit to the null target (which is
        // the framebuffer

        //_cameraMain.targetTexture = null;  //// tells Blit that  the current active target render texture is null,
        // which refers to the framebuffer
        //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
        Graphics.Blit(ConvergedRenderTexForPresenting, null as RenderTexture);
        return;

      }
      else {
        Debug.Log("current sample=" + CurrentSamplingCount);


        int threadGroupsX = Mathf.CeilToInt(TargetScreenWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(TargetScreenHeight / 8.0f);

        //Different mKernelToUse is used depending on the task, that is, on the value
        // of _CaptureOrProjectOrView

        RayTracerShader.Dispatch(mKernelToUse, threadGroupsX, threadGroupsY, 1);
        // This dispatch of the compute shader will set _Target TWTexure2D


        // Blit the result texture to the screen
        if (AddMaterial_WholeSizeScreenSampling == null) {
          AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
        }

        AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCount);
        // TODO: Upscale To 4K and downscale to 1k.
        //_Target is the RWTexture2D created by the computeshader
        // note that  _cameraMain.targetTexture = _converged;

        Graphics.Blit(TargetRenderTex, ConvergedRenderTexForPresenting, AddMaterial_WholeSizeScreenSampling);

        // Null the target Texture of the camera and blit to the null target (which is
        // the framebuffer

        //_cameraMain.targetTexture = null;
        //the destination (framebuffer= null) has a resolution of Screen.width x Screen.height
        Graphics.Blit(ConvergedRenderTexForPresenting, null as RenderTexture);

        //if (_currentSample == 0)
        //{
        //    DebugLogOfRWBuffers();
        //}

        CurrentSamplingCount++;
        // Each cycle of rendering, a new location within every pixel area is sampled 
        // for the purpose of  anti-aliasing.


      }  // else of if (mPauseNewRendering)
    }   // if (_CaptureOrProjectOrView == 2)
    else {

    }




  } // OnRenderImage()


  void DebugRenderTexture(RenderTexture target) {

    //save the active renderTexture
    var savedTarget = RenderTexture.active;

    RenderTexture.active = target;
    ////RenderTexture.active = _mainScreenRT;

    ////RenderTexture.active = _Target;

    //// Read pixels  from the currently active render texture, _Target
    ResultTex1.ReadPixels(new Rect(0, 0, TargetScreenWidth, TargetScreenHeight), 0, 0);
    ////Actually apply all previous SetPixel and SetPixels changes.
    ResultTex1.Apply();

    RenderTexture.active = savedTarget;




    for (int y = 0; y < TargetScreenHeight; y += 5) {
      for (int x = 0; x < TargetScreenWidth; x += 5) {
        int idx = y * TargetScreenWidth + x;


        Debug.Log("_Predistorted[" + x + "," + y + "]=");
        Debug.Log(ResultTex1.GetPixel(x, y));

      }
    }



  }   // DebugRenderTexture()


  void DebugTexture(Texture2D target) {


    for (int y = 0; y < TargetScreenHeight; y += 5) {
      for (int x = 0; x < TargetScreenWidth; x += 5) {
        int idx = y * TargetScreenWidth + x;


        Debug.Log("_PredistortedImage[" + x + "," + y + "]=");
        Debug.Log(ResultTex1.GetPixel(x, y));

      }
    }



  }   // DebugTexture()



  void DebugRenderTextures() {

    //RenderTexture.active = _Target;
    //////RenderTexture.active = _mainScreenRT;

    //////RenderTexture.active = _Target;

    ////// Read pixels  from the currently active render texture
    //_resultTexture.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
    //////Actually apply all previous SetPixel and SetPixels changes.
    //_resultTexture.Apply();


    RenderTexture.active = Dbg_RWTex;
    ////RenderTexture.active = _mainScreenRT;

    ////RenderTexture.active = _Target;

    //// Read pixels  from the currently active render texture
    ResultTex2.ReadPixels(new Rect(0, 0, TargetScreenWidth, TargetScreenHeight), 0, 0);
    ////Actually apply all previous SetPixel and SetPixels changes.
    ResultTex2.Apply();



    for (int y = 0; y < TargetScreenHeight; y += 5) {
      for (int x = 0; x < TargetScreenWidth; x += 5) {
        int idx = y * TargetScreenWidth + x;


        //Debug.Log("Target[" + x + "," + y + "]=");
        //Debug.Log(_resultTexture.GetPixel(x, y));
        Debug.Log("DebugRWTexture(index)[" + x + "," + y + "]=");
        Debug.Log(ResultTex2.GetPixel(x, y));

      }
    }

    RenderTexture.active = null; // added to avoid errors 

  }   // DebugRenderTextures()

  void DebugLogOfRWBuffers() {

    // for debugging: print the buffer

    //_vertexBufferRW.GetData(mVertexArray);

    ////Debug.Log("Triangles");
    //////int meshObjectIndex = 0;
    //foreach (var meshObj in _panoramaScreens)
    //{


    //    int indices_count = meshObj.indices_count;
    //    int indices_offset = meshObj.indices_offset;

    //    int triangleIndex = 0;

    //    for (int i = indices_offset; i < indices_offset + indices_count; i += 3)
    //    {
    //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i]].ToString("F6"));
    //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i + 1]].ToString("F6"));
    //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i + 2]].ToString("F6"));

    //        //Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i]].ToString("F6"));
    //        //Debug.Log((triangleIndex) + "uv :" + _texcoords[_indices[i + 1]].ToString("F6"));
    //        //Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i + 2]].ToString("F6"));


    //        ++triangleIndex;
    //    }  // for each triangle


    //} // for each meshObj

    //Debug.Log("Panorama Cylinder");

    //foreach (var meshObj in _panoramaScreens)
    //{


    //    int indices_count = meshObj.indices_count;
    //    int indices_offset = meshObj.indices_offset;

    //    int triangleIndex = 0;

    //    for (int i = indices_offset; i < indices_offset + indices_count; i += 3)
    //    {
    //        Debug.Log((triangleIndex) + "th triangle:" + _vertices[_indices[i]].ToString("F6"));
    //        Debug.Log((triangleIndex) + "th triangle:" + _vertices[_indices[i + 1]].ToString("F6"));
    //        Debug.Log((triangleIndex) + "th triangle:" + _vertices[_indices[i + 2]].ToString("F6"));

    //        Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i]].ToString("F6"));
    //        Debug.Log((triangleIndex) + "uv :" + _texcoords[_indices[i + 1]].ToString("F6"));
    //        Debug.Log((triangleIndex) + "uv:" + _texcoords[_indices[i + 2]].ToString("F6"));


    //        ++triangleIndex;
    //    }  // for each triangle


    //} // for each meshObj



    //RenderTexture.active = _PredistortedImage;
    //////RenderTexture.active = _mainScreenRT;

    //////RenderTexture.active = _Target;

    ////// Read pixels  from the currently active render texture
    //_resultTexture.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
    //////Actually apply all previous SetPixel and SetPixels changes.
    //_resultTexture.Apply();

    //save the active renderTexture
    var savedTarget = RenderTexture.active;

    RenderTexture.active = TargetRenderTex;
    ////RenderTexture.active = _mainScreenRT;

    ////RenderTexture.active = _Target;

    //// Read pixels  from the currently active render texture, _Target
    ResultTex2.ReadPixels(new Rect(0, 0, TargetScreenWidth, TargetScreenHeight), 0, 0);
    ////Actually apply all previous SetPixel and SetPixels changes.
    ResultTex2.Apply();

    RenderTexture.active = savedTarget;

    //RenderTexture.active = _DebugRWTexture;
    //////RenderTexture.active = _mainScreenRT;

    //////RenderTexture.active = _Target;

    ////// Read pixels  from the currently active render texture
    //_resultTexture3.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
    //////Actually apply all previous SetPixel and SetPixels changes.
    //_resultTexture3.Apply();




    // debugging for the ray 0
    // mRayDirectionBuffer.GetData(mRayDirectionArray);
    // mIntersectionBuffer.GetData(mIntersectionArray);
    mAccumRayEnergyBuffer.GetData(mAccumRayEnergyArray);
    mEmissionBuffer.GetData(mEmissionArray);
    //mSpecularBuffer.GetData(mSpecularArray);           

    for (int y = 0; y < TargetScreenHeight; y += 5) {
      for (int x = 0; x < TargetScreenWidth; x += 5) {
        int idx = y * TargetScreenWidth + x;


        //var myRayDir = mRayDirectionArray[idx];
        // var intersection = mIntersectionArray[idx];
        var accumRayEnergy = mAccumRayEnergyArray[idx];
        // var emission = mEmissionArray[idx];
        //var specular = mSpecularArray[idx];


        //for debugging


        // Debug.Log("(" + x + "," + y + "):" + "incoming ray direction=" + myRayDir.ToString("F6"));
        // Debug.Log("(" + x + "," + y + "):" + "hit point=" + intersection.ToString("F6"));


        Debug.Log("PassedPredistortedImage(" + x + "," + y + ")=" + accumRayEnergy.ToString("F6"));
        //Debug.Log("(" + x + "," + y + "):" + "unTex.xy +id.xy=" + emission.ToString("F6"));
        //Debug.Log("(" + x + "," + y + "):" + "reflected direction=" + specular.ToString("F6"));
        // Debug.Log("Predistorted[" + x + "," + y + "]=" + _resultTexture.GetPixel(x, y));
        Debug.Log("Target[" + x + "," + y + "]=" + ResultTex2.GetPixel(x, y));
        Debug.Log("DebugRWTexture(index) [" + x + "," + y + "]=" + ResTex3.GetPixel(x, y));

      } // for x
    }  // for y

    // RenderTexture.active = null;  

  } //    void DebugLogOfRWBuffers()

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


  //Event Handler for "CreatePredistortedImage" Button
  public void InitCreatePreDistortedImage() {

    CaptureOrProjectOrView = 0;

    //mPauseNewRendering = false;  // ready to render
    CurrentSamplingCount = 0;


    // it means that the raytracing process for obtatining
    // predistorted image is in progress
    // 
    // Make sure we have a current render target
    InitRenderTextureForCreateImage();
    // create _Target, _converge, _ProjectedImage renderTexture   (only once)

    // Set the parameters for the mirror object; 

    if (_triangularConeMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelCreateImageTriConeMirror;
        Debug.Log(" kernelCreateImageTriConeMirror is executed");
        RayTracerShader.SetBuffer(mKernelToUse, "_TriangularConeMirrors", _triangularConeMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama mesh should be defined");
        StopPlay();
      }

    }
    else if (_geoConeMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelCreateImageGeoConeMirror;
        Debug.Log("  kernelCreateImageGeoConeMirror is executed");
        RayTracerShader.SetBuffer(mKernelToUse, "_GeoConedMirrors", _geoConeMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama  mesh should be defined");
        StopPlay();
      }


    }
    else if (_paraboloidMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelCreateImageParaboloidMirror;
        Debug.Log("  kernelCreateImageParaboloidMirror is executed");

        RayTracerShader.SetBuffer(mKernelToUse, "_ParaboloidMirrors", _paraboloidMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama mesh should be defined");
        StopPlay();
      }

    }
    else if (_hemisphereMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelCreateImageHemisphereMirror;
        Debug.Log("  kernelCreateImageHemisphereMirror is executed");

        RayTracerShader.SetBuffer(mKernelToUse, "_HemisphereMirrors", _hemisphereMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama mesh should be defined");
        StopPlay();
      }

    }
    else {
      Debug.LogError("A mirror should be defined in the scene");
      StopPlay();
    }

    //Vector3 l = DirectionalLight.transform.forward;
    //RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

    //RayTracingShader.SetFloat("_FOV", Mathf.Deg2Rad * _cameraMain.fieldOfView);


    Debug.Log("_FOV" + Mathf.Deg2Rad * MainCamera.fieldOfView);
    Debug.Log("aspectRatio" + MainCamera.aspect + ":" + TargetScreenWidth / (float)TargetScreenHeight);

    RayTracerShader.SetInt("_MaxBounce", MaxNumOfBounce);

    RayTracerShader.SetBuffer(mKernelToUse, "_Vertices", _vertexBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_Indices", _indexBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_UVs", _texcoordsBuffer);

    RayTracerShader.SetBuffer(mKernelToUse, "_VertexBufferRW", _vertexBufferRW);



    // The disptached kernel mKernelToUse will do different things according to the value
    // of _CaptureOrProjectOrView,
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("_CaptureOrProjectOrView = " + CaptureOrProjectOrView);
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("888888888888888888888888888888888888888888");

    RayTracerShader.SetInt("_CaptureOrProjectOrView", CaptureOrProjectOrView);

    if (MainCamera != null) {
      RayTracerShader.SetMatrix("_Projection", MainCamera.projectionMatrix);
      RayTracerShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);
      RayTracerShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);
    }
    else {
      Debug.LogError("MainCamera should be activated");
      StopPlay();
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

    ClearRenderTexture(TargetRenderTex);

    RayTracerShader.SetTexture(mKernelToUse, "_Result", TargetRenderTex);  // used always      

    // set the textures
    RayTracerShader.SetTexture(mKernelToUse, "_SkyboxTexture", SkyboxTex);
    RayTracerShader.SetTexture(mKernelToUse, "_RoomTexture", PanoramaTex);

    #region debugging

    SetBuffersToShaderForDebugging();


    #endregion


  }   //InitCreatePreDistortedImage()

  //Event Handler for "ProjectPredistortedImage" Button
  public void InitProjectPreDistortedImage() {
    CurrentSamplingCount = 0;
    CaptureOrProjectOrView = 1;


    bStopRender = false;  // ready to render

    // it means that the raytracing process for projecting the predistorted
    // image onto the scene is in progress.
    //  _CaptureOrProjectOrView = 2 means that the raytracing process of viewing
    // the projected image is in progress.

    // RebuildObjectBuffers();

    // Make sure we have a current render target
    InitRenderTextureForProjectImage();
    // create _Target, _converge,  renderTexture   (only once)


    // Set the parameters for the mirror object; 

    // determine the kernel to use:
    if (_triangularConeMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelProjectImageTriConeMirror;
        Debug.Log("  kernelProjectImageTriConeMirror is executed");


        RayTracerShader.SetBuffer(mKernelToUse, "_TriangularConeMirrors",
                                  _triangularConeMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);



      }
      else {
        Debug.LogError("A panorama  mesh should be defined");
        StopPlay();
      }

    }
    else if (_geoConeMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelProjectImageGeoConeMirror;
        Debug.Log(" kernelProjectImageGeoConeMirror is executed");
        RayTracerShader.SetBuffer(mKernelToUse, "_GeoConedMirrors", _geoConeMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama  mesh should be defined");
        StopPlay();
      }


    }
    else if (_paraboloidMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelProjectImageParaboloidMirror;
        Debug.Log("  kernelProjectImageParaboloidMirror is executed");

        RayTracerShader.SetBuffer(mKernelToUse, "_ParaboloidMirrors", _paraboloidMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama  mesh should be defined");
        StopPlay();
      }

    }
    else if (_hemisphereMirrorBuffer != null) {
      if (_panoramaMeshBuffer != null) {
        mKernelToUse = kernelProjectImageHemisphereMirror;
        Debug.Log("  kernelProjectImageHemisphereMirror is executed");

        RayTracerShader.SetBuffer(mKernelToUse, "_HemisphereMirrors", _hemisphereMirrorBuffer);
        RayTracerShader.SetBuffer(mKernelToUse, "_PanoramaMeshes", _panoramaMeshBuffer);
      }
      else {
        Debug.LogError("A panorama  mesh should be defined");
        StopPlay();
      }

    }
    else {
      Debug.LogError("A mirror should be defined in the scene");
      StopPlay();
    }


    var l = DirectionalLight.transform.forward;
    RayTracerShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

    // Added by Moon Jung, 2020/1/21
    RayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamera.fieldOfView);

    // Added by Moon Jung, 2020/1/29
    RayTracerShader.SetInt("_MaxBounce", MaxNumOfBounce);
    // RayTracingShader.SetInt("_MirrorType", _mirrorType);  // _mirrorType should be set
    // in the inspector of this script component which is attached to the camera gameObject

    //SetComputeBuffer("_Spheres", _sphereBuffer); //  commented out by Moon Jung
    //RayTracingShader.SetBuffer  

    RayTracerShader.SetBuffer(mKernelToUse, "_Vertices", _vertexBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_Indices", _indexBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_UVs", _texcoordsBuffer);

    RayTracerShader.SetBuffer(mKernelToUse, "_VertexBufferRW", _vertexBufferRW);


    // The disptached kernel mKernelToUse will do different things according to the value
    // of _CaptureOrProjectOrView,
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("_CaptureOrProjectOrView = " + CaptureOrProjectOrView);
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("888888888888888888888888888888888888888888");

    RayTracerShader.SetInt("_CaptureOrProjectOrView", CaptureOrProjectOrView);

    if (MainCamera != null) {
      RayTracerShader.SetMatrix("_CameraToWorld", MainCamera.cameraToWorldMatrix);

      RayTracerShader.SetMatrix("_Projection", MainCamera.projectionMatrix);

      RayTracerShader.SetMatrix("_CameraInverseProjection", MainCamera.projectionMatrix.inverse);

    }
    else {
      Debug.LogError("MainCamera should be activated");
      StopPlay();
    }

    // CameraUser should be active all the time to show the menu
    //if (_cameraUser != null)
    //{
    //    Debug.Log("CameraUser will be deactivated");
    //    _cameraUser.enabled = false;
    //    //StopPlay();
    //}





    // use the result of the rendering (raytracing shader)
    //_PredistortedImage = _convergedForCreateImage;

    if (DistortedResultImage == null) {
      Debug.LogError("_PredistortedImage [RenderTexture] should be created by Create predistorted image");

      StopPlay();
    }
    else {
      RayTracerShader.SetTexture(mKernelToUse, "_PredistortedImage", DistortedResultImage);
    }




    //_Target.DiscardContents();
    // Clear the target render Texture _Target

    ClearRenderTexture(TargetRenderTex);

    RayTracerShader.SetTexture(mKernelToUse, "_Result", TargetRenderTex);


    #region debugging

    SetBuffersToShaderForDebugging();


    #endregion



  }   //InitProjectPreDistortedImage()

  //Event Handler for "ViewPanoramaScene" Button
  public void InitViewPanoramaScene() {
    UserCamera = GameObject.FindWithTag("CameraUser").GetComponent<Camera>();

    CaptureOrProjectOrView = 2;

    bStopRender = false;  // ready to render

    CurrentSamplingCount = 0;
    //  _CaptureOrProjectOrView = 2 means that the raytracing process of viewing
    // the projected image is in progress.


    // RebuildObjectBuffersWithoutMirror();

    // Make sure we have a current render target
    InitRenderTextureForViewImage();
    // create _Target, _converge, _ProjectedImage renderTexture   (only once)


    // determine the kernel for viewing image on the scene          

    if (_panoramaMeshBuffer != null) {
      mKernelToUse = kernelViewImageOnPanoramaScreen;
      Debug.Log("   kernelViewImageOnPanoramaScreen kernel is executed");

    }
    else {
      Debug.LogError("A panorama  mesh should be defined");
      StopPlay();
    }

    // The disptached kernel mKernelToUse will do different things according to the value
    // of _CaptureOrProjectOrView,
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("_CaptureOrProjectOrView = " + CaptureOrProjectOrView);
    Debug.Log("888888888888888888888888888888888888888888");
    Debug.Log("888888888888888888888888888888888888888888");

    RayTracerShader.SetInt("_CaptureOrProjectOrView", CaptureOrProjectOrView);

    // CameraMain should be enabled all the time
    //if (_cameraMain != null)
    //{
    //    _cameraMain.enabled = false;
    //    Debug.Log("CameraMain will be deactivated");
    //}


    if (UserCamera != null) {

      //MyIO.DebugLogMatrix(_cameraMain.worldToCameraMatrix);
      // "forward" in OpenGL is "-z".In Unity forward is "+z".Most hand - rules you might know from math are inverted in Unity
      //    .For example the cross product usually uses the right hand rule c = a x b where a is thumb, b is index finger and c is the middle
      //    finger.In Unity you would use the same logic, but with the left hand.

      //    However this does not affect the projection matrix as Unity uses the OpenGL convention for the projection matrix.
      //    The required z - flipping is done by the cameras worldToCameraMatrix.
      //    So the projection matrix should look the same as in OpenGL.


      RayTracerShader.SetMatrix("_Projection", UserCamera.projectionMatrix);

      RayTracerShader.SetMatrix("_CameraInverseProjection", UserCamera.projectionMatrix.inverse);

      RayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamera.fieldOfView);


    }
    else {
      Debug.LogError("CameraUser should be defined");

      StopPlay();
    }


    var l = DirectionalLight.transform.forward;
    RayTracerShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));


    RayTracerShader.SetInt("_MaxBounce", MaxNumOfBounce);
    // RayTracingShader.SetInt("_MirrorType", _mirrorType);  // _mirrorType should be set
    // in the inspector of this script component which is attached to the camera gameObject

    //SetComputeBuffer("_Spheres", _sphereBuffer);   commented out by Moon Jung

    RayTracerShader.SetBuffer(mKernelToUse, "_Vertices", _vertexBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_Indices", _indexBuffer);
    RayTracerShader.SetBuffer(mKernelToUse, "_UVs", _texcoordsBuffer);

    RayTracerShader.SetBuffer(mKernelToUse, "_VertexBufferRW", _vertexBufferRW);




    // use the result of the rendering (raytracing shader)
    // _ProjectedImage = _convergedForProjectImage;

    if (ProjectedResultImage == null) {
      Debug.LogError("_ProjectedImage [RenderTexture] should be created by Project Predistorted image");

      StopPlay();
    }
    else {
      RayTracerShader.SetTexture(mKernelToUse, "_ProjectedImage", ProjectedResultImage);
    }

    //_Target.DiscardContents();
    //So what do DiscardContents do:
    //1.it marks appropriate render buffers(there are bools in there) 
    //              to be forcibly cleared next time you activate the RenderTexture
    // 2.IF you call it on active RT it will discard when you will set another RT as active.

    // Clear the target render Texture _Target

    ClearRenderTexture(TargetRenderTex);

    RayTracerShader.SetTexture(mKernelToUse, "_Result", TargetRenderTex);


    #region debugging

    SetBuffersToShaderForDebugging();


    #endregion


  }   //InitViewPanoramaScene()

  //Event Handler for "ViewPanoramaScene" Button
  public void SaveImage() {  //activate the InputField gameObject => The inputField will be prompted for the
                             // user to enter text

    bStopRender = true;   // pause new rendering while saving image;
                            // The current renderTexture _converge will be passed to the framebuffer

    // it is false  when a renderihng task is selected by the user

    //mInputFieldObj.SetActive(true);

    //mInputFieldObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,0.5f);
    //mInputFieldObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0.5f)
    // mInputFieldObj.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

    // the position of the pivot of the rectform relative to its anchors (the center of the canvas)

    //mInputFieldObj.GetComponent<RectTransform>().anchoredPosition 

    //                                 = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);


    // InputField Input Caret is automatically added in front of placeHolder
    // so that placeHolder becomes the second child of InputFieldObj
    //GameObject placeHolder = mInputFieldObj.transform.GetChild(1).gameObject;
    mPlaceHolder.SetActive(true);

    CaptureScreenToFileName(mInputField.textComponent.text);
    //When the user enters text,    CaptureScreenToFileName(mInputField.textComponent.text)
    // will be called and the current renderTexture will be saved to the filepath entered


  }   //SaveImage()

}  //RayTracingMaster
