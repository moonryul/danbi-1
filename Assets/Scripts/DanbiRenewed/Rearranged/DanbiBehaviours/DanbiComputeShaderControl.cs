using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Rendering;

using UnityEngine;


namespace Danbi {

  #region Fields and Main Behaviours
  [System.Serializable]
  public partial class DanbiComputeShaderControl : MonoBehaviour {
    #region Exposed    

    [SerializeField, Header("Ray-Tracer Compute Shader")]
    ComputeShader RTShader;

    public ComputeShader rtShader {
      get {
        return RTShader;
      }
      set {
        RTShader = value;
      }
    }

    [SerializeField, Header("2 by default for the best performance")]
    int MaxNumOfBounce = 2;

    public int maxNumOfBounce {
      get => MaxNumOfBounce;
      set {
        if (value < 0) {
          Debug.Log("MaxNumOfBounce cannot be under 0! it's set to 2 by default");
          MaxNumOfBounce = 2;
        }
        else {
          MaxNumOfBounce = value;
        }
      }
    }

    [Readonly, SerializeField, Space(15)]
    Material AddMaterial_ScreenSampling;

    public Material addMaterial_ScreenSampling { get => AddMaterial_ScreenSampling; set => AddMaterial_ScreenSampling = value; }

    [SerializeField]
    uint SamplingThreshold = 30u;

    [SerializeField, Readonly]
    uint SamplingCounter;

    #endregion Exposed

    #region Internal

    /// <summary>
    /// TODO:
    /// </summary>
    internal struct POD_MeshData {
      public List<Vector3> vertices;
      public List<int> indices;
      public List<Vector2> texcoords;
      public List<int> indices_offsets;
      public List<int> indices_counts;
      public List<Matrix4x4> local2Worlds;
      public List<Vector3> albedos;
      public List<Vector3> speculars;
      public List<Vector3> emissions;
      public List<float> smoothnesses;

      public void ClearMeshData() {
        vertices.Clear();
        indices.Clear();
        texcoords.Clear();
      }
    };

    POD_MeshData MeshDataForComputeBuffers;

    RenderTexture ResultRT_LowRes;
    public RenderTexture resultRT_LowRes { get => ResultRT_LowRes; set => ResultRT_LowRes = value; }

    RenderTexture ConvergedResultRT_HiRes;
    public RenderTexture convergedResultRT_HiRes { get => ConvergedResultRT_HiRes; set => ConvergedResultRT_HiRes = value; }

    static Dictionary<string, DanbiPrewarperSetting> PrewarperSettingDic = new Dictionary<string, DanbiPrewarperSetting>();
    public static Dictionary<string, DanbiPrewarperSetting> prewarperSettingDic { get => PrewarperSettingDic; }

    ComputeBuffer PrewarperSettingBuf;


    static Dictionary<string, ComputeBuffer> ComputeBufDic = new Dictionary<string, ComputeBuffer>();
    public static Dictionary<string, ComputeBuffer> computeBufDic { get => ComputeBufDic; set => ComputeBufDic = value; }

    public delegate void OnValueChanged();
    public static OnValueChanged Call_OnValueChanged;

    #endregion Internal

    void Start() {
      Call_OnValueChanged += PrepareMeshesAsComputeBuffer;
      AddMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
      PerpareResources();
    }

    void PerpareResources() {
      // 1. Retrieve the mesh data as the type of POD_MeshData for transferring into the compute shader
      PrepareMeshesAsComputeBuffer();
    }

    public void MakePredistortedImage(Texture2D target, (int, int) screenResolutions) {
      // 01. Prepare RenderTextures.
      PrepareRenderTextures(screenResolutions);
      // 02. Prepare the current kernel for connecting Compute Shader.
      int currentKernel = DanbiKernelHelper.CurrentKernelIndex;
      RTShader.SetBuffer(currentKernel, "_PrewarperSetting", computeBufDic["PrewarperSetting"]);
      // 03. Prepare the translation matrices.
      // 04. Set buffers.

    }

