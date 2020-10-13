using UnityEngine;
using System.Collections.Generic;

namespace Danbi
{
#pragma warning disable 3001
    [System.Serializable]
    public struct DanbiMeshData
    {
        public List<Vector3> Vertices;
        public List<int> Indices;
        public List<Vector2> Texcoords;

        public void init()
        {
            Vertices = new List<Vector3>();
            Indices = new List<int>();
            Texcoords = new List<Vector2>();
        }

        public void JoinData(params DanbiMeshData[] data)
        {
            for (var i = 0; i < data.Length; ++i)
            {
                this.Vertices.AddRange(data[i].Vertices);
                this.Indices.AddRange(data[i].Indices);
                this.Texcoords.AddRange(data[i].Texcoords);
            }
        }
    };
};