using UnityEngine;
// using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi
{
    public class DanbiCylinderPanorama : DanbiBaseShape
    {
        [SerializeField, Readonly, Space(20)]
        float radius;

        [SerializeField]
        DanbiPanoramaData m_cylinderShape = new DanbiPanoramaData();

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(3.2f, 0.6718f, 3.2f);

        override protected void Awake()
        {
            base.Awake();
            //DanbiUISync.onPanelUpdate += OnPanelUpdated;
        }

        protected override void OnShapeChanged()
        {
            Vector3 heightOffset = new Vector3(0, m_cylinderShape.low, 0);
            transform.position = Camera.main.transform.position + (heightOffset * 0.01f);
            transform.localScale = new Vector3(radius / originalSize.x,
                                               (m_cylinderShape.high - m_cylinderShape.low) / originalSize.y,
                                               radius / originalSize.z) * 0.01f;
        }

        public override void RebuildMeshShapeForComputeShader(ref DanbiMeshesData dat)
        {
            base.RebuildMeshShapeForComputeShader(ref dat);
            m_cylinderShape.indexOffset = dat.prevIndexCount;
            m_cylinderShape.indexCount = dat.Indices.Count;
        }

        public override void RebuildMeshInfoForComputeShader(ref DanbiBaseShapeData dat)
        {
            m_cylinderShape.local2World = transform.localToWorldMatrix;
            m_cylinderShape.world2Local = transform.worldToLocalMatrix;
            m_cylinderShape.specular = new Vector3(0.9f, 0.9f, 0.9f);
            m_cylinderShape.emission = new Vector3(-1.0f, -1.0f, -1.0f);
            dat = m_cylinderShape;
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            // if (control is DanbiUIPanoramaScreenDimensionPanel)
            // {
            //     var dimensionPanel = control as DanbiUIPanoramaScreenDimensionPanel;

            //     radius = dimensionPanel.m_cylinder.m_radius;
            //     m_cylinderShape.high = dimensionPanel.m_cylinder.m_ch;
            //     m_cylinderShape.low = dimensionPanel.m_cylinder.m_cl;

            //     OnShapeChanged();
            // }

            // if (control is DanbiUIPanoramaScreenOpticalPanelControl)
            // {
            //     var opticalPanel = control as DanbiUIPanoramaScreenOpticalPanelControl;

            //     m_cylinderShape.specular = new Vector3(opticalPanel.Cylinder.specularR, opticalPanel.Cylinder.specularG, opticalPanel.Cylinder.specularB);
            //     m_cylinderShape.emission = new Vector3(opticalPanel.Cylinder.emissionR, opticalPanel.Cylinder.emissionG, opticalPanel.Cylinder.emissionB);
            // }
        }
    };
};