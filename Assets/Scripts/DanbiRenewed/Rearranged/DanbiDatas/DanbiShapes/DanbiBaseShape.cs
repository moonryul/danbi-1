using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
    public class DanbiBaseShape : MonoBehaviour
    {
        public DanbiBaseShapeData BaseShapeData;

        public delegate void OnMeshRebuild(ref DanbiMeshData data,
                                           out DanbiBaseShapeData shapeTransform);
        /// <summary>
        /// Callback which is called when the mesh is rebuilt.
        /// </summary>
        public OnMeshRebuild Call_OnMeshRebuild;

        protected virtual void Awake()
        {   // Bind the OnMeshRebuild.
            Call_OnMeshRebuild += Caller_OnMeshRebuild;
        }        

        protected virtual void OnDisable() => Call_OnMeshRebuild -= Caller_OnMeshRebuild;

        protected virtual void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                    out DanbiBaseShapeData shapeData)
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;
            data.Vertices.AddRange(mesh.vertices);
            data.Texcoords.AddRange(mesh.uv);

            int previousVertexCount = data.Vertices.Count;
            int previousIndexCount = data.Indices.Count;
            var indices = mesh.GetIndices(0);
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
