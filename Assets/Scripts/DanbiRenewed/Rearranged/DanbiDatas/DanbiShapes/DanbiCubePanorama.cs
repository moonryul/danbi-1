using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        DanbiPanoramaData dat;

        [SerializeField, Readonly]
        float originalHeight = 0.6748f;

        protected override void OnShapeChanged()
        {
            var mainCamTransform = Camera.main.transform;
            var heightOffset = new Vector3(0, dat.low, 0);
            transform.position = mainCamTransform.position + heightOffset;

            float newScaleY = (dat.high - dat.low) / originalHeight;
            transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            var panoramaShapePanel = control as DanbiUIPanoramaScreenShapePanelControl;            
            
        }
    };
};
