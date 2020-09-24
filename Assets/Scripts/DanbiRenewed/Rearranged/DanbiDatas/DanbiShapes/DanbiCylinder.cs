using UnityEngine;
// using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi
{
    public class DanbiCylinder : DanbiBaseShape
    {
        DanbiCyclinderData ShapeData;
        protected override void OnShapeChanged()
        {
            var mainCamTransform = transform.parent;
            transform.position = mainCamTransform.position +
                new Vector3(0, -(ShapeData.Distance + ShapeData.Height), 0);
        }

        // protected override void Caller_OnMeshRebuild(ref DanbiMeshData data, ref DanbiOpticalData opticalData, DanbiBaseShapeData shapeData)
        // {
            
        //     base.Caller_OnMeshRebuild(ref data, ref opticalData, shapeData);
        // }
    };
};