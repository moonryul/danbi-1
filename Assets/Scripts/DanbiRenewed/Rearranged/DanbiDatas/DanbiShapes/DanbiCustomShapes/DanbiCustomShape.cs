
using UnityEngine;

namespace Danbi {
  /// <summary>
  /// Danbi ray-tracer object class that is only for the user-created meshes.
  /// </summary>
  public class DanbiCustomShape : DanbiBaseShape {
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public struct DanbiCustomMeshMaterialProperty {
      public Vector3 Albedo;
      public Vector3 Specular;
      public float Smoothness;
      public Vector3 Emission;
    };

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] protected DanbiCustomMeshMaterialProperty CustomMeshMaterialProp;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newShapeName"></param>
    public DanbiCustomShape(string newShapeName) {
      MeshData = new DanbiMeshData {
        VerticesCount = 0,
        IndicesCount = 0,
        uvCount = 0
      };
      ShapeName = newShapeName;
    }
    
    protected override void Start() {
      base.Start();
      CustomMeshMaterialProp = new DanbiCustomMeshMaterialProperty {
        Albedo = new Vector3(0.0f, 0.0f, 0.0f),
        Specular = new Vector3(1.0f, 1.0f, 1.0f),
        Smoothness = 1.0f,
        Emission = new Vector3(0.0f, 0.0f, 0.0f)
      };
    }
  };
};