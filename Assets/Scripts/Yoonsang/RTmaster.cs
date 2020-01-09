using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Ray Tracer masters that controls and assemble everything of the ray tracer.
/// </summary>
[RequireComponent(typeof(Camera))]
public class RTmaster : MonoBehaviour {
  #region Exposed Variables.

  [SerializeField, Range(10, 100)] float FOV = 85.0f;

  /// <summary>
  /// 
  /// </summary>
  [SerializeField]
  bool DoesRayTracingInversed = false;

  [Header("Bounced amount of Ray Tracing. (default = 2)"), Space(2)]
  [Range(0, 8), Header("  -Ray Tracer Parameter-"), SerializeField, Space(10)]
  int MaxBounceCount = 2;

  [Range(10, 10000), Header("Max resample count for anti-aliasing (default = 1000)"), SerializeField, Space(2)]
  int MaxResampleCount = 1000;

  [Header("Ray Tracing Compute Shader."), Space(2)]
  [Header("  -Required Resources-"), Space(10), SerializeField]
  ComputeShader RayTracerShader;

  [Header("Result Image of Ray tracing is stored into here."), SerializeField, Space(2)]
  RenderTexture ResultRenderTexture;

  [Header("Skybox Texture for testing."), SerializeField, Space(2)]
  Texture SkyboxTexture;

  [Header("Room Texture."), SerializeField, Space(2)]
  Texture2D RoomTexture;

  [Header("Plain Texture."), SerializeField, Space(2)]
  Texture2D PlainTexture;
  #endregion

  #region Private Variables.
  /// <summary>
  /// Kernel Index of Ray tracing shader.
  /// </summary>
  int RTshaderKernelIndex;

  /// <summary>
  /// Reference to the main camera.
  /// </summary>
  Camera MainCamRef;

  //[Header("Video Material for testing the real time reflection."), Space(5)]
  //public Material VideoMat;

  /// <summary>
  /// Current Sample Count for the optimized resampler of pixel edges.
  /// </summary>
  uint CurrentSampleCount;

  /// <summary>
  /// Resampler Material (Hidden/AddShader).
  /// </summary>
  Material ResampleAddMat;

  //[Header("Light for Ray Tracing Rendering."), Space(5)]
  //public Light DirLight;
  /// <summary>
  /// ComputeShader Helper.
  /// </summary>
  RTcomputeShaderHelper computeShaderHelper;

  /// <summary>
  /// 
  /// </summary>
  //[Header("Sphere Locator."), Space(5)]
  //public RTsphereLocator sphereLocator;
  /// <summary>
  /// 
  /// </summary>
  //RTdbg dbg;
  #endregion

  #region Event Functions
  void Start() {
    MainCamRef = GetComponent<Camera>();
    //sphereLocator.LocateSphereRandomly();   
    ResampleAddMat = new Material(Shader.Find("Hidden/AddShader"));
    Assert.IsNotNull(ResampleAddMat, "Resample Shader cannot be null!");
    Assert.IsNotNull(RayTracerShader, "Ray Tracing Shader cannot be null!");
    RTshaderKernelIndex = RayTracerShader.FindKernel("CSMain");
    computeShaderHelper = GetComponent<RTcomputeShaderHelper>();
  }

