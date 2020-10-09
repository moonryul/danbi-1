using System.Collections.Generic;
using System.Linq;

using UnityEngine;

// using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi
{
    public sealed class DanbiPrewarperSetting : MonoBehaviour
    {
        [SerializeField]
        EDanbiPrewarperSetting_MeshType MeshType;

        [SerializeField]
        EDanbiPrewarperSetting_PanoramaType PanoramaType;
        DanbiBaseShape Reflector;
        DanbiBaseShape Panorama;

        public DanbiCameraInternalData camAdditionalData { get; set; }

        public delegate void OnMeshRebuild(DanbiComputeShaderControl control);
        public OnMeshRebuild Call_OnMeshRebuild;

        void Awake()
        {
            // PlayerPrefs.DeleteAll();
            Call_OnMeshRebuild += Caller_OnMeshRebuild;
            DanbiComputeShaderControl.Call_OnShaderParamsUpdated += Caller_OnShaderParamsUpdated;

            #region Assign resources
            // 1. Assign automatically the reflector and the Panorama screen.
            foreach (var it in GetComponentsInChildren<DanbiBaseShape>())
            {
                if (!(it is DanbiBaseShape))
                    continue;

                if (it.name.Contains("Reflector"))
                {
                    Reflector = it;
                }

                if (it.name.Contains("Panorama"))
                {
                    Panorama = it;
                }
            }
            #endregion Assign resources      
        }

        void OnDisable()
        {
            Call_OnMeshRebuild -= Caller_OnMeshRebuild;
            DanbiComputeShaderControl.Call_OnShaderParamsUpdated -= Caller_OnShaderParamsUpdated;
        }

        void Caller_OnMeshRebuild(DanbiComputeShaderControl control)
        {
            control.buffersDic.Clear();
            var meshData = new DanbiMeshData()
            {
                Vertices = new List<Vector3>(),
                Indices = new List<int>(),
                Texcoords = new List<Vector2>()
            };

            DanbiBaseShapeData reflectorShapeData = null, panoramaShapeData = null;

            // 2. fill out the meshData for mesh geometries and the additionalData for Shader.
            Reflector.Call_OnMeshRebuild?.Invoke(ref meshData,
                                                 out var reflectorOpticalData,
                                                 out reflectorShapeData);
            Panorama.Call_OnMeshRebuild?.Invoke(ref meshData,
                                                out var PanoramaOpticalData,
                                                out panoramaShapeData);

            // 3. Find Kernel and set it as a current kernel.
            DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.CalcCurrentKernelIndex(MeshType, PanoramaType);

            // 4. Populate the compuate buffer dictionary.
            // 1. vertex
            control.buffersDic.Add("_Vertices",
                DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector3>(meshData.Vertices, 12));
            // 2. index
            control.buffersDic.Add("_Indices",
                DanbiComputeShaderHelper.CreateComputeBuffer_Ret<int>(meshData.Indices, 4));
            // 3. uv
            control.buffersDic.Add("_Texcoords",
                DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector2>(meshData.Texcoords, 8));

            // 4. reflector additional data
            switch (MeshType)
            {
                case EDanbiPrewarperSetting_MeshType.Custom_Cone:
                    // control.buffersDic.Add("_", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiConeData_struct>((reflectorShapeData as DanbiConeData).asStruct, 1));
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
                    // control.buffersDic.Add("_", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiCylinderData_struct>((reflectorShapeData as DanbiCylinderData).asStruct, 1));
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Halfsphere:
                    var halfsphereData = reflectorShapeData as DanbiHalfsphereData;
                    control.buffersDic.Add("_HalfsphereData",
                        DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiHalfsphereData_struct>(halfsphereData.asStruct, halfsphereData.stride));
                    break;
            }

            // 5. panorama additional data            
            var panoramaData = panoramaShapeData as DanbiPanoramaData;
            control.buffersDic.Add("_PanoramaData",
                DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiPanoramaData_struct>(panoramaData.asStruct, panoramaData.stride));

            // 6. Camera External Data.
            // TODO: change the name !
            control.buffersDic.Add("_CamerExternalData",
                DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiCameraInternalData_struct>(camAdditionalData.asStruct, camAdditionalData.stride));
        }

        void Caller_OnShaderParamsUpdated()
        {

        }
    };
};
