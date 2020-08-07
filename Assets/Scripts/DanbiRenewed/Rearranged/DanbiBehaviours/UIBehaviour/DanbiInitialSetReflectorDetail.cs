using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiInitialSetReflectorDetail : DanbiInitialDetail {
    /// <summary>
    /// TODO: make dropdown to responsive by the each item!
    /// </summary>
    Dropdown Dropdown_ReflectorType;

    [SerializeField]
    List<string> ReflectorTypeContents = new List<string>();

    Dictionary<string, List<GameObject>> UIelementSetsByReflectorType = new Dictionary<string, List<GameObject>>();    

    protected override void Start() {
      base.Start();

      // 1. Assign the dropdown automatically.
      foreach (var i in GetComponentsInChildren<Dropdown>()) {
        Dropdown_ReflectorType = i;
      }

      // 2. populate the contents in any case to prevent the null-ref exception.
      // TODO: need to ensure which contents will be.
      if (ReflectorTypeContents.Count == 0) {
        ReflectorTypeContents.AddRange(new string[] {
          "Halfsphere", "Cone", "UFO Halfsphere"
        });
      }

      // 3. Populate the options of dropdown.
      Dropdown_ReflectorType.AddOptions(ReflectorTypeContents);
      /**
       * onValueChanged -> int : return value correspond for each option in a row.
       */
      Dropdown_ReflectorType.onValueChanged.AddListener(Caller_OnOptionChanged);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="newOption"></param>
    void Caller_OnOptionChanged(int newOption) {
      switch (newOption) {
        case 0:

        default:
          Debug.Log($"Uncatched option-> {newOption}", this);
          break;
      }
    }

  };
};