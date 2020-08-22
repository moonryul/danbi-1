using UnityEngine;
public struct HemisphereMirror {
  public Matrix4x4 localToWorldMatrix;
  public float distanceToOrigin;
  public float height;
  public float radius;
  public Vector3 albedo;
  public Vector3 specular;
  public float smoothness;
  public Vector3 emission;
  // public int indices_offset;
  //public int indices_count;
};