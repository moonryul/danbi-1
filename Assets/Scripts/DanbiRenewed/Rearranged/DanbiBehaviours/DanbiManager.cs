using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiManager : PremenantSingletonAsComponent<DanbiManager> {

    public static DanbiManager Instance => abstractInstance as DanbiManager;

    [SerializeField, Readonly]
    List<DanbiIBaseSubmenu> DetailsToSimulator = new List<DanbiIBaseSubmenu>();
    
    // TODO: Hold the trans-scene data (not decided yet).
    

  };
};
