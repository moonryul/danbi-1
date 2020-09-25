using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        [SerializeField]
        DanbiPanoramaData ShapeData = new DanbiPanoramaData();

        [SerializeField, Readonly]
        float originalHeight = 0.6748f;

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
            if (mainCamTransform.Null()) { return; }

            var heightOffset = new Vector3(0, ShapeData.low, 0);
            transform.position = mainCamTransform.position + heightOffset;

            float newScaleY = (ShapeData.high - ShapeData.low) / originalHeight;
            transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
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
            if (!(control is DanbiUIPanoramaScreenShapePanelControl)) { return; }

            var panoramaShapePanel = control as DanbiUIPanoramaScreenShapePanelControl;
            // ShapeData.width = panoramaShapePanel.Cube.width;
            // ShapeData.height = panoramaShapePanel.Cube.height;
            // ShapeData.depth = panoramaShapePanel.Cube.depth;

            ShapeData.high = panoramaShapePanel.Cube.ch;
            ShapeData.low = panoramaShapePanel.Cube.cl;
            ShapeData.specular = new Vector3(0.1f, 0.1f, 0.1f);
        }
    };
};
