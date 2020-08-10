using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Rendering;

using UnityEngine;

using ComputeBuffersDic = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi {
  #region Fields and Main Behaviours
  [System.Serializable]
  public partial class DanbiComputeShaderControl : MonoBehaviour {
    #region Exposed    
    [SerializeField, Header("Ray-Tracer Compute Shader")]
    ComputeShader RTShader;

    public ComputeShader rtShader => RTShader;

    [SerializeField, Header("2 by default for the best performance")]
    int MaxNumOfBounce = 2;

    public int maxNumOfBounce { get => MaxNumOfBounce; }

    [SerializeField]
    uint SamplingThreshold = 30u;

    [SerializeField, Readonly]
    uint SamplingCounter;

    #endregion Exposed

    #region Internal

    Material AddMaterial_ScreenSampling;

    //public Material addMaterial_ScreenSampling { get => AddMaterial_ScreenSampling; set => AddMaterial_ScreenSampling = value; }

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

    public void MakePredistortedImage(Texture2D target, (int, int) screenResolutions) {
      // 01. Prepare RenderTextures.
      PrepareRenderTextures(screenResolutions);
      // 02. Prepare the current kernel for connecting Compute Shader.
      int currentKernel = DanbiKernelHelper.CurrentKernelIndex;
      // Set DanbiOpticalData, DanbiShapeTransform into the compute shader.
      RTShader.SetBuffer(currentKernel, "_PrewarperSetting", BuffersDic["PrewarperSetting"]);
      // 03. Prepare the translation matrices.
      // 04. Set buffers.

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

  #endregion Fields and Main Behaviours

  #region Rest Behaviours

  public partial class DanbiComputeShaderControl {
    //#region PrewarperSettings
    //public static void RegisterNewPrewarperSetting(DanbiPrewarperSetting newSetting, string name = "") {
    //  SettingsDic.Add(name ?? newSetting.kernalName, newSetting);
    //}

    //public static void UnregisterPrewarperSet(string name) {
    //  SettingsDic.Remove(name);
    //}

    //public static void UnregisterAllPrewarperSets() {
    //  SettingsDic.Clear();
    //}
    //#endregion PrewarperSettings

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

    void PrepareMeshesAsComputeBuffer() {
      RebuildPrerequisites();
      //BuffersDic.Add("CamParam", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiCameraInternalParameters>(SettingsDic["CamParam"].camParams, 40));
    }

    public void RebuildPrerequisites() {
      DanbiPrewarperSetting.Call_OnMeshRebuild?.Invoke(this);

      //POD_Data.ClearMeshData();
      //foreach (var it in SettingsDic) {
      //  var meshData = it.Value.reflector.meshData;
      //  int prevVtxCount = POD_Data.vertices.Count;
      //  POD_Data.vertices.AddRange(meshData.Vertices);
      //  POD_Data.texcoords.AddRange(meshData.Texcoords);
      //
      //  int prevIndexCount = POD_Data.indices.Count;
      //
      //  POD_Data.indices.AddRange(meshData.Indices.Select(idx => idx + prevVtxCount));
      //
      //  var rsrcList = new List<(DanbiOpticalData, DanbiShapeTransform)>();
      //  rsrcList.Add((it.Value.reflector.opticalData, (it.Value.reflector as DanbiCustomShape).shapeTransform));
      //  int stride = 40 + 80; // bit size of OpticalData and of CustomShape.
      //  BuffersDic.Add("PrewarperSetting", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<(DanbiOpticalData, DanbiShapeTransform)>(rsrcList, stride));
      //  DanbiKernelHelper.AddKernalIndexWithKey(it.Value.kernalName, RTShader.FindKernel("/*TODO*/"));
      //  DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.GetKernalIndex(it.Value.kernalName);
      //}
      //BuffersDic.Add("Vertices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector3>(POD_Data.vertices, 12));
      //BuffersDic.Add("Indices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<int>(POD_Data.indices, 4));
      //BuffersDic.Add("Texcoords", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector2>(POD_Data.texcoords, 8));
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