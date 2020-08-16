using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiManager : MonoBehaviour {
    
    public static DanbiManager Inst { get; internal set; }

    [SerializeField, Readonly]
    List<DanbiInitialDetail> DetailsToSimulator = new List<DanbiInitialDetail>();

    void Awake() {
      if (Inst.Null()) {
        Inst = this;
        DontDestroyOnLoad(Inst);
      } else {
        //
      }
    }

    public void RegisterUnloadForSimulator(DanbiInitialDetail detail) {
      if (detail.Null()) {
        Debug.LogError($"DanbiDetail isn't valid somehow!", this);
      }

      DetailsToSimulator.Add(detail);
    }
    // TODO: Hold the trans-scene data (not decided yet).
    

  };
};
