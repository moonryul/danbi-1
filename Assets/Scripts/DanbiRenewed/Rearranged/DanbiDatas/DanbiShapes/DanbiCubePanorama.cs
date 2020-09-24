using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        DanbiPanoramaData ShapeData = new DanbiPanoramaData();

        [SerializeField, Readonly]
        float originalHeight = 0.6748f;

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

        // void OnPanelUpdated(DanbiUIPanelControl control)
        // {
        //     var panoramaShapePanel = control as DanbiUIPanoramaScreenShapePanelControl;            

        // }

        // void OnPanelUpdated(DanbiUIPanelControl control)
        // {
        //     var panoramaShapePanel = control as DanbiUIPanoramaScreenShapePanelControl;            

        // }
    };
};
