using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
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
            Call_OnPreparePrerequisites += Caller_OnPreparePrerequisites;
        }

        void OnDisable()
        {
            Call_OnPreparePrerequisites -= Caller_OnPreparePrerequisites;
        }

        void Caller_OnPreparePrerequisites(DanbiComputeShaderControl control)
        {
            control.buffersDic.Clear();

            var panoramaMeshData = new DanbiMeshData()
            {
                Vertices = new List<Vector3>(),
                Indices = new List<int>(),
                Texcoords = new List<Vector2>()
            };

            var reflectorMeshData = new DanbiMeshData()
            {
                Vertices = new List<Vector3>(),
                Indices = new List<int>(),
                Texcoords = new List<Vector2>()
            };

            var meshData = new DanbiMeshData()
            {
                Vertices = new List<Vector3>(),
                Indices = new List<int>(),
                Texcoords = new List<Vector2>()
            };

            DanbiBaseShapeData reflectorShapeData = null, panoramaShapeData = null;

            // 2. fill out with the meshData for mesh data and the shape data for Shader.
            Panorama.Call_OnMeshRebuild?.Invoke(ref panoramaMeshData, out panoramaShapeData);
            Reflector.Call_OnMeshRebuild?.Invoke(ref reflectorMeshData, out reflectorShapeData);

            meshData.Vertices.AddRange(panoramaMeshData.Vertices);
            meshData.Vertices.AddRange(reflectorMeshData.Vertices);
            TotalVertexCount = meshData.Vertices.Count;

            meshData.Indices.AddRange(panoramaMeshData.Indices);
            meshData.Indices.AddRange(reflectorMeshData.Indices);
            TotalIndexCount = meshData.Indices.Count;

            meshData.Texcoords.AddRange(panoramaMeshData.Texcoords);
            meshData.Texcoords.AddRange(reflectorMeshData.Texcoords);
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
            var panoramaData = panoramaShapeData as DanbiPanoramaData;
            control.buffersDic.Add("_PanoramaData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(panoramaData.asStruct, panoramaData.stride));
        }
    };
};
