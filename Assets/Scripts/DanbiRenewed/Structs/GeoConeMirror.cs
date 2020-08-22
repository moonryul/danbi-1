using UnityEngine;
public struct GeoConeMirror {
  public Matrix4x4 localToWorldMatrix; // the world frame of the cone
  public float distanceToOrigin;
  public Vector3 albedo;
  public Vector3 specular;
  public float smoothness;
  public Vector3 emission;
  public float height;
  public float radius;  // the radius of the base of the cone
};