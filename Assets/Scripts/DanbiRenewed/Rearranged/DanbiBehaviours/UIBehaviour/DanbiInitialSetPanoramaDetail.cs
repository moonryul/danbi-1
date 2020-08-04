using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  /// <summary>
  /// 
  /// </summary>
  public class DanbiInitialSetPanoramaDetail : DanbiInitialDetail {
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
      base.Start();

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

      // 3. Populate the panorama dropdown.
      Dropdown_PanoramaType.AddOptions(PanoramaTypeContents);
    }
  };
};
