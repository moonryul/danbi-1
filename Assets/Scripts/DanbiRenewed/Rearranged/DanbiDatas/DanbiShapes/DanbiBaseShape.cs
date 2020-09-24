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
        public DanbiHalfsphereData ShapeTransform { get; set; }

        [SerializeField]
        protected string ShapeName;
        // public string getShapeName => ShapeName;

        public delegate void OnMeshRebuild(ref DanbiMeshData data,
                                           out DanbiOpticalData opticalData,
                                           out DanbiHalfsphereData shapeTransform);
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

            // 2. Intialise the Mesh Data.
            // var currentSharedMesh = GetComponent<MeshFilter>().sharedMesh;
            // MeshData.Vertices.AddRange(currentSharedMesh.vertices);
            // MeshData.Indices.AddRange(currentSharedMesh.GetIndices(0));
            // MeshData.Texcoords.AddRange(currentSharedMesh.uv);
            // 3. Bind the OnMeshRebuild.
            Call_OnMeshRebuild += Caller_OnMeshRebuild;
        }

        void OnValidate() => OnShapeChanged();

        void OnDisable() => Call_OnMeshRebuild -= Caller_OnMeshRebuild;

        protected virtual void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                    out DanbiOpticalData opticalData,
                                                    out DanbiHalfsphereData shapeTransform)
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;
            // var reflectorMesh = MeshData;

            data.Vertices.AddRange(mesh.vertices);
            data.Texcoords.AddRange(mesh.uv);

            int previousVertexCount = data.Vertices.Count;
            int previousIndexCount = data.Indices.Count;
            data.Indices.AddRange(mesh.GetIndices(0).Select(i => i + previousVertexCount));

            ShapeTransform.indexOffset = previousIndexCount;
            ShapeTransform.indexCount = mesh.GetIndices(0).Length;
            ShapeTransform.local2World = transform.localToWorldMatrix;

            opticalData = default;
            //opticalData = OpticalData;
            shapeTransform = ShapeTransform;
        }

        protected virtual void OnShapeChanged() { }

        public virtual void PrintMeshInfo()
        {
            Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VertexCount}, Indices Count : {MeshData.IndexCount}, UV Count : {MeshData.TexcoordsCount} >>", this);
        }
    };
};
