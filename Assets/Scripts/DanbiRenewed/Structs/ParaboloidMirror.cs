using UnityEngine;
public struct ParaboloidMirror {
  public Matrix4x4 localToWorldMatrix; // the world frame of the cone
  public float distanceToOrigin; // distance from the camera to the origin of the paraboloid
  public float height;
  public float notUseRatio; //
  public Vector3 albedo;
  public Vector3 specular;
  public float smoothness;
  public Vector3 emission;
  public float coefficientA;  // z = - ( x^2/a^2 + y^2/b^2)
  public float coefficientB;
};