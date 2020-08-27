using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi {
  public class DanbiPanorama : DanbiBaseShape {
    [SerializeField]
    DanbiPanoramaMeshShapeTransform ShapeTransform;
    public DanbiPanoramaMeshShapeTransform shapeTransform => ShapeTransform;

    protected override void Caller_OnMeshRebuild(ref POD_MeshData data, out AdditionalData additionalData) {
      base.Caller_OnMeshRebuild(ref data, out additionalData);
      ShapeTransform.local2World = transform.localToWorldMatrix;
      additionalData = new AdditionalData(opticalData, ShapeTransform);
    }

    protected override void OnShapeChanged() {
      var mainCamTransform = transform.parent;
      var heightOffset = new Vector3(0, -(ShapeTransform.Distance + ShapeTransform.Height), 0);
      transform.position = mainCamTransform.position + heightOffset;
    }
  };
};
