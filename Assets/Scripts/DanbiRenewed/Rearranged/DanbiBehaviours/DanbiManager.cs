using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiManager : PremenantSingletonAsComponent<DanbiManager> {

    public static DanbiManager Instance => abstractInstance as DanbiManager;

    [SerializeField, Readonly]
    List<DanbiUIBaseSubmenu> DetailsToSimulator = new List<DanbiUIBaseSubmenu>();

    public void RegisterUnloadForSimulator(DanbiUIBaseSubmenu detail) {
      if (detail.Null()) {
        Debug.LogError($"DanbiDetail isn't valid somehow!", this);
      }

      DetailsToSimulator.Add(detail);
    }
    // TODO: Hold the trans-scene data (not decided yet).
    

  };
};
