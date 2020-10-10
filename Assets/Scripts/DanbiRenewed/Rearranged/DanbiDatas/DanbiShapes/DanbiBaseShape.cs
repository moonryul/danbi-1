using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
    public class DanbiBaseShape : MonoBehaviour
    {
        public DanbiBaseShapeData BaseShapeData;
        [SerializeField, Readonly]
        int VertexCount;
        [SerializeField, Readonly]
        int IndexCount;
        [SerializeField, Readonly]
        int TexcoordsCount;

        Mesh mesh;

        public delegate void OnMeshRebuild(ref DanbiMeshData data,
                                           out DanbiBaseShapeData shapeTransform);
        /// <summary>
        /// Callback which is called when the mesh is rebuilt.
        /// </summary>
        public OnMeshRebuild Call_OnMeshRebuild;

        protected virtual void Awake()
        {   // Bind the OnMeshRebuild.
            Call_OnMeshRebuild += Caller_OnMeshRebuild;
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        protected virtual void OnDisable() => Call_OnMeshRebuild -= Caller_OnMeshRebuild;

        protected virtual void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                    out DanbiBaseShapeData shapeData)
        {
            int previousVertexCount = data.Vertices.Count;
            int previousIndexCount = data.Indices.Count;
            
            data.Vertices.AddRange(mesh.vertices);
            VertexCount = data.Vertices.Count;

            data.Texcoords.AddRange(mesh.uv);
            TexcoordsCount = data.Texcoords.Count;

            var indices = mesh.GetIndices(0);
            IndexCount = indices.Length;

            data.Indices.AddRange(indices.Select(i => i + previousVertexCount));

            BaseShapeData.indexOffset = previousIndexCount;
            BaseShapeData.indexCount = indices.Length;
            BaseShapeData.local2World = transform.localToWorldMatrix;
            // TODO: recover later on the project runs normally
            // BaseShapeData.world2Local = transform.worldToLocalMatrix;
            shapeData = BaseShapeData;
        }

        protected virtual void OnShapeChanged() { }

        public virtual void PrintMeshInfo()
        {
            // Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VertexCount}, Indices Count : {MeshData.IndexCount}, UV Count : {MeshData.TexcoordsCount} >>", this);
        }
    };
};
