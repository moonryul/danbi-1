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
        DanbiBaseShape m_panoramaCeiling;
        EDanbiTextureType m_texType;

        DanbiMeshesData m_panoramaMeshData = new DanbiMeshesData();
        DanbiMeshesData m_reflectorMeshData = new DanbiMeshesData();
        DanbiMeshesData m_meshData = new DanbiMeshesData();

        DanbiBaseShapeData m_panoramaShapeData = new DanbiBaseShapeData();
        DanbiBaseShapeData m_reflectorShapeData = new DanbiBaseShapeData();

        void Awake()
        {
            DanbiUISync.onPanelUpdate += this.OnPanelUpdate;
            m_panoramaRegular = transform.GetChild(1).GetComponent<DanbiBaseShape>();
            m_panoramaCeiling = transform.GetChild(2).GetComponent<DanbiBaseShape>();
            m_reflector = transform.GetChild(3).GetComponent<DanbiBaseShape>();
        }

        void Start()
        {            
            m_panoramaCeiling.gameObject.SetActive(false);            
        }

        public void RebuildMesh()
        {
            var control = DanbiManager.instance.shaderControl;
            m_panoramaMeshData.Clear();
            m_reflectorMeshData.Clear();
            m_meshData.Clear();

            //  fill out with the meshData for mesh data and the shape data for Shader.

            switch (m_texType)
            {
                case EDanbiTextureType.Regular:
                case EDanbiTextureType.Faces4:
                    m_panoramaRegular.RebuildMesh_internal(ref m_panoramaMeshData);
                    break;

                case EDanbiTextureType.Panorama:
                    m_panoramaCeiling.RebuildMesh_internal(ref m_panoramaMeshData);
                    break;
            }
            m_reflector.RebuildMesh_internal(ref m_reflectorMeshData);

            m_meshData.JoinData(m_panoramaMeshData, m_reflectorMeshData);

            // 3. Find Kernel and set it as a current kernel.            
            DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.CalcCurrentKernelIndex(m_meshType, m_panoramaType);

            // 4. Populate the compuate buffer dictionary.       
            var vtxComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_meshData.Vertices, 12);
            control.bufferDict.AddBuffer_NoOverlap("_Vertices", vtxComputeBuffer);

            var idxComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_meshData.Indices, 4);
            control.bufferDict.AddBuffer_NoOverlap("_Indices", idxComputeBuffer);

            var texcoordsComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_meshData.Texcoords, 8);
            control.bufferDict.AddBuffer_NoOverlap("_Texcoords", texcoordsComputeBuffer);
        }

        public void RebuildShape()
        {
            var control = DanbiManager.instance.shaderControl;

            switch (m_texType)
            {
                case EDanbiTextureType.Regular:
                case EDanbiTextureType.Faces4:
                    m_panoramaRegular.RebuildShape_internal(ref m_panoramaShapeData);
                    break;

                case EDanbiTextureType.Panorama:
                    m_panoramaCeiling.RebuildShape_internal(ref m_panoramaShapeData);
                    break;
            }
            m_reflector.RebuildShape_internal(ref m_reflectorShapeData);

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
                    control.bufferDict.AddBuffer_NoOverlap("_DomeData", domeDataComputeBuffer);
                    break;
            }

            // 6. panorama mesh shape data
            // since panorama shares same shape data, there's no need to make overlaps.
            var panoramaData = m_panoramaShapeData as DanbiPanoramaData;
            var panoramaDataComputeBuffer = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(panoramaData.asStruct, panoramaData.stride);
            control.bufferDict.AddBuffer_NoOverlap("_PanoramaData", panoramaDataComputeBuffer);
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var imageControl = control as DanbiUIImageGeneratorTexturePanelControl;
                m_texType = imageControl.textureType;

                switch (m_texType)
                {
                    case EDanbiTextureType.Regular:
                    case EDanbiTextureType.Faces4:
                        m_panoramaRegular.gameObject.SetActive(true);
                        m_panoramaCeiling.gameObject.SetActive(false);
                        break;

                    case EDanbiTextureType.Panorama:
                        m_panoramaRegular.gameObject.SetActive(false);
                        m_panoramaCeiling.gameObject.SetActive(true);
                        break;
                }

                RebuildMesh();
                RebuildShape();
            }

            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var videoControl = control as DanbiUIVideoGeneratorVideoPanelControl;
                m_texType = videoControl.vidType == EDanbiVideoType.Regular ?
                    EDanbiTextureType.Regular : EDanbiTextureType.Panorama;

                switch (m_texType)
                {
                    case EDanbiTextureType.Regular:
                        m_panoramaRegular.gameObject.SetActive(true);
                        m_panoramaCeiling.gameObject.SetActive(false);
                        break;

                    case EDanbiTextureType.Panorama:
                        m_panoramaRegular.gameObject.SetActive(false);
                        m_panoramaCeiling.gameObject.SetActive(true);
                        break;
                }

                RebuildMesh();
                RebuildShape();
            }

            if (control is DanbiUIReflectorShapePanelControl)
            {
                var reflControl = control as DanbiUIReflectorShapePanelControl;
                m_meshType = (EDanbiPrewarperSetting_MeshType)reflControl.selectedReflectorIndex;
                RebuildMesh();
            }

            if (control is DanbiUIPanoramaScreenShapePanelControl)
            {
                var panoControl = control as DanbiUIPanoramaScreenShapePanelControl;
                m_panoramaType = (EDanbiPrewarperSetting_PanoramaType)panoControl.selectedPrewarperSettingIndex;
                RebuildMesh();
            }

            if (control is DanbiUIReflectorOpticalPanelControl || control is DanbiUIPanoramaScreenOpticalPanelControl ||
                control is DanbiUIReflectorDimensionPanelControl || control is DanbiUIPanoramaScreenDimensionPanelControl)
            {
                RebuildShape();
            }
        }
    };
};
