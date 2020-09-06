using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  /// <summary>
  /// 
  /// </summary>
  public class DanbiUIPanoramaScreenElement : DanbiUIBaseElement {
    [SerializeField]
    float MaxHeight;
    Text Text_PlaceHolderMaxHeight;
    InputField InputField_MaxHeight;

    [SerializeField]
    float MinHeight;
    Text Text_PlaceHolderMinHeight;
    InputField InputField_MinHeight;


    Dropdown Dropdown_PanoramaType;

    [SerializeField]
    List<string> PanoramaTypeContents = new List<string>();

    void Start() {
      // 1. Assign UI.Text and UI.InputField automatically. 
      foreach (var i in GetComponentsInChildren<Text>()) {
        if (i.name.Contains("MaxHeight")) {
          Text_PlaceHolderMaxHeight = i;
        }

        if (i.name.Contains("MinHeight")) {
          Text_PlaceHolderMinHeight = i;
        }
      }

      foreach (var i in GetComponentsInChildren<InputField>()) {
        if (i.name.Contains("MaxHeight")) {
          InputField_MaxHeight = i;
        }

        if (i.name.Contains("MinHeight")) {
          InputField_MinHeight = i;
        }
      }

      // 2. Assign UI.dropdown automatically.
      foreach (var i in GetComponentsInChildren<Dropdown>()) {
        Dropdown_PanoramaType = i;
      }

      // TODO: need to ensure which contents will be.
      if (PanoramaTypeContents.Count == 0) {
        PanoramaTypeContents.AddRange(new string[] {
          "Cylinder", "Cube"
        });
      }

      // 3. Populate the panorama dropdown.
      Dropdown_PanoramaType.AddOptions(PanoramaTypeContents);
      Dropdown_PanoramaType.onValueChanged.AddListener(Caller_OnOptionChanged);
    }

    void Caller_OnOptionChanged(int newOption) {
      switch (newOption) {
        default:
          Debug.Log(string.Format("{0}", newOption));
          break;
      }
    }
  };
};
