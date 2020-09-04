using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiManager : PremenantSingletonAsComponent<DanbiManager> {

    public static DanbiManager Instance => abstractInstance as DanbiManager;

    [SerializeField, Readonly]
    List<DanbiUIBaseElement> DetailsToSimulator = new List<DanbiUIBaseElement>();
    
    // TODO: Hold the trans-scene data (not decided yet).
    

  };
};
