using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiManager : PremenantSingletonAsComponent<DanbiManager> {

    public static DanbiManager Instance => instance as DanbiManager;

    [SerializeField, Readonly]
    List<DanbiUIPanelControl> DetailsToSimulator = new List<DanbiUIPanelControl>();
    
    // TODO: Hold the trans-scene data (not decided yet).
    

  };
};
