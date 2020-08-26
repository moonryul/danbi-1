using UnityEngine;
namespace Danbi {
  [System.Serializable]
  public class DanbiPanoramaMeshShapeTransform : DanbiShapeTransform {
    public float high;
    public float low;

    public new int stride => base.stride + 4 + 4;
  };
};
