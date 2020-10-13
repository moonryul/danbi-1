using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    public sealed class DanbiPrewarperSetting : MonoBehaviour
    {
        [SerializeField]
        EDanbiPrewarperSetting_MeshType MeshType;

        [SerializeField]
        EDanbiPrewarperSetting_PanoramaType PanoramaType;

        [SerializeField, Readonly]
        int TotalVertexCount;

        [SerializeField, Readonly]
        int TotalIndexCount;

        [SerializeField, Readonly]
        int TotalTexcoordsCount;

        DanbiBaseShape Reflector;
        DanbiBaseShape Panorama;

        public delegate void OnPreparePrerequisites(DanbiComputeShaderControl control);
        public static OnPreparePrerequisites Call_OnPreparePrerequisites;

        void Awake()
        {
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

            // 2. bind the delegates
            Call_OnPreparePrerequisites += Caller_OnPreparePrerequisites;
        }

        void OnDisable()
        {
            // 1. unbind the delegates
            Call_OnPreparePrerequisites -= Caller_OnPreparePrerequisites;
        }

        void Caller_OnPreparePrerequisites(DanbiComputeShaderControl control)
        {
            // 1. clear the buffers Dic before preparing prerequisites.
            control.buffersDic.Clear();
            // prepare the local resources
            var panoramaMeshData = new DanbiMeshData();
            panoramaMeshData.init();

            var reflectorMeshData = new DanbiMeshData();
            reflectorMeshData.init();

            var meshData = new DanbiMeshData();
            meshData.init();

            DanbiBaseShapeData reflectorShapeData = null, panoramaShapeData = null;

            // 2. fill out with the meshData for mesh data and the shape data for Shader.
            Panorama.Call_OnMeshRebuild?.Invoke(ref panoramaMeshData, out panoramaShapeData);
            Reflector.Call_OnMeshRebuild?.Invoke(ref reflectorMeshData, out reflectorShapeData);

            meshData.JoinData(panoramaMeshData, reflectorMeshData);
            // Update the display       
            TotalVertexCount = meshData.Vertices.Count;
            TotalIndexCount = meshData.Indices.Count;
            TotalTexcoordsCount = meshData.Texcoords.Count;

            // 3. Find Kernel and set it as a current kernel.
            int curKernel = DanbiKernelHelper.CalcCurrentKernelIndex(MeshType, PanoramaType);
            // Debug.Log($"Current kernel idx : {curKernel}", this);
            DanbiKernelHelper.CurrentKernelIndex = curKernel;

            // 4. Populate the compuate buffer dictionary.       
            control.buffersDic.Add("_Vertices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(meshData.Vertices, 12));
            control.buffersDic.Add("_Indices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(meshData.Indices, 4));
            control.buffersDic.Add("_Texcoords", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(meshData.Texcoords, 8));

            // 5. reflector mesh shape data
            switch (MeshType)
            {
                // case EDanbiPrewarperSetting_MeshType.Custom_Cone:
                //     // control.buffersDic.Add("_", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiConeData_struct>((reflectorShapeData as DanbiConeData).asStruct, 1));
                //     break;

                // case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
                //     // control.buffersDic.Add("_", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiCylinderData_struct>((reflectorShapeData as DanbiCylinderData).asStruct, 1));
                //     break;

                case EDanbiPrewarperSetting_MeshType.Custom_Dome:
                    var domeData = reflectorShapeData as DanbiDomeData;
                    control.buffersDic.Add("_DomeData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(domeData.asStruct, domeData.stride));
                    break;
            }

            // 6. panorama mesh shape data
            // since panorama shares same shape data, there's no need to make overlaps.
            var panoramaData = panoramaShapeData as DanbiPanoramaData;
            control.buffersDic.Add("_PanoramaData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(panoramaData.asStruct, panoramaData.stride));
        }
    };
};
