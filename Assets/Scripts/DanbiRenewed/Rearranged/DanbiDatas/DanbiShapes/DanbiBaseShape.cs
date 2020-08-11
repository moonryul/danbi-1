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

    protected Camera MainCamRef;

    [SerializeField, Readonly]
    protected DanbiMeshData MeshData;

    public DanbiMeshData meshData => MeshData;

    [SerializeField]
    protected DanbiOpticalData OpticalData;
    public DanbiOpticalData opticalData => OpticalData;

    /// <summary>
    /// You are responsible to initialize this on the child classes.
    /// </summary>
    [SerializeField]
    protected DanbiShapeTransform ShapeTransform;
    public DanbiShapeTransform shapeTransform => ShapeTransform;

    [SerializeField]
    protected string ShapeName;
    public string getShapeName => ShapeName;

    public delegate void OnMeshRebuild(ref DanbiComputeShaderControl.POD_MeshData data,
                                       out AdditionalData additionalData);
    /// <summary>
    /// Callback which is called when the mesh is rebuilt.
    /// </summary>
    public OnMeshRebuild Call_OnMeshRebuild;    

    protected virtual void Start() {
      MeshData = new DanbiMeshData {
        Vertices = new List<Vector3>(),
        VertexCount = 0,
        Indices = new List<int>(),
        IndexCount = 0u,
        IndexOffset = 0u,
        Texcoords = null,
        TexcoordsCount = 0,
      };

      OpticalData = new DanbiOpticalData {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = Vector3.zero
      };

      MainCamRef = Camera.main;

      var currentSharedMesh = GetComponent<MeshFilter>().sharedMesh;
      MeshData.Vertices.AddRange(currentSharedMesh.vertices);
      MeshData.VertexCount = currentSharedMesh.vertexCount;
      MeshData.Indices.AddRange(currentSharedMesh.GetIndices(0));
      MeshData.IndexCount = currentSharedMesh.GetIndexCount(0);
      MeshData.Texcoords = new List<Vector2>(currentSharedMesh.uv);

      Call_OnMeshRebuild += Caller_OnMeshRebuild;
    }

    protected virtual void OnValidate() => OnShapeChanged();

    protected virtual void OnDisable() {
      Call_OnMeshRebuild -= Caller_OnMeshRebuild;
    }

    protected virtual void Caller_OnMeshRebuild(ref DanbiComputeShaderControl.POD_MeshData data,
                                                out AdditionalData additionalData) {
      var reflectorMesh = meshData;
      int previousVertexCount = data.vertices.Count;

      data.vertices.AddRange(reflectorMesh.Vertices);
      data.texcoords.AddRange(reflectorMesh.Texcoords);

      int previousIndexCount = data.indices.Count;
      data.indices.AddRange(data.indices.Select(i => i + previousVertexCount));

      shapeTransform.local2World = transform.localToWorldMatrix;
      data.indices_offsets.Add(previousIndexCount);
      data.indices_counts.Add(data.indices.Count);

      additionalData = new AdditionalData(opticalData, shapeTransform);
    }

    protected virtual void OnShapeChanged() { /**/ }

    public virtual void PrintMeshInfo() {
      Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VertexCount}, Indices Count : {MeshData.IndexCount}, UV Count : {MeshData.TexcoordsCount} >>", this);
    }
  };
};
