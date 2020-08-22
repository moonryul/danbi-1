namespace Danbi
{
  [System.Serializable]
  public struct DanbiCamAdditionalData
  {
    public UnityEngine.Vector3 RadialCoefficient;  // 4 * 3 = 12
    public UnityEngine.Vector2 TangentialCoefficient; // 4 * 2 = 8
    public UnityEngine.Vector2 PrincipalPoint; // 4 * 2 = 8
    public UnityEngine.Vector2 FocalLength; // 4 * 2 = 8
    public float SkewCoefficient;  // 4 

    public int stride => (4 * 3) + (4 * 2) +
                         (4 * 2) + (4 * 2) +
                         4; // 40
  }; // 12 + 8 + 8 + 8 + 4 = 40
};
