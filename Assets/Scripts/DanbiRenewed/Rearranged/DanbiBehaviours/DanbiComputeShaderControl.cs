using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Rendering;

using UnityEngine;

using ComputeBuffersDic = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi {
  [System.Serializable]
  public class DanbiComputeShaderControl : MonoBehaviour {
    #region Exposed    

    [SerializeField, Header("Ray-Tracer Compute Shader")]
    ComputeShader RTShader;

    [SerializeField, Header("2 by default for the best performance")]
    int MaxNumOfBounce = 2;

    [SerializeField]
    uint SamplingThreshold = 30u;

    [SerializeField]
    bool bUseProjectionFromCamCalibration = false;

    #endregion Exposed

    #region Internal    

    Material AddMaterial_ScreenSampling;

    //public Material addMaterial_ScreenSampling { get => AddMaterial_ScreenSampling; set => AddMaterial_ScreenSampling = value; }

    public ComputeShader rtShader => RTShader;
    public int maxNumOfBounce => MaxNumOfBounce;

    //[SerializeField, Readonly]
    uint SamplingCounter;

    /// <summary>
    /// TODO: Need to disassemble into the Tuples.
    /// </summary>

    public struct POD_MeshData {
      public List<Vector3> vertices;
      public List<int> indices;
      public List<Vector2> texcoords;
      public List<int> indices_offsets;
      public List<int> indices_counts;

      public void ClearMeshData() {
        vertices.Clear();
        indices.Clear();
        texcoords.Clear();
      }
    };

    public POD_MeshData POD_Data { get; set; }

    public RenderTexture ResultRT_LowRes { get; set; }

    public RenderTexture ConvergedResultRT_HiRes { get; set; }

    public ComputeBuffersDic BuffersDic { get; } = new ComputeBuffersDic();

    public static DanbiCamAdditionalData CamAdditionalData { get; set; }

    public delegate void OnValueChanged();
    public static OnValueChanged Call_OnValueChanged;

    public delegate void OnShaderParamsUpdated();
    public static OnShaderParamsUpdated Call_OnShaderParamsUpdated;

    #endregion Internal

    void Reset() {
      AddMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
    }

    void Start() {
      Call_OnValueChanged += PrepareMeshesAsComputeBuffer;
      // 1. Retrieve the mesh data as the type of POD_MeshData for transferring into the compute shader
      PrepareMeshesAsComputeBuffer();
    }

    void OnDisable() {
      Call_OnValueChanged -= PrepareMeshesAsComputeBuffer;
    }

    void PrepareMeshesAsComputeBuffer() {
      // RebuildPrerequisites.
      DanbiPrewarperSetting.Call_OnMeshRebuild?.Invoke(this);

    }

    void SetShaderParams() {
      RTShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    }


    void PrepareRenderTextures((int x, int y) screenResolutions) {
      if (ResultRT_LowRes.Null()) {
        ResultRT_LowRes = new RenderTexture(screenResolutions.x, screenResolutions.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        ResultRT_LowRes.enableRandomWrite = true;
        ResultRT_LowRes.Create();
      }

      if (ConvergedResultRT_HiRes.Null()) {
        ConvergedResultRT_HiRes = new RenderTexture(screenResolutions.x, screenResolutions.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        ConvergedResultRT_HiRes.enableRandomWrite = true;
        ConvergedResultRT_HiRes.Create();
      }

      // TODO: Check it revise to reset.
      SamplingCounter = 0;
    }

    void ClearRenderTexture(RenderTexture rt) {
      // To clear the target render texture, we have to set this as a main frame buffer.
      // so we do safe-render-texture-setting.
      var prevRT = RenderTexture.active;
      RenderTexture.active = rt;
      GL.Clear(true, true, Color.clear);
      RenderTexture.active = prevRT;
    }

    public void MakePredistortedImage(Texture2D target, (int x, int y) screenResolutions, Camera MainCamRef) {
      // 01. Prepare RenderTextures.
      PrepareRenderTextures(screenResolutions);

      // 02. Prepare the current kernel for connecting Compute Shader.
      int currentKernel = DanbiKernelHelper.CurrentKernelIndex;
      // Set DanbiOpticalData, DanbiShapeTransform as MeshAdditionalData into the compute shader.
      RTShader.SetBuffer(currentKernel, "_MeshAdditionalData", BuffersDic["_MeshAdditionalData"]);
      RTShader.SetInt("_MaxBounce", MaxNumOfBounce);
      RTShader.SetBuffer(currentKernel, "_Vertices", BuffersDic["_Vertices"]);
      RTShader.SetBuffer(currentKernel, "_Indices", BuffersDic["_Indices"]);
      RTShader.SetBuffer(currentKernel, "_Texcoords", BuffersDic["_Texcoords"]);

      // 03. Prepare the translation matrices.
      // TODO: How you will notice that shader's using simulator mode?
      if (!bUseProjectionFromCamCalibration) {
        RTShader.SetMatrix("_Projection", MainCamRef.projectionMatrix);
        RTShader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);
      } else {
        float left = 0.0f;
        float right = screenResolutions.x;
        float bottom = 0.0f;
        float top = screenResolutions.y;
        float near = MainCamRef.nearClipPlane;
        float far = MainCamRef.farClipPlane;

        var openGL_NDC_KMat = DanbiComputeShaderHelper.GetOpenGL_KMatrix(left, right, bottom, top, near, far);

        var openCV_NDC_KMat = DanbiComputeShaderHelper.GetOpenCV_KMatrix(CamAdditionalData.FocalLength.x, CamAdditionalData.FocalLength.y,
                                                                         CamAdditionalData.PrincipalPoint.x, CamAdditionalData.PrincipalPoint.y,
                                                                         near, far);
        var projMat = openGL_NDC_KMat * openCV_NDC_KMat;
        RTShader.SetMatrix("_Projection", projMat);
        RTShader.SetMatrix("_CameraInverseProjection", projMat.inverse);
        // TODO: Need to decide how we choose the undistort way.

        RTShader.SetBuffer(currentKernel, "_CamAdditionalData", BuffersDic["_CamAdditionalData"]);
        //RTShader.SetVector("_ThresholdIterative", new Vector2())
        //RTShader.SetInt("_IterativeSafeCounter", );
        //RTShader.SetVector("_ThresholdNewTonIterative", );
      }

      RTShader.SetMatrix("_CameraToWorldMat", MainCamRef.cameraToWorldMatrix);
      ClearRenderTexture(ResultRT_LowRes);
      RTShader.SetTexture(currentKernel, "_Result", ResultRT_LowRes);
      RTShader.SetTexture(currentKernel, "_RoomTexture", target);
    }

    public void Dispatch((int x, int y) threadGroups, RenderTexture dest) {
      SetShaderParams();
      // 01. Check the ray tracing shader is valid.
      if (RTShader.Null()) {
        Debug.LogError("Ray-tracing shader is invalid!", this);
      }
      // 02. Dispatch with the current kernel.
      RTShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.x, threadGroups.y, 1);
      // 03. Check Screen Sampler and apply it.      
      AddMaterial_ScreenSampling.SetFloat("_Sample", SamplingCounter);
      // 04. Sample the result into the ConvergedResultRT to improve the aliasing quality.
      Graphics.Blit(ResultRT_LowRes, ConvergedResultRT_HiRes, AddMaterial_ScreenSampling);
      // 05. To improve the resolution of the result RenderTextue, we upscale it in float precision.
      Graphics.Blit(ConvergedResultRT_HiRes, dest);
      // 06. Update the sample counts.
      ++SamplingCounter;
      if (SamplingCounter > SamplingThreshold) {
        DanbiControl.Call_OnRenderFinished?.Invoke();
        SamplingCounter = 0;
      }
    }
  }; // class ending.
}; // namespace Danbi
