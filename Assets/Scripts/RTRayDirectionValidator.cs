using UnityEngine;

public class RTRayDirectionValidator : MonoBehaviour {
  [System.Serializable]
  public struct RayInfo {
    Vector3 origin;
    Vector3 direction;
  };

  public float Length = 5.0f;

  public ComputeShader RayTracerRef;

  void Start() {
    
  }
};
