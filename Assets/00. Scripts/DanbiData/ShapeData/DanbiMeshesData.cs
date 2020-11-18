using UnityEngine;
using System.Collections.Generic;

namespace Danbi
{
#pragma warning disable 3001
    [System.Serializable]
    public class DanbiMeshesData
    {
        public List<Vector3> Vertices = new List<Vector3>();
        public List<int> Indices = new List<int>();
        public List<Vector2> Texcoords = new List<Vector2>();
        public int prevIndexCount = 0;

        public void JoinData(params DanbiMeshesData[] meshesData)
        {
            for (var i = 0; i < meshesData.Length; ++i)
            {
                this.Vertices.AddRange(meshesData[i].Vertices);
                this.Indices.AddRange(meshesData[i].Indices);
                this.Texcoords.AddRange(meshesData[i].Texcoords);
            }
        }

    };
};