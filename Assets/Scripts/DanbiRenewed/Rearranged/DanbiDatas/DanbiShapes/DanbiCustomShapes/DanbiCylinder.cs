namespace Danbi {
  public class DanbiCylinder : DanbiCustomShape {
    protected override void Start() {
      base.Start();

      ShapeTransform = new DanbiShapeTransform {
        Distance = 0.37f,
        Height = 0.08f,
        Radius = 0.04f,
        MaskingRatio = 0.1f
      };
    }

    protected override void Caller_CustomShapeChanged() {
      //
    }
  };
};