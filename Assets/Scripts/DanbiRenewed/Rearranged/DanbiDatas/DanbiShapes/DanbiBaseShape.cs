using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Danbi
{
    public class DanbiBaseShape : MonoBehaviour
    {
        // [SerializeField]
        // protected DanbiOpticalData OpticalData;
        // public DanbiOpticalData opticalData => OpticalData;
        public DanbiBaseShapeData BaseShapeData;

        [SerializeField]
        protected string ShapeName;
        // public string getShapeName => ShapeName;

        public delegate void OnMeshRebuild(ref DanbiMeshData data,
                                           out DanbiOpticalData opticalData,
                                           out DanbiBaseShapeData shapeTransform);
        /// <summary>
        /// Callback which is called when the mesh is rebuilt.
        /// </summary>
        public OnMeshRebuild Call_OnMeshRebuild;

        protected virtual void Awake()
        {
            // 1. Initialise the Optical Data.
            // OpticalData = new DanbiOpticalData
            // {
            //     albedo = new Vector3(0.9f, 0.9f, 0.9f),
            //     specular = new Vector3(0.1f, 0.1f, 0.1f),
            //     smoothness = 0.9f,
            //     emission = Vector3.zero
            // };

            // 2. Bind the OnMeshRebuild.
            Call_OnMeshRebuild += Caller_OnMeshRebuild;
        }

        void OnValidate() => OnShapeChanged();

        void OnDisable() => Call_OnMeshRebuild -= Caller_OnMeshRebuild;

        protected virtual void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                    out DanbiOpticalData opticalData,
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
            opticalData = default;
            shapeData = BaseShapeData;
        }

        protected virtual void OnShapeChanged() { }

        public virtual void PrintMeshInfo()
        {
            // Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VertexCount}, Indices Count : {MeshData.IndexCount}, UV Count : {MeshData.TexcoordsCount} >>", this);
        }
    };
};
