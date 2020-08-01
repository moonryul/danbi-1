using System.Collections.Generic;

namespace Danbi {
  public static class DanbiKernelHelper {    
    public static Dictionary<string, int> KernalDic { get; set; }    
    public static string CurrentKernalKey { get; set; }

    public static int CurrentKernelIndex { get; set; }

    static DanbiKernelHelper() {
      KernalDic = new Dictionary<string, int>();
      CurrentKernalKey = "";
    }

    public static void AddKernalIndexWithKey(string key, int kernalIndex) {
      KernalDic.Add(key, kernalIndex);
    }    

    public static void AddKernalIndexWithKey(params (string, int)[] keyKernalIndexPair) {
      foreach (var e in keyKernalIndexPair) {
        KernalDic.Add(e.Item1, e.Item2);
      }
    }

    public static int GetKernalIndex(string key) {
      return KernalDic[key];
    }
  };

};