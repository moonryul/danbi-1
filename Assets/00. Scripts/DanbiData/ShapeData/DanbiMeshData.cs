using UnityEngine;
using System.Collections.Generic;

namespace Danbi
{
    [System.Serializable]
    public struct DanbiMeshData
    {
        public List<Vector3> Vertices;
        public List<int> Indices;
        public List<Vector2> Texcoords;
    };
};