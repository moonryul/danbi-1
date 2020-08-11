using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi {
  public sealed class DanbiPrewarperSetting : MonoBehaviour {
    [SerializeField]
    EDanbiPrewarperSetting_MeshType MeshType;

    [SerializeField]
    EDanbiPrewarperSetting_PanoramaType PanoramaType;

    public int stride => CalcStride();

    [SerializeField]
    DanbiBaseShape Reflector;
    public DanbiBaseShape reflector => Reflector;

    [SerializeField]
    DanbiBaseShape Panorama;
    public DanbiBaseShape panorama => Panorama;

    [SerializeField]
    DanbiCamAdditionalData CamAdditionalData;

    public DanbiCamAdditionalData camAdditionalData => CamAdditionalData;

    [SerializeField]
    string KernalName;
    public string kernalName { get => KernalName; set => KernalName = value; }

    public delegate void OnMeshRebuild(DanbiComputeShaderControl control);
    public static OnMeshRebuild Call_OnMeshRebuild;    

    void Start() {
      Call_OnMeshRebuild += Caller_OnMeshRebuild;
      DanbiComputeShaderControl.Call_OnShaderParamsUpdated += Caller_OnShaderParamsUpdated;

      if (!Reflector.Null()) {
        return;
      }

      #region Assign resources      
      // 1. Assign automatically the reflector and the panorama screen.
      foreach (var it in GetComponentsInChildren<DanbiBaseShape>()) {
        if (!(it is DanbiBaseShape))
          continue;

        if (it.name.Contains("Reflector")) {
          Reflector = it;
        }

        if (it.name.Contains("Panorama")) {
          Panorama = it;
        }
      }

      if (Reflector.Null()) {
        Debug.LogError($"Reflector isn't assigned yet!", this);
      }

      if (Panorama.Null()) {
        Debug.LogError($"Panorama isn't assigned yet!", this);
      }
      #endregion Assign resources

      DanbiComputeShaderControl.CamAdditionalData = CamAdditionalData;

    }

    void OnDisable() {
      Call_OnMeshRebuild -= Caller_OnMeshRebuild;
      DanbiComputeShaderControl.Call_OnShaderParamsUpdated -= Caller_OnShaderParamsUpdated;
    }

    void Caller_OnMeshRebuild(DanbiComputeShaderControl control) {
      // 1. Clear every data before rebuilt every meshes into the POD_meshdata.
      control.POD_Data.ClearMeshData();
      var rsrcList = new List<AdditionalData>();
      var data = control.POD_Data;
      var additionalData = default(AdditionalData);

      // 2. fill out the POD_Data for mesh geometries and the additionalData for Shader.
      reflector.Call_OnMeshRebuild?.Invoke(ref data, out additionalData);
      rsrcList.Add(additionalData);

      panorama.Call_OnMeshRebuild?.Invoke(ref data, out additionalData);
      rsrcList.Add(additionalData);

      // 3. Find Kernel and set it as a current kernel.
      DanbiKernelHelper.AddKernalIndexWithKey(kernalName, control.rtShader.FindKernel("/*TODO*/"));
      DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.GetKernalIndex(kernalName);

      // 4. Create new ComputeBuffer.
      control.BuffersDic.Add("_Vertices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector3>(data.vertices, 12));
      control.BuffersDic.Add("_Indices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<int>(data.indices, 4));
      control.BuffersDic.Add("_Texcoords", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector2>(data.texcoords, 8));
      control.BuffersDic.Add("_MeshAdditionalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<AdditionalData>(rsrcList, stride));
      control.BuffersDic.Add("_CamAdditionalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiCamAdditionalData>(CamAdditionalData, 40));
    }

    void Caller_OnShaderParamsUpdated() {

    }

    int CalcStride() {
      int res = 0;
      // 1. Create Shape MeshAdditionalData by MeshType.
      switch (MeshType) {
        case EDanbiPrewarperSetting_MeshType.Custom_Cone:
          break;

        case EDanbiPrewarperSetting_MeshType.Custom_Cube:
          break;

        case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
          break;

        case EDanbiPrewarperSetting_MeshType.Custom_Hemisphere:
          break;

        case EDanbiPrewarperSetting_MeshType.Custom_Pyramid:
          break;

        case EDanbiPrewarperSetting_MeshType.Procedural_Cylinder:
          break;

        case EDanbiPrewarperSetting_MeshType.Procedural_Hemisphere:
          break;
      }

      // 2. Create Panorama MeshAdditionalData by PanoramaType.
      switch (PanoramaType) {
        case EDanbiPrewarperSetting_PanoramaType.Cube_panorama:
          break;

        case EDanbiPrewarperSetting_PanoramaType.Cylinder_panorama:
          break;
      }

      // 3. Add DanbiMeshData.
      res += reflector.meshData.stride;
      res += panorama.meshData.stride;

      // 4. Add DanbiOpticalData.
      res += reflector.opticalData.stride;
      res += panorama.opticalData.stride;

      // 5. Add DanbiShapeTransform.
      res += reflector.shapeTransform.stride;
      res += panorama.shapeTransform.stride;

      // 7. Add DanbiCameraInternalParameters.
      res += CamAdditionalData.stride;
      return res;
    }
    
  };
};
