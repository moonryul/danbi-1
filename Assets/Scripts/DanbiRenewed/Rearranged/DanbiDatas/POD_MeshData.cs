using UnityEngine;
using System.Collections.Generic;

namespace Danbi {
  public struct POD_MeshData {
    public List<Vector3> vertices;
    public List<int> indices;
    public List<Vector2> texcoords;
    public List<int> indices_offsets;
    public List<int> indices_counts;

    public void ClearMeshData() {
      vertices.Clear();
      indices.Clear();
      texcoords.Clear();
    }
  };
};