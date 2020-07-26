using System;
using System.Collections.Generic;

using UnityEngine;


namespace Danbi {
  using PrewarperSets = Dictionary<string, DanbiPrewarperSet>;


  #region Fields

  public partial class DanbiShaderControl : MonoBehaviour {
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
        } else {
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

    [SerializeField, Readonly]
    RenderTexture ResultRT;
    public RenderTexture resultRT { get => ResultRT; set => ResultRT = value; }


    [SerializeField, Readonly]
    RenderTexture ConvergedResultRT;
    public RenderTexture convergedResultRT { get => ConvergedResultRT; set => ConvergedResultRT = value; }

    PrewarperSets PrewarperDic = new PrewarperSets();

    internal struct POD_MeshData {
      public List<Vector3> vertices;
      public List<uint> indices;
      public List<Vector2> texcoords;
    };


    public delegate void OnMakePrewarpedImage();
    public static OnMakePrewarpedImage Call_OnMakePrewarpedImage;


    void Start() {
      Call_OnMakePrewarpedImage += Caller_OnMakePrewarpedImage;
      AddMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
    }

    void Caller_OnMakePrewarpedImage() {

    }    

    public void Dispatch((int, int) threadGroups, RenderTexture dest) {
      if (RTShader.Null()) {
        Debug.LogError("Ray-tracing shader is invalid!", this);
      }

      RTShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.Item1, threadGroups.Item2, 1);

      if (AddMaterial_ScreenSampling.Null()) {
        Debug.LogError("ScreenSampling shader is invalid!", this);
      }
      AddMaterial_ScreenSampling.SetFloat("_Sample", SamplingCounter);

      // Sample the result into the ConvergedResultRT to improve the aliasing quality.
      Graphics.Blit(ResultRT, ConvergedResultRT, AddMaterial_ScreenSampling);
      // To improve the resolution of the result RenderTextue, we upscale it in float precision.
      Graphics.Blit(ConvergedResultRT, dest);

      ++SamplingCounter;
      if (SamplingCounter > SamplingThreshold) {
        DanbiControl_Internal.Call_RenderFinished.Invoke();
        SamplingCounter = 0;
      }
    }

  };

  #endregion

  #region Behaviours

  public partial class DanbiShaderControl {
    public void RegisterNewPrewarperSet(string name, DanbiPrewarperSet newSet) {
      PrewarperDic.Add(name, newSet);
    }

    public void UnregisterPrewarperSet(string name) {
      PrewarperDic.Remove(name);
    }

    public void UnregisterAllPrewarperSets() {
      PrewarperDic.Clear();
    }
    public void Rebuild(string name) {
      if (String.IsNullOrWhiteSpace(name)) {
        Debug.LogError("name is invalid!", this);
      }
    }
    public void RebuildAll() {

    }

    void SetShaderParams() {

    }

    void DisposeAll() {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rt"></param>
    void ClearRenderTexture(RenderTexture rt) {
      // To clear the target render texture, we have to set this as a main framebuffer.
      // so we do safe-rendertexture-setting.
      var prevRT = RenderTexture.active;
      RenderTexture.active = rt;
      GL.Clear(true, true, Color.clear);
      RenderTexture.active = prevRT;
    }
  }; // class ending

  #endregion
}; // namespace Danbi