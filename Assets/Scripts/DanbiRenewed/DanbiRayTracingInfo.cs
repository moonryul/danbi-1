using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DanbiRayTracingInfo {
  public Dictionary<EDanbiCurrentKernalKey, int> KernalDic { get; set; }
  public EDanbiCurrentKernalKey CurrentKernalKey { get; set; }
  public DanbiRayTracingInfo() {
    KernalDic = new Dictionary<EDanbiCurrentKernalKey, int>();
    CurrentKernalKey = EDanbiCurrentKernalKey.None;
  }

  public void AddKernalIndexWithKey(EDanbiCurrentKernalKey key, int kernalIndex) {
    KernalDic.Add(key, kernalIndex);
  }

  public void AddKernalIndexWithKey(params (EDanbiCurrentKernalKey, int)[] keyKernalIndexPair) {
    foreach (var e in keyKernalIndexPair) {
      KernalDic.Add(e.Item1, e.Item2);
    }
  }

  public int GetKernalIndex(EDanbiCurrentKernalKey key) {
    return KernalDic[key];
  }
};
