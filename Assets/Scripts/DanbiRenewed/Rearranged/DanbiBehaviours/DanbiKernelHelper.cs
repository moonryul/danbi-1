using System.Collections;
using System.Collections.Generic;

namespace Danbi {
  [System.Serializable]
  public class DanbiKernelHelper {
    public Dictionary<Danbi.EDanbiKernelKey, int> KernalDic { get; set; }
    public Danbi.EDanbiKernelKey CurrentKernalKey { get; set; }

    public int CurrentKernelIndex { get; set; }

    public DanbiKernelHelper() {
      KernalDic = new Dictionary<Danbi.EDanbiKernelKey, int>();
      CurrentKernalKey = Danbi.EDanbiKernelKey.None;
    }

    public void AddKernalIndexWithKey(Danbi.EDanbiKernelKey key, int kernalIndex) {
      KernalDic.Add(key, kernalIndex);
    }

    public void AddKernalIndexWithKey(params (Danbi.EDanbiKernelKey, int)[] keyKernalIndexPair) {
      foreach (var e in keyKernalIndexPair) {
        KernalDic.Add(e.Item1, e.Item2);
      }
    }

    public int GetKernalIndex(Danbi.EDanbiKernelKey key) {
      return KernalDic[key];
    }
  };

};