    public void Dispatch((int, int) threadGroups, RenderTexture dest) {
      SetShaderParams();
      // 01. Check the ray tracing shader is valid.
      if (RTShader.Null()) {
        Debug.LogError("Ray-tracing shader is invalid!", this);
      }
      // 02. Dispatch with the current kernel.
      RTShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.Item1, threadGroups.Item2, 1);
      // 03. Check Screen Sampler and apply it.
      if (AddMaterial_ScreenSampling.Null()) {
        Debug.LogError("ScreenSampling shader is invalid!", this);
      }
      AddMaterial_ScreenSampling.SetFloat("_Sample", SamplingCounter);
      // 04. Sample the result into the ConvergedResultRT to improve the aliasing quality.
      Graphics.Blit(ResultRT_LowRes, ConvergedResultRT_HiRes, AddMaterial_ScreenSampling);
      // 05. To improve the resolution of the result RenderTextue, we upscale it in float precision.
      Graphics.Blit(ConvergedResultRT_HiRes, dest);
      // 06. Update the sample counts.
      ++SamplingCounter;
      if (SamplingCounter > SamplingThreshold) {
        DanbiControl_Internal.Call_RenderFinished.Invoke();
        SamplingCounter = 0;
      }
    }
  }; // class ending.

  #endregion Fields and Main Behaviours

  #region Rest Behaviours

  public partial class DanbiComputeShaderControl {

    public static void RegisterNewPrewarperSetting(DanbiPrewarperSetting newSetting) {
      PrewarperSettingDic.Add(newSetting.shape.getShapeName, newSetting);
    }

    public static void UnregisterPrewarperSet(string name) {
      PrewarperSettingDic.Remove(name);
    }

    public static void UnregisterAllPrewarperSets() {
      PrewarperSettingDic.Clear();
    }

    void PrepareRenderTextures((int, int) screenResolutions) {
      if (ResultRT_LowRes.Null()) {
        ResultRT_LowRes = new RenderTexture(screenResolutions.Item1, screenResolutions.Item2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        ResultRT_LowRes.enableRandomWrite = true;
        ResultRT_LowRes.Create();
      }

      if (ConvergedResultRT_HiRes.Null()) {
        ConvergedResultRT_HiRes = new RenderTexture(screenResolutions.Item1, screenResolutions.Item2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        ConvergedResultRT_HiRes.enableRandomWrite = true;
        ConvergedResultRT_HiRes.Create();
      }

      // TODO: Check it revise to reset.
      SamplingCounter = 0;
    }
    
    void PrepareMeshesAsComputeBuffer() {
      RebuildAll();
      ComputeBufDic.Add("CamParam", DanbiShaderHelper.CreateComputeBuffer_Ret<DanbiCameraInternalParameters>(PrewarperSettingDic["CamParam"].camParams, 40));     
    }

    public void Rebuild(string name) {
      if (string.IsNullOrWhiteSpace(name)) {
        Debug.LogError("name is invalid!", this);
      }

      var fwd = PrewarperSettingDic[name];
      var meshData = fwd.shape.getMeshData;
      int prevVtxCount = MeshDataForComputeBuffers.vertices.Count;
      MeshDataForComputeBuffers.vertices.AddRange(meshData.Vertices);
      MeshDataForComputeBuffers.texcoords.AddRange(meshData.Texcoords);

      int prvIdxCount = MeshDataForComputeBuffers.indices.Count;

      MeshDataForComputeBuffers.indices.AddRange(meshData.Indices.Select(idx => idx + prevVtxCount));



    }

    public void RebuildAll() {
      MeshDataForComputeBuffers.ClearMeshData();
      foreach (var it in PrewarperSettingDic) {
        var meshData = it.Value.shape.getMeshData;
        int prevVtxCount = MeshDataForComputeBuffers.vertices.Count;
        MeshDataForComputeBuffers.vertices.AddRange(meshData.Vertices);
        MeshDataForComputeBuffers.texcoords.AddRange(meshData.Texcoords);

        int prevIndexCount = MeshDataForComputeBuffers.indices.Count;

        MeshDataForComputeBuffers.indices.AddRange(meshData.Indices.Select(idx => idx + prevVtxCount));

        var rsrcList = new List<(DanbiOpticalData, DanbiShapeTransform)>();
        rsrcList.Add((it.Value.shape.getOpticalData, (it.Value.shape as DanbiCustomShape).shapeTransform));
        int stride = 40 + 80; // bit size of OpticalData and of CustomShape.
        ComputeBufDic.Add("PrewarperSetting", DanbiShaderHelper.CreateComputeBuffer_Ret<(DanbiOpticalData, DanbiShapeTransform)>(rsrcList, stride));
        DanbiKernelHelper.AddKernalIndexWithKey(it.Value.kernalName, RTShader.FindKernel("/*TODO*/"));
        DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.GetKernalIndex(it.Value.kernalName);
      }
      ComputeBufDic.Add("Vertices", DanbiShaderHelper.CreateComputeBuffer_Ret<Vector3>(MeshDataForComputeBuffers.vertices, 12));
      ComputeBufDic.Add("Indices", DanbiShaderHelper.CreateComputeBuffer_Ret<int>(MeshDataForComputeBuffers.indices, 4));
      ComputeBufDic.Add("Texcoords", DanbiShaderHelper.CreateComputeBuffer_Ret<Vector2>(MeshDataForComputeBuffers.texcoords, 8));      
    }

    void SetShaderParams() {
      RTShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    }    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rt"></param>
    void ClearRenderTexture(RenderTexture rt) {
      // To clear the target render texture, we have to set this as a main frame buffer.
      // so we do safe-render-texture-setting.
      var prevRT = RenderTexture.active;
      RenderTexture.active = rt;
      GL.Clear(true, true, Color.clear);
      RenderTexture.active = prevRT;
    }
  }; // class ending


  #endregion Rest Behaviours
}; // namespace Danbi