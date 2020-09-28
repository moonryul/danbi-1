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

        public DanbiCameraExternalData camAdditionalData { get; set; }

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
            var opticalDataList = new List<DanbiOpticalData>();
            var meshData = new DanbiMeshData()
            {
                Vertices = new List<Vector3>(),
                Indices = new List<int>(),
                Texcoords = new List<Vector2>()
            };

            DanbiBaseShapeData reflectorShapeData = null, panoramaShapeData = null;

            // 2. fill out the meshData for mesh geometries and the additionalData for Shader.
            Reflector.Call_OnMeshRebuild?.Invoke(ref meshData, out var reflectorOpticalData, out reflectorShapeData);
            // opticalDataList.Add(     opticalData);

            Panorama.Call_OnMeshRebuild?.Invoke(ref meshData, out var PanoramaOpticalData, out panoramaShapeData);
            // opticalDataList.Add(opticalData);

            // 3. Find Kernel and set it as a current kernel.
            //DanbiKernelHelper.AddKernalIndexWithKey(KernalName, control.rtShader.FindKernel("/*TODO*/"));
            //DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.GetKernalIndex(KernalName);

            // 4. Create new ComputeBuffer.      
            // 1. vertex
            control.buffersDic.Add("_Vertices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector3>(meshData.Vertices, 12));
            // 2. index
            control.buffersDic.Add("_Indices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<int>(meshData.Indices, 4));
            // 3. uv
            control.buffersDic.Add("_Texcoords", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector2>(meshData.Texcoords, 8));

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
                    control.buffersDic.Add("_HalfsphereData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiHalfsphereData_struct>(halfsphereData.asStruct, halfsphereData.stride));
                    break;
            }
            // 5. panorama additional data
            switch (PanoramaType)
            {
                // TODO: Keep on watching for the stride value.
                case EDanbiPrewarperSetting_PanoramaType.Cube_panorama:
                    var cubePanoramaData = panoramaShapeData as DanbiPanoramaData;
                    control.buffersDic.Add("_PanoramaData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiPanoramaData_struct>(cubePanoramaData.asStruct, cubePanoramaData.stride));
                    break;

                case EDanbiPrewarperSetting_PanoramaType.Cylinder_panorama:
                    var CylinderPanoramaData = panoramaShapeData as DanbiPanoramaData;
                    control.buffersDic.Add("_PanoramaData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiPanoramaData_struct>(CylinderPanoramaData.asStruct, CylinderPanoramaData.stride));
                    break;
            }

            // 5. Set the current kernel            
            DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.CalcCurrentKernelIndex(MeshType, PanoramaType);
        }

        void Caller_OnShaderParamsUpdated()
        {

        }

        // int CalcStride()
        // {
        //     int res = 0;
        //     // 1. Create Shape MeshAdditionalData by MeshType.
        //     switch (MeshType)
        //     {
        //         case EDanbiPrewarperSetting_MeshType.Custom_Cone:
        //             break;

        //         case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
        //             break;

        //         case EDanbiPrewarperSetting_MeshType.Custom_Halfsphere:
        //             break;

        //         case EDanbiPrewarperSetting_MeshType.Custom_Pyramid:
        //             break;

        //         case EDanbiPrewarperSetting_MeshType.Procedural_Cylinder:
        //             break;

        //         case EDanbiPrewarperSetting_MeshType.Procedural_Halfsphere:
        //             break;
        //     }

        //     // 2. Create Panorama MeshAdditionalData by PanoramaType.
        //     switch (PanoramaType)
        //     {
        //         case EDanbiPrewarperSetting_PanoramaType.Cube_panorama:
        //             break;

        //         case EDanbiPrewarperSetting_PanoramaType.Cylinder_panorama:
        //             break;
        //     }

        //     // 4. Add DanbiOpticalData.
        //     // res += Reflector.opticalData.stride;
        //     // res += Panorama.opticalData.stride;

        //     // 5. Add DanbiShapeTransform.
        //     if (Reflector is DanbiCylinder)
        //     {
        //         // res += (Reflector as DanbiCylinder).ShapeTransform.stride;
        //     }

        //     if (Reflector is DanbiCone)
        //     {
        //         // res += (Reflector as DanbiCone).shapeTransform.stride;
        //     }

        //     if (Reflector is DanbiHalfSphere)
        //     {
        //         // res += (Reflector as DanbiHalfSphere).shapeTransform.stride;
        //     }

        //     // res += (Panorama as DanbiCubePanorama).shapeTransform.stride;

        //     // 7. Add DanbiCameraInternalParameters.
        //     // res += camAdditionalData.stride;

        //     return res;
        // }
    };
};
