using UnityEngine;
namespace Danbi {
  public class DanbiBaseShape : MonoBehaviour {

    protected Camera MainCamRef;
    protected Transform TfThisRef;

    public DanbiMeshData getMeshData => MeshData;
    public string shapeName { get => ShapeName; set => ShapeName = value; }

    [SerializeField] protected DanbiMeshData MeshData;
    [SerializeField] protected string ShapeName;

    public delegate void OnShapeChanged();
    public OnShapeChanged Evt_OnShapeChanged;

    protected virtual void Start() {
      MainCamRef = Camera.main;
      TfThisRef = transform;

      Evt_OnShapeChanged += OnCustomShapeChanged;
    }

    protected virtual void OnValidate() { Evt_OnShapeChanged.Invoke(); }

    protected virtual void OnDestroy() { Evt_OnShapeChanged -= OnCustomShapeChanged; }

    protected virtual void OnCustomShapeChanged() { /**/ }

    public virtual void PrintMeshInfo() {
      Debug.Log($"Mesh : {ShapeName} Info << Vertices Count : {MeshData.VerticesCount}, Indices Count : {MeshData.IndicesCount}, UV Count : {MeshData.uvCount} >>", this);
    }
  };
};
