using System.Collections.Generic;

namespace Danbi {
  public static class DanbiKernelHelper {
    public static Dictionary<Danbi.EDanbiKernelKey, int> KernalDic { get; set; }
    public static Danbi.EDanbiKernelKey CurrentKernalKey { get; set; }

    public static int CurrentKernelIndex { get; set; }

    static DanbiKernelHelper() {
      KernalDic = new Dictionary<Danbi.EDanbiKernelKey, int>();
      CurrentKernalKey = Danbi.EDanbiKernelKey.None;
    }

    public static void AddKernalIndexWithKey(Danbi.EDanbiKernelKey key, int kernalIndex) {
      KernalDic.Add(key, kernalIndex);
    }

    public static void AddKernalIndexWithKey(params (Danbi.EDanbiKernelKey, int)[] keyKernalIndexPair) {
      foreach (var e in keyKernalIndexPair) {
        KernalDic.Add(e.Item1, e.Item2);
      }
    }

    public static int GetKernalIndex(Danbi.EDanbiKernelKey key) {
      return KernalDic[key];
    }
  };

};