  /// <summary>
  /// It's call-backed only by Camera component when the image is really got rendered.
  /// </summary>
  /// <param name="source"></param>
  /// <param name="destination"></param>
  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    // Rebuild the mesh objects if new mesh objects are coming up.    
    RebuildMeshObjects();
    // Set Shader parameters.    
    SetShaderParams();
    // Render it onto the render texture.
    Render(destination);
  }

  void Update() {
    if (transform.hasChanged) {
      CurrentSampleCount = 0;
      transform.hasChanged = false;
    }
    MainCamRef.fieldOfView = FOV;
    SetShaderParamsAtRuntime();
    //RayTracingShader.SetTexture(0, "_SkyboxTexture", _Video.mainTexture);
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target.
    RefreshRenderTarget();
    // Set the target and dispatch the compute shader.
    RayTracerShader.SetTexture(0, "_Result", ResultRenderTexture);
        // TODO: Check the ratio of Screen.Width and Screen.Height is 16 by 9. 

        //MOON: define the number of thread groups in X and Y directions.
        // The  Screen.width/ 8.0f means that there  are  Screen.width/ 8.0f thread groups in
        // X direction where each thread group has 8 threads ==> Each thread corresponds to a single
        // pixel; Each thread computes the color of a single pixel

    int threadGroupsX = Mathf.CeilToInt(Screen.width * 0.125f /*/ 8.0f*/);
    int threadGroupsY = Mathf.CeilToInt(Screen.height * 0.125f /*/ 8.0f*/);
    RayTracerShader.Dispatch(RTshaderKernelIndex, threadGroupsX, threadGroupsY, 1);
    ResampleAddMat.SetFloat("_SampleCount", CurrentSampleCount);
     
   
        // Blit the result texture to the screen.
        Graphics.Blit(ResultRenderTexture, destination, ResampleAddMat);

    // Increase the sample count up to the resample count.
    if (CurrentSampleCount < MaxResampleCount) {
      ++CurrentSampleCount;
    }
  }
  #endregion

  /// <summary>
  /// Render Target for RT must be refreshed on every render due to
  /// keep printing on the result.
  /// </summary>
  void RefreshRenderTarget() {
    if (ResultRenderTexture.Null() || ResultRenderTexture.width != Screen.width || ResultRenderTexture.height != Screen.height) {
      // Release render texture if we already have one
      if (!ResultRenderTexture.Null()) {
        ResultRenderTexture.Release();
        ResultRenderTexture = null;
      }

      // Get a render target for Ray Tracing
      ResultRenderTexture = new RenderTexture(Screen.width, Screen.height, 0,
          RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      ResultRenderTexture.enableRandomWrite = true;
      ResultRenderTexture.Create();
    }
  }

  /// <summary>
  /// Set the Shader parameters into the ray tracer.
  /// </summary>
  void SetShaderParams() {
    // Set the maximum count of ray bounces.
    RayTracerShader.SetInt("_MaxBounceCount", MaxBounceCount);
    // Set the pixel offset for screen uv.
    RayTracerShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    // Set the skybox texture.
    //RayTracerShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
    // Set the room texture.
    RayTracerShader.SetTexture(0, "_RoomTexture", RoomTexture);
    // Set the plain texture.
    RayTracerShader.SetTexture(0, "_PlainTexture", PlainTexture);
    // Set the Camera to the World matrix.
    RayTracerShader.SetMatrix("_CameraToWorld", MainCamRef.cameraToWorldMatrix);
    // Set the inversed projection matrix.
    RayTracerShader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);

    RayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamRef.fieldOfView);
        // Set the light attributes.
        //var light_dir = DirLight.transform.forward;
        //RTshader.SetVector("_DirectionalLight",
        //                   new Vector4(light_dir.x, light_dir.y, light_dir.z, DirLight.intensity));

        // Set the sphere attributes compute buffers.                  
        //RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_Spheres", sphereLocator.SpheresComputeBuf);
        // Set the mesh objects attributes compute buffers.
        RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_MeshObjects", computeShaderHelper.MeshObjectsAttrsComputeBuf);

    // if there's vertices, set the vertices and the indices compute buffers.
    if (computeShaderHelper.VerticesList.Count > 0) {
      RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Vertices", computeShaderHelper.VerticesComputeBuf);
      RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Indices", computeShaderHelper.IndicesComputeBuf);
    }
    // if there's vertex color applied, set the vertex color compute buffers.
    if (computeShaderHelper.VtxColorsList.Count > 0) {
      //RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_VertexColors", computeShaderHelper.VtxColorsComputeBuf);
    }
    // if there's texture color applied, set the texture color compute buffers.
    if (computeShaderHelper.UVsList.Count > 0) {
      //RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_TextureColors", computeShaderHelper.TextureColorsComputeBuf);
    }
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_UVs", computeShaderHelper.UVsComputeBuf);

    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_RVertices", computeShaderHelper.ReflectorVerticesComputeBuffer);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_RIndices", computeShaderHelper.ReflectorIndicesComputeBuffer);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_RUVs", computeShaderHelper.ReflectorUVsComputeBuffer);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_RMeshObjects", computeShaderHelper.ReflectorMeshObjectComputeBuffer);
  }

  /// <summary>
  /// Set Ray Tracing Shader Parameters at runtime (called in Update)
  /// </summary>
  void SetShaderParamsAtRuntime() {
    // Set the Time parameter for moving the objects in Ray Tracer.
    RayTracerShader.SetVector("_Time", new Vector4(Time.time * 10, Time.time * 20, Time.time * 50, Time.time * 100));
  }

  /// <summary>
  /// It's called when if some mesh objects is target to be added into the Ray Tracer.  
  /// </summary>
  void RebuildMeshObjects() {
    computeShaderHelper.RebuildMeshObjects();
  }
};
