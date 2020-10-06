using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Danbi
{
    public class DanbiHalfSphere : DanbiBaseShape
    {
        [SerializeField]
        DanbiHalfsphereData ShapeData = new DanbiHalfsphereData();

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(0.08f, 0.08f, 0.08f);

        protected override void Awake()
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

            var heightOffset = new Vector3(0, -(ShapeData.Distance + ShapeData.Height), 0);
            transform.position = mainCamTransform.position + heightOffset * 0.01f;

            var newScale = new Vector3(ShapeData.Radius / originalSize.x, ShapeData.Height / originalSize.y, ShapeData.Radius / originalSize.z);
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
            if (!(control is DanbiUIReflectorShapePanelControl)) { return; }

            var halfspherePanel = control as DanbiUIReflectorShapePanelControl;
            ShapeData.Distance = halfspherePanel.Halfsphere.distance;
            ShapeData.Height = halfspherePanel.Halfsphere.height;
            ShapeData.Radius = halfspherePanel.Halfsphere.radius;
            ShapeData.specular = new Vector3(0.1f, 0.1f, 0.1f); // TODO: Specular!            

            OnShapeChanged();
        }
    }; // class ending.
}; // namespace Danbi
