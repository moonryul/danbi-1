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
        float m_originalRadius = 8.0f; // unit is cm

        float m_height;
        float m_bottomRadius;
        float m_maskingRatio;

        protected override void Awake()
        {
            base.Awake();

            DanbiUIReflectorDome.onHeightUpdate +=
                (float height) =>
                {
                    m_height = height;
                    OnShapeChanged();
                };

            DanbiUIReflectorDome.onBottomRadiusUpdate +=
                (float radius) =>
                {
                    m_bottomRadius = radius;
                    OnShapeChanged();
                };

            DanbiUIReflectorDome.onMaskingRatioUpdate +=
                (float maskingRatio) =>
                {
                    m_maskingRatio = maskingRatio * 0.01f;
                    // OnShapeChanged();
                };

            DanbiUIReflectorDome.onDistanceUpdate +=
                (float distance) =>
                {
                    m_domeShape.distance = distance;
                    OnShapeChanged();
                };
        }

        protected override void OnShapeChanged()
        {
            if (m_height <= 0.0f)
            {
                return;
            }
            float h = m_height;

            if (m_bottomRadius <= 0.0f)
            {
                return;
            }
            float dr = m_bottomRadius;

            // if (m_maskingRatio < 0.0f)
            // {
            //     return;
            // }
            // float ratio = m_maskingRatio;
            // Debug.Log($"masking ratio : {ratio}");

            m_domeShape.radius = ((h * h) + (dr * dr)) / (2 * h);
            // m_domeShape.usedHeight = (1.0f - ratio) * h;

            if (m_domeShape.radius <= 0.0f)
            {
                return;
            }

            transform.position = Camera.main.transform.position
                + new Vector3(0, -(m_domeShape.distance + m_domeShape.radius), 0) * 0.01f;
            transform.localScale = new Vector3(m_domeShape.radius / m_originalRadius,
                                               m_domeShape.radius / m_originalRadius,
                                               m_domeShape.radius / m_originalRadius); // unit is cm
        }

        public override void RebuildMeshShapeForComputeShader(ref DanbiMeshesData meshesData)
        {
            base.RebuildMeshShapeForComputeShader(ref meshesData);
            m_domeShape.indexOffset = meshesData.prevIndexCount;
            m_domeShape.indexCount = meshesData.Indices.Count;
        }

        public override void RebuildMeshInfoForComputeShader(ref DanbiBaseShapeData shapesData)
        {
            m_domeShape.local2World = transform.localToWorldMatrix;
            m_domeShape.world2Local = transform.worldToLocalMatrix;
            m_domeShape.specular = new Vector3(1.0f, 1.0f, 1.0f);
            m_domeShape.emission = new Vector3(0.0f, 0.0f, 0.0f);

            float h = m_height;
            float dr = m_bottomRadius;            
            float ratio = m_maskingRatio;
            Debug.Log($"masking ratio : {ratio}");

            m_domeShape.radius = ((h * h) + (dr * dr)) / (2 * h);
            m_domeShape.usedHeight = (1.0f - ratio) * h;

            shapesData = m_domeShape;
        }
    }; // class ending.
}; // namespace Danbi
