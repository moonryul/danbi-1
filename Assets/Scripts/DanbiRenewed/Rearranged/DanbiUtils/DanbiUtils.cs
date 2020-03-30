using UnityEngine;

namespace Danbi {
  public class DanbiUtils : MonoBehaviour {

  };

  public enum EDanbiCustomMeshType {
    Cylinder,
    Cube,
    Cone,
    Pyramid,
    Hemisphere,
  };

  public enum EDanbiProceduralMeshType {
    Sphere
  };

  public static class DanbiMeshHelper {
    /// <summary>
    /// Make the custom mesh.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="meshType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T MakeCustomMesh<T>(EDanbiCustomMeshType meshType, string name)
      where T : DanbiCustomShape {
      var newShape = new DanbiCustomShape(name);
      switch (meshType) {
        case EDanbiCustomMeshType.Cylinder:
        //newShape = new Danbi
        break;

        case EDanbiCustomMeshType.Cube:
        break;

        case EDanbiCustomMeshType.Cone:
        newShape = new DanbiCone(name);
        break;

        case EDanbiCustomMeshType.Pyramid:
        break;

        case EDanbiCustomMeshType.Hemisphere:
        break;
      }

      return (T)newShape;
    }

    //public static T MakeProceduralMesh<T>(EDanbiProceduralMeshType meshType, string name)
    //  where T : DanbiProceduralShape {
    //  var newShape = new DanbiProceduralShape(name);
    //  switch (meshType) {
    //    case EDanbiProceduralMeshType.Sphere:
    //    break;
    //  }

    //  return (T)newShape;
    //};
  };
};