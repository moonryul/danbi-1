using UnityEngine;

[System.Serializable]
public struct CameraParams {
  public Vector3 RadialCoefficient;  // 4 * 3 = 12
  public Vector2 TangentialCoefficient; // 4 * 2 = 8
  public Vector2 CentralPoint; // 4 * 2 = 8
  public Vector2 FocalLength; // 4 * 2 = 8
  public float SkewCoefficient;  // 4 
}; // 12 + 8 + 8 + 8 + 4 = 40