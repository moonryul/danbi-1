

using System.Collections.Generic;
using UnityEngine;

namespace Danbi {  
  [System.Serializable]
  public struct DanbiMeshData {
    public List<Vector3> Vertices; // 4 * 3 * Count
    public int VertexCount; // 4
    public List<int> Indices; // 4 * Count
    public uint IndexCount; // 4
    public uint IndexOffset; // 4
    public List<Vector2> Texcoords; // 4 * 2 * Count
    public int TexcoordsCount; // 4

    public int stride => (4 * 3 * Vertices.Count) +
                         4 +
                         (4 * Indices.Count) +
                         4 + 4 +
                         (4 * 2 * Texcoords.Count) +
                         4;    
  };
};
