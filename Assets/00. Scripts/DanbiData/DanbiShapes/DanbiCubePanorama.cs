using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        [SerializeField, Readonly, Space(20)]
        float width;

        [SerializeField, Readonly]
        float depth;

        [SerializeField]
        DanbiPanoramaData m_panoramaShape = new DanbiPanoramaData();
        public DanbiPanoramaData shapeData => m_panoramaShape;

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(320f, 67.48f, 320f);

        override protected void Awake()
        {
            base.Awake();
            DanbiUISync.onPanelUpdate += this.OnPanelUpdate;
        }

        protected override void OnShapeChanged()
        {
            var heightOffset = new Vector3(0, m_panoramaShape.low, 0);
            transform.position = Camera.main.transform.position + (heightOffset * 0.01f);
            transform.localScale = new Vector3(width / originalSize.x,
                                               (m_panoramaShape.high - m_panoramaShape.low) / originalSize.y,
                                               depth / originalSize.z);
        }

        public override void RebuildMesh_internal(ref DanbiMeshesData dat)
        {
            base.RebuildMesh_internal(ref dat);
            m_panoramaShape.indexOffset = dat.prevIndexCount;
            m_panoramaShape.indexCount = dat.Indices.Count;
        }

        public override void RebuildShape_internal(ref DanbiBaseShapeData dat)
        {
            m_panoramaShape.local2World = transform.localToWorldMatrix;
            m_panoramaShape.world2Local = transform.worldToLocalMatrix;
            dat = m_panoramaShape;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            // control is the parent of all the control which related to ui panel control.
            // if ((DanbiUIPanoramaScreenDimensionPanelControl)control != null)
            if (control is DanbiUIPanoramaScreenDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIPanoramaScreenDimensionPanelControl;

                width = dimensionPanel.Cube.width;
                depth = dimensionPanel.Cube.depth;
                m_panoramaShape.high = dimensionPanel.Cube.ch;
                m_panoramaShape.low = dimensionPanel.Cube.cl;

                OnShapeChanged();
            }

            if (control is DanbiUIPanoramaScreenOpticalPanelControl)
            {
                var opticalPanel = control as DanbiUIPanoramaScreenOpticalPanelControl;
                m_panoramaShape.specular = new Vector3(opticalPanel.Cube.specularR, opticalPanel.Cube.specularG, opticalPanel.Cube.specularB);
                m_panoramaShape.emission = new Vector3(opticalPanel.Cube.emissionR, opticalPanel.Cube.emissionG, opticalPanel.Cube.emissionB);
            }
        }
    };
};
