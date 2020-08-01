using UnityEngine;

namespace Danbi {
  public class DanbiBaseShape : MonoBehaviour {

    protected Camera MainCamRef;

    [SerializeField, Readonly] 
    protected DanbiMeshData MeshData;
    public DanbiMeshData getMeshData => MeshData;

    [SerializeField] 
    protected DanbiOpticalData OpticalData;
    public DanbiOpticalData getOpticalData => OpticalData;

    /// <summary>
    /// You are responsible to initialise this on the child classes.
    /// </summary>
    [SerializeField]
    protected DanbiShapeTransform ShapeTransform;
    public DanbiShapeTransform shapeTransform => ShapeTransform;

    [SerializeField] protected string ShapeName;
    public string getShapeName => ShapeName;


    public delegate void OnShapeChanged();
    /// <summary>
    /// Callback which is bind when the shape is changed. it's mainly happened OnValidate().
    /// </summary>
    public OnShapeChanged Call_ShapeChanged;

    [SerializeField]
    DanbiPrewarperSetting Setting;

    protected virtual void Start() {
      /*
       *  1. Initialise resources.
       **/
      MainCamRef = Camera.main;

      Call_ShapeChanged += Caller_CustomShapeChanged;

      MeshData = new DanbiMeshData {
        Vertices = new System.Collections.Generic.List<Vector3>(),
        VertexCount = 0,
        Indices = new System.Collections.Generic.List<int>(),
        IndexCount = 0u,
        IndexOffset = 0u,
        Texcoords = null,
        TexcoordsCount = 0,
      };

      var currentSharedMesh = GetComponent<MeshFilter>().sharedMesh;
      MeshData.Vertices.AddRange(currentSharedMesh.vertices);
      MeshData.VertexCount = currentSharedMesh.vertexCount;
      MeshData.Indices.AddRange(currentSharedMesh.GetIndices(0));
      MeshData.IndexCount = currentSharedMesh.GetIndexCount(0);
      MeshData.Texcoords = new System.Collections.Generic.List<Vector2>(currentSharedMesh.uv);
      //MeshData.TexcoordsCount = currentSharedMesh.count


      OpticalData = new DanbiOpticalData {
        albedo = new Vector3(0.9f, 0.9f, 0.9f),
        specular = new Vector3(0.1f, 0.1f, 0.1f),
        smoothness = 0.9f,
        emission = Vector3.zero
      };      
    }

    protected virtual void OnValidate() { Call_ShapeChanged.Invoke(); }    

    protected virtual void Caller_CustomShapeChanged() { /**/ }

    public virtual void PrintMeshInfo() {
      Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VertexCount}, Indices Count : {MeshData.IndexCount}, UV Count : {MeshData.TexcoordsCount} >>", this);
    }
  };
};
