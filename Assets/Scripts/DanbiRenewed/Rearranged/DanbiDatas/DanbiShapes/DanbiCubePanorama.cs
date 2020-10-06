using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        [SerializeField, Readonly]
        float width;

        [SerializeField, Readonly]
        float depth;

        [SerializeField]
        DanbiPanoramaData ShapeData = new DanbiPanoramaData();

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(3.2f, 0.6748f, 3.2f);

        override protected void Awake()
        {
            base.Awake();
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdated;
        }

        protected override void OnShapeChanged()
        {
            var mainCamTransform = Camera.main.transform;
            if (mainCamTransform.Null())
                return;

            var heightOffset = new Vector3(0, ShapeData.low, 0);
            transform.position = mainCamTransform.position + heightOffset * 0.01f;

            var newScale = new Vector3(width / originalSize.x, (ShapeData.high - ShapeData.low) / originalSize.y, depth / originalSize.z);            
            transform.localScale = newScale * 0.01f;
        }

        protected override void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                     out DanbiOpticalData opticalData,
                                                     out DanbiBaseShapeData shapeData)
        {
            BaseShapeData = ShapeData;
            base.Caller_OnMeshRebuild(ref data, out opticalData, out shapeData);
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (!(control is DanbiUIPanoramaScreenShapePanelControl))
                return;

            var panoramaShapePanel = control as DanbiUIPanoramaScreenShapePanelControl;

            width = panoramaShapePanel.Cube.width;
            depth = panoramaShapePanel.Cube.depth;

            ShapeData.high = panoramaShapePanel.Cube.ch;
            ShapeData.low = panoramaShapePanel.Cube.cl;
            ShapeData.specular = new Vector3(0.1f, 0.1f, 0.1f);
            OnShapeChanged();
        }
    };
};
