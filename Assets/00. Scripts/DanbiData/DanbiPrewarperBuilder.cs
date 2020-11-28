using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    public sealed class DanbiPrewarperBuilder : MonoBehaviour
    {
        [SerializeField, Readonly]
        EDanbiPrewarperSetting_MeshType m_meshType;

        [SerializeField, Readonly]
        EDanbiPrewarperSetting_PanoramaType m_panoramaType;

        DanbiBaseShape m_reflector;
        DanbiBaseShape m_panoramaRegular;
        DanbiBaseShape m_panorama360;
        DanbiBaseShape m_panorama4faces;

        EDanbiTextureType m_texType;

        DanbiMeshesData m_panoramaMeshData = new DanbiMeshesData();
        DanbiMeshesData m_reflectorMeshData = new DanbiMeshesData();
        DanbiMeshesData m_meshData = new DanbiMeshesData();

        DanbiBaseShapeData m_panoramaShapeData = new DanbiBaseShapeData();
        DanbiBaseShapeData m_reflectorShapeData = new DanbiBaseShapeData();

        void Awake()
        {
            // DanbiUISync.onPanelUpdate += this.OnPanelUpdate;
            DanbiUIImageGeneratorTexturePanel.onTextureTypeChange += this.OnTextureTypeChange;

            m_panoramaRegular = transform.GetChild(1).GetComponent<DanbiBaseShape>();
            m_panorama4faces = transform.GetChild(2).GetComponent<DanbiBaseShape>();
            m_panorama360 = transform.GetChild(3).GetComponent<DanbiBaseShape>();
            m_reflector = transform.GetChild(4).GetComponent<DanbiBaseShape>();

            // Panorama 
            DanbiUIPanoramaCubeDimension.onCHChange +=
                (float ch) =>
                {
                    RebuildMeshShape();
                    RebuildMeshInfo();
                };

            DanbiUIPanoramaCubeDimension.onCLChange +=
                (float cl) =>
                {
                    RebuildMeshShape();
                    RebuildMeshInfo();
                };
            
            // Reflector

            DanbiUIReflectorDome.onDistanceUpdate +=
                (float distance) =>
                {
                    // RebuildMesh();
                    RebuildMeshInfo();
                };

            DanbiUIReflectorDome.onHeightUpdate +=
                (float height) =>
                {
                    //RebuildMesh();
                    RebuildMeshInfo();
                };

            DanbiUIReflectorDome.onBottomRadiusUpdate +=
                (float radius) =>
                {
                    //RebuildMesh();
                    RebuildMeshInfo();
                };

            DanbiUIReflectorDome.onMaskingRatioUpdate +=
                (float ratio) =>
                {
                    //RebuildMesh();
                    RebuildMeshInfo();
                };
        }

        void Start()
        {
            m_panorama4faces.gameObject.SetActive(false);
            m_panorama360.gameObject.SetActive(false);

            RebuildMeshShape();
            RebuildMeshInfo();
        }

        public void RebuildMeshShape()
        {
            var control = DanbiManager.instance.shaderControl;

            m_panoramaMeshData.Clear();
            m_reflectorMeshData.Clear();
            m_meshData.Clear();

            //  fill out with the meshData for mesh data and the shape data for Shader.

            switch (m_texType)
            {
                case EDanbiTextureType.Regular:
                    m_panoramaRegular.RebuildMeshShapeForComputeShader(ref m_panoramaMeshData);
                    break;

                case EDanbiTextureType.Faces4:
                    m_panorama4faces.RebuildMeshShapeForComputeShader(ref m_panoramaMeshData);
                    break;

                case EDanbiTextureType.Panorama:
                    m_panorama360.RebuildMeshShapeForComputeShader(ref m_panoramaMeshData);
                    break;
            }
            m_reflector.RebuildMeshShapeForComputeShader(ref m_reflectorMeshData);

            m_meshData.JoinData(m_panoramaMeshData, m_reflectorMeshData);

            // 3. Find Kernel and set it as a current kernel.            
            DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.CalcCurrentKernelIndex(m_meshType, m_panoramaType);

            // 4. Populate the compuate buffer dictionary.       
            var vtxComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_meshData.Vertices, 12);
            control.bufferDict.AddBuffer_NoDuplicate("_Vertices", vtxComputeBuffer);

            var idxComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_meshData.Indices, 4);
            control.bufferDict.AddBuffer_NoDuplicate("_Indices", idxComputeBuffer);

            var texcoordsComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_meshData.Texcoords, 8);
            control.bufferDict.AddBuffer_NoDuplicate("_Texcoords", texcoordsComputeBuffer);
        }

        public void RebuildMeshInfo()
        {
            var control = DanbiManager.instance.shaderControl;

            switch (m_texType)
            {
                case EDanbiTextureType.Regular:
                    m_panoramaRegular.RebuildMeshInfoForComputeShader(ref m_panoramaShapeData);
                    break;

                case EDanbiTextureType.Faces4:
                    m_panorama4faces.RebuildMeshInfoForComputeShader(ref m_panoramaShapeData);
                    break;

                case EDanbiTextureType.Panorama:
                    m_panorama360.RebuildMeshInfoForComputeShader(ref m_panoramaShapeData);
                    break;
            }
            m_reflector.RebuildMeshInfoForComputeShader(ref m_reflectorShapeData);

            // 5. reflector mesh shape data
            switch (m_meshType)
            {
                // case EDanbiPrewarperSetting_MeshType.Custom_Cone:
                //     // control.buffersDic.Add("_", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiConeData_struct>((reflectorShapeData as DanbiConeData).asStruct, 1));
                //     break;

                // case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
                //     // control.buffersDic.Add("_", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiCylinderData_struct>((reflectorShapeData as DanbiCylinderData).asStruct, 1));
                //     break;

                case EDanbiPrewarperSetting_MeshType.Custom_Dome:
                    var domeData = m_reflectorShapeData as DanbiDomeData;
                    var domeDataComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(domeData.asStruct, domeData.stride);
                    control.bufferDict.AddBuffer_NoDuplicate("_DomeData", domeDataComputeBuffer);
                    break;
            }

            // 6. panorama mesh shape data
            // since panorama shares same shape data, there's no need to make overlaps.
            var panoramaData = m_panoramaShapeData as DanbiPanoramaData;
            var panoramaDataComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(panoramaData.asStruct, panoramaData.stride);
            control.bufferDict.AddBuffer_NoDuplicate("_PanoramaData", panoramaDataComputeBuffer);
        }

        void OnTextureTypeChange(EDanbiTextureType type)
        {
            m_texType = type;

            switch (m_texType)
            {
                case EDanbiTextureType.Regular:
                    m_panoramaRegular.gameObject.SetActive(true);
                    m_panorama360.gameObject.SetActive(false);
                    m_panorama4faces.gameObject.SetActive(false);
                    break;

                case EDanbiTextureType.Faces4:
                    m_panoramaRegular.gameObject.SetActive(false);
                    m_panorama360.gameObject.SetActive(false);
                    m_panorama4faces.gameObject.SetActive(true);
                    break;

                case EDanbiTextureType.Panorama:
                    m_panoramaRegular.gameObject.SetActive(false);
                    m_panorama360.gameObject.SetActive(true);
                    m_panorama4faces.gameObject.SetActive(false);
                    break;
            }

            RebuildMeshShape();
            RebuildMeshInfo();
        }

    };
};
