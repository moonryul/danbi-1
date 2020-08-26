using UnityEngine;
using System.Collections.Generic;
using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi {
  public class DanbiHalfSphere : DanbiBaseShape {
    [SerializeField]
    DanbiMeshShapeTransform ShapeTransform;
    public DanbiMeshShapeTransform shapeTransform => ShapeTransform;

    protected override void Caller_OnMeshRebuild(ref POD_MeshData data, out AdditionalData additionalData) {
      base.Caller_OnMeshRebuild(ref data, out additionalData);
      ShapeTransform.local2World = transform.localToWorldMatrix;
      additionalData = new AdditionalData(opticalData, ShapeTransform);
    }

    protected override void OnShapeChanged() {
      var MainCamTransform = transform.parent;
      transform.position = MainCamTransform.position +
          new Vector3(0, -(ShapeTransform.Distance + ShapeTransform.Height), 0);
    }
  }; // class ending.
}; // namespace Danbi
