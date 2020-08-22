using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

//using DanbiMeshData1 = System.ValueTuple<System.Collections.Generic.List<Danbi.Float3>/*Vertices*/,
//                                        int/*VertexCount*/,
//                                        System.Collections.Generic.List<Danbi.Float3>/*Indices*/,
//                                        uint/*IndexCount*/,
//                                        uint/*IndexOffset*/>;

namespace Danbi {
  public class DanbiBaseShape : MonoBehaviour {
    [SerializeField, Readonly]
    protected DanbiMeshData MeshData;

    public DanbiMeshData meshData => MeshData;

    [SerializeField]
    protected DanbiOpticalData OpticalData;
    public DanbiOpticalData opticalData => OpticalData;    

    [SerializeField]
    protected string ShapeName;
    public string getShapeName => ShapeName;

    public delegate void OnMeshRebuild(ref POD_MeshData data,
                                       out AdditionalData additionalData);
    /// <summary>
    /// Callback which is called when the mesh is rebuilt.
    /// </summary>
    public OnMeshRebuild Call_OnMeshRebuild;

    protected virtual void Start() {      
      // 1. Initialise the Optical Data.
      OpticalData = new DanbiOpticalData {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = Vector3.zero
      };

      // 2. Intialise the Mesh Data.
      var currentSharedMesh = GetComponent<MeshFilter>().sharedMesh;
      MeshData = new DanbiMeshData {
        Vertices = new List<Vector3>(),
        VertexCount = currentSharedMesh.vertexCount,
        Indices = new List<int>(),
        IndexCount = currentSharedMesh.GetIndexCount(0),
        IndexOffset = 0u,
        Texcoords = new List<Vector2>(currentSharedMesh.uv),
        TexcoordsCount = 0,
      };
      MeshData.Vertices.AddRange(currentSharedMesh.vertices);
      MeshData.Indices.AddRange(currentSharedMesh.GetIndices(0));

      // 3. Bind the OnMeshRebuild.
      Call_OnMeshRebuild += Caller_OnMeshRebuild;
    }

    void OnValidate() => OnShapeChanged();

    void OnDisable() => Call_OnMeshRebuild -= Caller_OnMeshRebuild;    

    protected virtual void Caller_OnMeshRebuild(ref POD_MeshData data,
                                                out AdditionalData additionalData) {
      var reflectorMesh = meshData;
      int previousVertexCount = data.vertices.Count;

      data.vertices.AddRange(reflectorMesh.Vertices);
      data.texcoords.AddRange(reflectorMesh.Texcoords);

      int previousIndexCount = data.indices.Count;
      data.indices.AddRange(data.indices.Select(i => i + previousVertexCount));

      
      data.indices_offsets.Add(previousIndexCount);
      data.indices_counts.Add(data.indices.Count);
      additionalData = default;
    }

    protected virtual void OnShapeChanged() {
      // Implemented in inherited class.
    }

    public virtual void PrintMeshInfo() {
      Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VertexCount}, Indices Count : {MeshData.IndexCount}, UV Count : {MeshData.TexcoordsCount} >>", this);
    }
  };
};
