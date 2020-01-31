using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Ray Tracer masters that controls and assemble everything of the ray tracer.
/// </summary>
[RequireComponent(typeof(Camera))]
public class RTmaster : MonoBehaviour {
  public static int resultID = 0;

  #region Exposed Variables.
  //[SerializeField, Range(10, 100)]
  //float FOV = 23.3f;

  [Header("Bounced amount of Ray Tracing. (default = 2)"), Space(2)]
  [Range(0, 8), Header("  -Ray Tracer Parameter-"), SerializeField, Space(5)]
  int MaxBounceCount = 2;

  [Range(10, 10000), Header("Max resample count for anti-aliasing (default = 1000)"), SerializeField, Space(2)]
  int MaxResampleCount = 1000;

  [Header("Ray Tracing Compute Shader."), Space(2)]
  [Header("  -Required Resources-"), Space(10), SerializeField]
  ComputeShader RayTracerShader;

  [Header("Result Image of Ray tracing is stored into here."), Space(2)]
  public RenderTexture ResultRenderTexture;

  [Header("Skybox Texture for testing."), SerializeField, Space(2)]
  Texture SkyboxTexture;

  [Header("Prewarping Texture."), SerializeField, Space(2)]
  Texture2D PrewarpingTexture;
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
  /// <summary>
  /// Current Sample Count for the optimized resampler of pixel edges.
  /// </summary>
  uint CurrentSampleCount;
  /// <summary>
  /// Resampler Material (Hidden/AddShader).
  /// </summary>
  Material ResampleAddMat;
  /// <summary>
  /// ComputeShader Helper.
  /// </summary>
  RTcomputeShaderHelper computeShaderHelper;
  //RTRayDirectionValidator rayValidator;

  #region 
  /// <summary>
  /// 
  /// </summary>
  //[Header("Sphere Locator."), Space(5)]
  //public RTsphereLocator sphereLocator;
  /// <summary>
  /// 
  /// </summary>
  //RTdbg dbg;
  // 
  //public Texture2D Dbg_DirectionTexture1Result;
  //public Texture2D Dbg_DirectionTexture2Result;
  #endregion

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
    //rayValidator = GetComponent<RTRayDirectionValidator>();
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

    //if (MainCamRef.fieldOfView != FOV) {
    //  MainCamRef.fieldOfView = FOV;
    //}

    SetShaderParamsAtRuntime();
  }

  /// <summary>
  /// It's called when if some mesh objects is target to be added into the Ray Tracer.  
  /// </summary>
  void RebuildMeshObjects() {
    computeShaderHelper.RebuildMeshObjects();
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target.
    RefreshRenderTarget();
    // Set the target and dispatch the compute shader.
    RayTracerShader.SetTexture(RTshaderKernelIndex, "_Result", ResultRenderTexture);

    #region
    // Failed code.
    // Render Texture 는 사용에 무리가있음.    
    //RefreshResultTex(ref Dbg_DirectionTexture1Result);
    //RayTracerShader.SetTexture(RTshaderKernelIndex, "_DirTex1", Dbg_DirectionTexture1Result);
    //
    //RefreshResultTex(ref Dbg_DirectionTexture2Result);
    //RayTracerShader.SetTexture(RTshaderKernelIndex, "_DirTex2", Dbg_DirectionTexture2Result);
    // TODO: Check the ratio of Screen.Width and Screen.Height is 16 by 9. 
    //
    //MOON: define the number of thread groups in X and Y directions.
    // The  Screen.width/ 8.0f means that there  are  Screen.width/ 8.0f thread groups in
    // X direction where each thread group has 8 threads ==> Each thread corresponds to a single
    // pixel; Each thread computes the color of a single pixel
    #endregion

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

  void RefreshResultTex(ref RenderTexture rt) {
    if (rt.Null() || rt.width != Screen.width || rt.height != Screen.height) {
      // Release render texture if we already have one
      if (!rt.Null()) {
        rt.Release();
        rt = null;
      }

      // Get a render target for Ray Tracing
      rt = new RenderTexture(Screen.width, Screen.height, 0,
          RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      rt.enableRandomWrite = true;
      rt.Create();
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
    // Set the room texture.
    RayTracerShader.SetTexture(RTshaderKernelIndex, "_PrewarpingTexture", PrewarpingTexture);
    // Set the Camera to the World matrix.
    RayTracerShader.SetMatrix("_CameraToWorldSpace", MainCamRef.cameraToWorldMatrix);
    // Set the inversed projection matrix.
    RayTracerShader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);

    //RayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamRef.fieldOfView);
    // Set the light attributes.


    //RayTracerShader.SetTexture(RTshaderKernelIndex, "_SkyboxTexture", SkyboxTexture);
    // Set the sphere attributes compute buffers.                  
    // RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_Spheres", sphereLocator.SpheresComputeBuf);

    // Set the mesh objects attributes compute buffers.
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_MeshObjects", computeShaderHelper.MeshObjectsAttrsComputeBuf);

    // if there's vertices, set the vertices and the indices compute buffers.
    if (computeShaderHelper.VerticesList.Count > 0) {
      RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Vertices", computeShaderHelper.VerticesComputeBuf);
      RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Indices", computeShaderHelper.IndicesComputeBuf);
      RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_UVs", computeShaderHelper.UVsComputeBuf);
    }

    // if there's vertex color applied, set the vertex color compute buffers.
    //if (computeShaderHelper.VtxColorsList.Count > 0) {
    //  RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_VertexColors", computeShaderHelper.VtxColorsComputeBuf);
    //}

    // if there's texture color applied, set the texture color compute buffers.
    //if (computeShaderHelper.UVsList.Count > 0) {
    //  RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_TextureColors", computeShaderHelper.TextureColorsComputeBuf);
    //}    

    #region 
    // 광선 투사시에 -z, +z 가 같은 값인지 확인 (방향이 제대로 진행되나)
    //RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_DirTex1", computeShaderHelper.Dbg_DirectionTexture1Buf);
    //computeShaderHelper.Dbg_DirectionTexture1Buf.GetData(computeShaderHelper.Dbg_DirectionTexture1Result);
    //
    //RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_DirTex2", computeShaderHelper.Dbg_DirectionTexture2Buf);
    //computeShaderHelper.Dbg_DirectionTexture2Buf.GetData(computeShaderHelper.Dbg_DirectionTexture2Result);
    //
    //var res1 = computeShaderHelper.Dbg_DirectionTexture1Result;
    //var res2 = computeShaderHelper.Dbg_DirectionTexture2Result;
    //
    //int sameCount = 0, differentCount = 0;
    //
    //for (int y = 0; y < Screen.height; y += 10) {
    //  for (int x = 0; x < Screen.width; x += 10) {
    //    int k = x * Screen.width + y;
    //    //Debug.Log($"Dbg1 (x: {res1[k].x}, y: {res1[k].y}, z: {res1[k].z} || Dbg2 (x: {res2[k].x}, y: {res2[k].y}, z: {res2[k].z})");
    //    if (res1[k] == res2[k]) {
    //      ++sameCount;
    //    }
    //    else {
    //      ++differentCount;
    //    }
    //  }
    //}
    //
    //Debug.Log($"Same Count : {sameCount} || Different Count : {differentCount}");
    #endregion
  }

  /// <summary>
  /// Set Ray Tracing Shader Parameters at runtime (called in Update)
  /// </summary>
  void SetShaderParamsAtRuntime() {
    // Set the Time parameter for moving the objects in Ray Tracer.
    //RayTracerShader.SetVector("_Time", new Vector4(Time.time * 10, Time.time * 20, Time.time * 50, Time.time * 100));
  }
};
