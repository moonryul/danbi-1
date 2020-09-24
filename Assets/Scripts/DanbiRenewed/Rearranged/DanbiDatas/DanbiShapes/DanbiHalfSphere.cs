using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Danbi
{
    public class DanbiHalfSphere : DanbiBaseShape
    {
        DanbiHalfsphereData ShapeData = new DanbiHalfsphereData();

        protected override void Awake()
        {
            base.Awake();

            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdated;
        }
        protected override void OnShapeChanged()
        {
            var MainCamTransform = transform.parent;
            transform.position = MainCamTransform.position +
                new Vector3(0, -(ShapeData.Distance + ShapeData.Height), 0);
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
            ShapeData.Distance = halfspherePanel.Halfsphere.distanceFromProjector;
            ShapeData.Height = halfspherePanel.Halfsphere.height;
            ShapeData.Radius = halfspherePanel.Halfsphere.radius;
            ShapeData.specular = new Vector3(0.1f, 0.1f, 0.1f); // TODO: Specular!            
        }

    }; // class ending.
}; // namespace Danbi
