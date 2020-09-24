using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Danbi
{
    public class DanbiHalfSphere : DanbiBaseShape
    {
        DanbiHalfsphereData ShapeData = new DanbiHalfsphereData();
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


    }; // class ending.
}; // namespace Danbi
