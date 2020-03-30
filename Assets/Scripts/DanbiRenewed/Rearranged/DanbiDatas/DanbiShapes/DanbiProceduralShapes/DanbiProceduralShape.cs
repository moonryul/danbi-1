namespace Danbi {
  /// <summary>
  /// Danbi ray-tracer object class that is only for generating the procedural meshes.
  /// </summary>
  public class DanbiProceduralShape : DanbiBaseShape {

    public DanbiProceduralShape(string newShapeName) {
      MeshData = new DanbiMeshData {
        VerticesCount = 0,
        IndicesCount = 0,
        uvCount = 0
      };
      ShapeName = newShapeName;
    }

    protected override void Start() {
      base.Start();
    }
  };
};
