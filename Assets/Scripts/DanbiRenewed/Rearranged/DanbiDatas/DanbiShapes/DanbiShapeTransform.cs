using System.Numerics;

namespace Danbi {
  [System.Serializable]
  public class DanbiShapeTransform {
    public float Distance; // 4
    public float Height; // 4
    public float Radius; // 4
    public float MaskingRatio; // 4
    public UnityEngine.Matrix4x4 local2World; // 64
  }; // 80
};