using UnityEngine;
using UnityEngine.Assertions;

public class RTprojectorMaster : MonoBehaviour {
  [SerializeField, Range(10, 100)]
  float FOV = 23.4f;

  [Header("Bounced amount of Ray Tracing. (default = 2)"), Space(2)]
  [Range(0, 8), Header("  -Ray Tracer Parameter-"), SerializeField, Space(5)]
  int MaxBounceCount = 2;

  [Range(10, 10000), Header("Max resample count for anti-aliasing (default = 1000)"), SerializeField, Space(2)]
  int MaxResampleCount = 1000;

  [Header("Ray Tracing Compute Shader."), Space(2)]
  [Header("  -Required Resources-"), Space(10), SerializeField]
  ComputeShader RayTracerShader;

  [Header("Projector Texture."), SerializeField, Space(2)]
  Texture2D ProjectorTexture;

  [Header("Display Camera."), SerializeField, Space(2)]
  Camera DisplayCamera;

  #region Private Variables.
  /// <summary>
  /// Result Image of Ray tracing is stored into here.
  /// </summary>
  RenderTexture ResultRenderTexture;
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
  uint CurrentSampleCount;
  /// <summary>
  /// Resampler Material (Hidden/AddShader).
  /// </summary>
  Material ResampleAddMat;
  /// <summary>
  /// ComputeShader Helper.
  /// </summary>
  RTcomputeShaderHelper computeShaderHelper;
  #endregion

  void Start() {
    // Get the reference of the main camera.
    MainCamRef = GetComponent<Camera>();
    // Create the resample shader with material.
    ResampleAddMat = new Material(Shader.Find("Hidden/AddShader"));
    // Check whether the resample shader is null or not.
    Assert.IsNotNull(ResampleAddMat, "Resample Shader cannot be null!");
    // Check whether the ray tracer is null or not.
    Assert.IsNotNull(RayTracerShader, "Ray Tracing Shader cannot be null!");
    // Retrieve the kernel index of the ray tracer.
    RTshaderKernelIndex = RayTracerShader.FindKernel("CSMain");
    
    computeShaderHelper = GetComponent<RTcomputeShaderHelper>();
    // Check whether the display camera is null or not.
    Assert.IsNotNull(DisplayCamera, "Display Camera cannot be null!");
  }

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
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target.
    RefreshRenderTarget();
    // Set the target and dispatch the compute shader.
    RayTracerShader.SetTexture(RTshaderKernelIndex, "_Result", ResultRenderTexture);

    //MOON: define the number of thread groups in X and Y directions.
    // The  Screen.width/ 8.0f means that there  are  Screen.width/ 8.0f thread groups in
    // X direction where each thread group has 8 threads ==> Each thread corresponds to a single
    // pixel; Each thread computes the color of a single pixel

    int threadGroupsX = Mathf.CeilToInt(Screen.width * 0.125f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height * 0.125f);
    RayTracerShader.Dispatch(RTshaderKernelIndex, threadGroupsX, threadGroupsY, 1);

    ResampleAddMat.SetFloat("_SampleCount", CurrentSampleCount);

    // Blit the result texture to the screen.
    Graphics.Blit(ResultRenderTexture, destination, ResampleAddMat);

    // Increase the sample count up to the resample count.
    if (CurrentSampleCount < MaxResampleCount) {
      ++CurrentSampleCount;
    }
  }

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
    // Set the projector texture.
    RayTracerShader.SetTexture(RTshaderKernelIndex, "_ProjectorTexture", ProjectorTexture);
    // Set the Camera to the World matrix.
    RayTracerShader.SetMatrix("_CameraToWorldSpace", MainCamRef.cameraToWorldMatrix);
    // Set the inversed projection matrix.
    RayTracerShader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);

    //RayTracerShader.SetMatrix("_DisplayCameraTRS", DisplayCamera.cameraToWorldMatrix);

    RayTracerShader.SetFloat("_FOV", Mathf.Deg2Rad * MainCamRef.fieldOfView);
    // Set the light attributes.
    //var light_dir = DirLight.transform.forward;
    //RTshader.SetVector("_DirectionalLight",
    //                   new Vector4(light_dir.x, light_dir.y, light_dir.z, DirLight.intensity));

    // Set the mesh objects attributes compute buffers.
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_MeshObjects", computeShaderHelper.MeshObjectsAttrsComputeBuf);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Vertices", computeShaderHelper.VerticesComputeBuf);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Indices", computeShaderHelper.IndicesComputeBuf);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_UVs", computeShaderHelper.UVsComputeBuf);

    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_ProjMeshObjects", computeShaderHelper.ProjMeshObjectComputeBuf);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_ProjVertices", computeShaderHelper.ProjVerticesComputeBuf);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_ProjIndices", computeShaderHelper.ProjIndicesComputeBuf);
    RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_ProjUVs", computeShaderHelper.ProjUVsComputeBuf);

    #region Check the projected uv has correct values

    //computeShaderHelper.ProjVerticesComputeBuf.GetData(computeShaderHelper.Dbg_ProjVertices);
    //var res1 = computeShaderHelper.Dbg_ProjVertices;
    //for (int i = 0; i < res1.Length; ++i) {
    //  Debug.Log($"{res1[i]}");
    //}

    //computeShaderHelper.ProjIndicesComputeBuf.GetData(computeShaderHelper.Dbg_ProjIndices);
    //var res1 = computeShaderHelper.Dbg_ProjIndices;
    //for (int i = 0; i < res1.Length; ++i) {
    //  Debug.Log($"{res1[i]}");
    //}

    //computeShaderHelper.ProjUVsComputeBuf.GetData(computeShaderHelper.Dbg_ProjUVs);
    //var res1 = computeShaderHelper.Dbg_ProjUVs;
    //for (int i = 0; i < res1.Length; ++i) {
    //  Debug.Log($"{res1[i]}");
    //}

    //RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Dbg_QuadUVs", computeShaderHelper.Dbg_ProjectorUVBuf);
    //computeShaderHelper.Dbg_ProjectorUVBuf.GetData(computeShaderHelper.Dbg_ProjectorUVResult);
    //var res = computeShaderHelper.Dbg_ProjectorUVResult;
    //for (int i = 0; i < res.Length; ++i) {
    //  Debug.Log($"{res[i]}");
    //}

    //RTcomputeShaderHelper.SetComputeBuffer(ref RayTracerShader, "_Dbg_QuadUVs", computeShaderHelper.Dbg_ProjectorUVBuf);
    //computeShaderHelper.Dbg_ProjectorUVBuf.GetData(computeShaderHelper.Dbg_ProjectorUVResult);
    //var res = computeShaderHelper.Dbg_ProjectorUVResult;
    //for (int i = 0; i < res.Length; ++i) {
    //  Debug.Log($"{res[i]}");
    //}
    #endregion
  }

  /// <summary>
  /// Set Ray Tracing Shader Parameters at runtime (called in Update)
  /// </summary>
  void SetShaderParamsAtRuntime() {
    // Set the Time parameter for moving the objects in Ray Tracer.
    //RayTracerShader.SetVector("_Time", new Vector4(Time.time * 10, Time.time * 20, Time.time * 50, Time.time * 100));
  }

  /// <summary>
  /// It's called when if some mesh objects is target to be added into the Ray Tracer.  
  /// </summary>
  void RebuildMeshObjects() {
    computeShaderHelper.RebuildMeshObjects();
    computeShaderHelper.RebuildMeshProjectorQuad();
  }

  void SynchronizeDisplayCamera() {
    
  }
};
