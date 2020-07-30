namespace Danbi {
  /// <summary>
  /// Danbi ray-tracer object class that is only for generating the procedural meshes.
  /// </summary>
  public class DanbiProceduralShape : DanbiBaseShape {

    public DanbiProceduralShape(string newShapeName) {
      ShapeName = newShapeName;
    }

    protected override void Start() {
      base.Start();
    }
  };
};
