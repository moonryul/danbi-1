using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiInitialSetReflectorDetail : DanbiInitialDetail {
    Dropdown Dropdown_ReflectorType;

    [SerializeField]
    List<string> ReflectorTypeContents = new List<string>();

    protected override void Start() {
      base.Start();

      foreach( var i in GetComponentsInChildren<Dropdown>()) {
        Dropdown_ReflectorType = i;
      }

      Dropdown_ReflectorType.AddOptions(ReflectorTypeContents);
    }
  };
};