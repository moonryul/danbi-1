using UnityEngine;

namespace Danbi {
  public class DanbiBaseShape : MonoBehaviour {

    protected Camera MainCamRef;

    [SerializeField, Readonly] protected DanbiMeshData MeshData;
    public DanbiMeshData getMeshData => MeshData;

    [SerializeField] protected DanbiOpticalData OpticalData;
    public DanbiOpticalData getOpticalData => OpticalData;

    [SerializeField] protected string ShapeName;
    public string getShapeName => ShapeName;


    public delegate void OnShapeChanged();
    /// <summary>
    /// Callback which is bind when the shape is changed. it's mainly happened OnValidate().
    /// </summary>
    public OnShapeChanged Call_ShapeChanged;

    protected virtual void Start() {
      /*
       *  1. Initialise resources.
       **/
      MainCamRef = Camera.main;

      Call_ShapeChanged += Caller_CustomShapeChanged;
      
      MeshData = new DanbiMeshData {
        VerticesCount = 0u,
        IndicesCount = 0u,
        uvCount = 0u
      };

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
      Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VerticesCount}, Indices Count : {MeshData.IndicesCount}, UV Count : {MeshData.uvCount} >>", this);
    }
  };
};
