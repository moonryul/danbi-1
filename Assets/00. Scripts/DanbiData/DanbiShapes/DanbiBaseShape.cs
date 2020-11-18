using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
    public class DanbiBaseShape : MonoBehaviour
    {
        [SerializeField, Readonly]
        int m_vertexCount;

        [SerializeField, Readonly]
        int m_indexCount;

        [SerializeField, Readonly]
        int m_texcoordsCount;

        [SerializeField, Readonly]
        int m_prevIndexCount;

        Mesh m_mesh;

        protected virtual void Awake()
        {
            m_mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        public virtual void RebuildMesh_internal(ref DanbiMeshesData meshesData)
        {
            int previousVertexCount = meshesData.Vertices.Count;
            m_prevIndexCount = meshesData.prevIndexCount = meshesData.Indices.Count;
            meshesData.Vertices.AddRange(m_mesh.vertices);
            m_vertexCount = meshesData.Vertices.Count;

            meshesData.Texcoords.AddRange(m_mesh.uv);
            m_texcoordsCount = meshesData.Texcoords.Count;

            var indices = m_mesh.GetIndices(0);
            m_indexCount = indices.Length;

            meshesData.Indices.AddRange(indices.Select(i => i + previousVertexCount));
        }

        public virtual void RebuildShape_internal(ref DanbiBaseShapeData shapesData)
        {
            //
        }

        protected virtual void OnShapeChanged() { }
    };
};
