using UnityEngine;

[System.Serializable]
public struct MeshOpticalProperty {
  public Vector3 albedo;
  public Vector3 specular;
  public float smoothness;
  public Vector3 emission;
}; // 4 * 3 * 4 = 48 size

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RayTracingObject : MonoBehaviour {
  public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty() {
    albedo = new Vector3(0.9f, 0.9f, 0.9f),
    specular = new Vector3(0.1f, 0.1f, 0.1f),
    smoothness = 0.9f,
    emission = new Vector3(0.0f, 0.0f, 0.0f)
  };

  private void OnEnable() {
    RayTracingMaster.RegisterObject(this);
  }

  private void OnDisable() {
    RayTracingMaster.UnregisterObject(this);
  }
}