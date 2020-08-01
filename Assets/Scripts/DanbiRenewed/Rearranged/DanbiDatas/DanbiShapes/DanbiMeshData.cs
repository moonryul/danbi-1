using System.Collections.Generic;
using UnityEngine;

namespace Danbi {  
  [System.Serializable]
  public struct DanbiMeshData {
    public List<Vector3> Vertices; // 
    public int VertexCount;
    public List<int> Indices;
    public uint IndexCount;
    public uint IndexOffset;
    public List<Vector2> Texcoords;
    public int TexcoordsCount;
  };
};
