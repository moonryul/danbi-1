using UnityEngine;

namespace Danbi {
  public class DanbiCone : DanbiCustomShape {            
    protected override void Start() {
      base.Start();

      ShapeTransform = new DanbiShapeTransform {
        Distance = 0.37f,
        Height = 0.1f,
        Radius = 0.05f,
        MaskingRatio = 0.1f
      };
    }


    protected override void OnShapeChanged() {      
      var CameraOriginLocation = new Vector3(0.0f, -(ShapeTransform.Distance + ShapeTransform.Height), 0.0f);
      var CameraLocation = MainCamRef.transform.position;
      transform.position = CameraOriginLocation + CameraOriginLocation;
    }
  }; // class ending
}; // namespace Danbi