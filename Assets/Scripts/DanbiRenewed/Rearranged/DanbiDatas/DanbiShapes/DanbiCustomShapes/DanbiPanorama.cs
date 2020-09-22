using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi {
  public class DanbiPanorama : DanbiBaseShape {    
    [SerializeField, Readonly]
    float originalHeight = 0.6748f;
    protected override void OnShapeChanged() {
      var mainCamTransform = Camera.main.transform;
      var heightOffset = new Vector3(0, ShapeTransform.low, 0);
      transform.position = mainCamTransform.position + heightOffset;

      float newScaleY = (ShapeTransform.high - ShapeTransform.low) / originalHeight;
      transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
    }
  };
};
