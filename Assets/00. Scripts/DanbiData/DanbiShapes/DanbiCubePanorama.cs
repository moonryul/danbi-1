using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        [SerializeField, Readonly, Space(20)]
        float m_width;

        [SerializeField, Readonly]
        float m_depth;

        [SerializeField]
        DanbiPanoramaData m_panoramaShape = new DanbiPanoramaData();
        public DanbiPanoramaData shapeData => m_panoramaShape;

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(320f, 67.48f, 320f);

        override protected void Awake()
        {
            base.Awake();

            DanbiUIPanoramaCubeDimension.onWidthChange +=
                (float width) =>
                {
                    m_width = width;
                    OnShapeChanged();
                };

            DanbiUIPanoramaCubeDimension.onDepthChange +=
                (float depth) =>
                {
                    m_depth = depth;
                    OnShapeChanged();
                };

            DanbiUIPanoramaCubeDimension.onCHChange +=
                (float ch) =>
                {
                    m_panoramaShape.high = ch;
                    OnShapeChanged();
                };

            DanbiUIPanoramaCubeDimension.onCLChange +=
                (float cl) =>
                {
                    m_panoramaShape.low = cl;
                    OnShapeChanged();
                };
        }

        protected override void OnShapeChanged()
        {
            var heightOffset = new Vector3(0, m_panoramaShape.low, 0);
            transform.position = Camera.main.transform.position + (heightOffset * 0.01f);
            transform.localScale = new Vector3(m_width / originalSize.x,
                                               (m_panoramaShape.high - m_panoramaShape.low) / originalSize.y,
                                               m_depth / originalSize.z);
        }

        public override void RebuildMeshShapeForComputeShader(ref DanbiMeshesData dat)
        {
            base.RebuildMeshShapeForComputeShader(ref dat);
            m_panoramaShape.indexOffset = dat.prevIndexCount;
            m_panoramaShape.indexCount = dat.Indices.Count;
        }

        public override void RebuildMeshInfoForComputeShader(ref DanbiBaseShapeData dat)
        {
            m_panoramaShape.local2World = transform.localToWorldMatrix;
            m_panoramaShape.world2Local = transform.worldToLocalMatrix;
            m_panoramaShape.specular = new Vector3(0.9f, 0.9f, 0.9f);
            m_panoramaShape.emission = new Vector3(-1.0f, -1.0f, -1.0f);
            dat = m_panoramaShape;
        }
    };
};
