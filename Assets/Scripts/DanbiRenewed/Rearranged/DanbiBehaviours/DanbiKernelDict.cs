using System.Collections.Generic;

namespace Danbi {
  public static class DanbiKernelDict {    
    public static Dictionary<EDanbiKernelKey, int> KernalDic { get; set; }    
    public static EDanbiKernelKey CurrentKernalKey { get; set; }

    public static int CurrentKernelIndex { get; set; }

    static DanbiKernelDict() {
      KernalDic = new Dictionary<EDanbiKernelKey, int>();
      //CurrentKernalKey = "";
    }

    public static void AddKernalIndexWithKey(EDanbiKernelKey key, int kernalIndex) {
      KernalDic.Add(key, kernalIndex);
    }    

    public static void AddKernalIndexWithKey(params (EDanbiKernelKey, int)[] keyKernalIndexPair) {
      foreach (var e in keyKernalIndexPair) {
        KernalDic.Add(e.Item1, e.Item2);
      }
    }

    public static int GetKernalIndex(EDanbiKernelKey key) {
      return KernalDic[key];
    }
  };

};