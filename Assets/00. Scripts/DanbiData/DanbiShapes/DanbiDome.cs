using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Danbi
{
    public class DanbiDome : DanbiBaseShape
    {
        [SerializeField, Space(20)]
        DanbiDomeData m_domeShape = new DanbiDomeData();

        [SerializeField, Readonly]
        float originalRadius = 8.0f; // unit is cm

        protected override void Awake()
        {
            base.Awake();
            DanbiUISync.onPanelUpdate += OnPanelUpdated;
        }

        protected override void OnShapeChanged()
        {
            if (m_domeShape.radius <= 0.0f)
            {
                return;
            }

            transform.position = Camera.main.transform.position
                + new Vector3(0, -(m_domeShape.distance + m_domeShape.radius), 0) * 0.01f;
            transform.localScale = new Vector3(m_domeShape.radius / originalRadius,
                                               m_domeShape.radius / originalRadius,
                                               m_domeShape.radius / originalRadius); // unit is cm
        }

        public override void RebuildMesh_internal(ref DanbiMeshesData dat)
        {
            base.RebuildMesh_internal(ref dat);
            m_domeShape.indexOffset = dat.prevIndexCount;
            m_domeShape.indexCount = dat.Indices.Count;
        }

        public override void RebuildShape_internal(ref DanbiBaseShapeData dat)
        {
            m_domeShape.local2World = transform.localToWorldMatrix;
            m_domeShape.world2Local = transform.worldToLocalMatrix;
            dat = m_domeShape;
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIReflectorDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIReflectorDimensionPanelControl;

                float h = dimensionPanel.Dome.height;
                float dr = dimensionPanel.Dome.bottomRadius;
                float maskingRatio = dimensionPanel.Dome.maskingRatio * 0.01f;

                m_domeShape.radius = ((h * h) + (dr * dr)) / (2 * h);
                m_domeShape.usedHeight = (1.0f - maskingRatio) * h;
                m_domeShape.distance = dimensionPanel.Dome.distance;

                OnShapeChanged();
            }

            if (control is DanbiUIReflectorOpticalPanelControl)
            {
                var opticalPanel = control as DanbiUIReflectorOpticalPanelControl;

                m_domeShape.specular = new Vector3(opticalPanel.Dome.specularR, opticalPanel.Dome.specularG, opticalPanel.Dome.specularB);
                m_domeShape.emission = new Vector3(opticalPanel.Dome.emissionR, opticalPanel.Dome.emissionG, opticalPanel.Dome.emissionB);
            }
        }
    }; // class ending.
}; // namespace Danbi
