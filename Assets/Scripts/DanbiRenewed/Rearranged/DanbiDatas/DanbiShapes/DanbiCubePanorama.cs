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
            var heightOffset = new Vector3(0, ShapeData.low, 0);
            transform.position = Camera.main.transform.position + (heightOffset * 0.01f);
            transform.localScale = new Vector3(width / originalSize.x,
                                               (ShapeData.high - ShapeData.low) / originalSize.y,
                                               depth / originalSize.z) * 0.01f;
        }

        protected override void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                     out DanbiBaseShapeData shapeData)
        {
            BaseShapeData = ShapeData;
            base.Caller_OnMeshRebuild(ref data, out shapeData);
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIPanoramaScreenDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIPanoramaScreenDimensionPanelControl;

                width = dimensionPanel.Cube.width;
                depth = dimensionPanel.Cube.depth;
                ShapeData.high = dimensionPanel.Cube.ch;
                ShapeData.low = dimensionPanel.Cube.cl;
                ShapeData.specular = Vector3.one; //new Vector3(0.1f, 0.1f, 0.1f);
                
                OnShapeChanged();
            }
        }
    };
};
