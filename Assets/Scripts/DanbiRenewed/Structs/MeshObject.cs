using UnityEngine;
public struct MeshObject {
  public Matrix4x4 localToWorldMatrix;
  public Vector3 albedo;
  public Vector3 specular;
  public float smoothness;
  public Vector3 emission;
  public int indices_offset;
  public int indices_count;
};