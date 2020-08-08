using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiManager : MonoBehaviour {
    
    public static DanbiManager Inst { get; internal set; }
    void Reset() {
      if (Inst.Null()) {
        Inst = this;
        DontDestroyOnLoad(Inst);
      } else {
        //
      }
    }


  };
};
