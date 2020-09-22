using UnityEngine;
using System.Collections.Generic;

namespace Danbi {
  public class POD_MeshData {
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> indices = new List<int>();
    public List<Vector2> texcoords = new List<Vector2>();
    public List<int> indices_offsets = new List<int>();
    public List<int> indices_counts = new List<int>();

    public void ClearMeshData() {
      vertices.Clear();
      indices.Clear();
      texcoords.Clear();
    }
  };
};