using UnityEngine;
using System.Collections.Generic;

namespace Danbi {
  public class DanbiHalfHemisphere : DanbiCustomShape {    



    protected override void Start() {
      base.Start();

      ShapeTransform = new DanbiShapeTransform {
        Distance = 0.08f,
        Height = 0.08f,
        Radius = 0.08f,
        MaskingRatio = 0.1f,
        local2World = Matrix4x4.identity,
      };
    }

    protected override void Caller_CustomShapeChanged() {
      //
    }

  }; // class ending.
}; // namespace Danbi
