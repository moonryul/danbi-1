using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  /// <summary>
  /// 
  /// </summary>
  public class DanbiRoomSubmenu : DanbiUIBaseSubmenu {
    [SerializeField]
    protected float Height;
    protected Text Text_PlaceHolderHeight;
    protected InputField InputField_Height;

    [SerializeField]
    protected float Width;
    protected Text Text_PlaceHolderWidth;
    protected InputField InputFiled_Width;

    [SerializeField]
    protected float Depth;
    protected Text Text_PlaceHolderDepth;
    protected InputField InputField_Depth;

    [SerializeField]
    protected float StartingHeight;
    protected Text Text_PlaceHolderStartingHeight;
    protected InputField InputField_StartingHeight;

    [SerializeField]
    protected float TextureTilingPower;
    protected Text Text_PlaceHolderTextureTilingPower;
    protected InputField InputField_TextureTilingPower;

    protected override void Start() {
      base.Start();

      // 1. Assign the UI.Text automatically.
      foreach (var i in GetComponentsInChildren<Text>()) {
        if (i.name.Contains("Height")) {
          Text_PlaceHolderHeight = i;
        }

        if (i.name.Contains("Width")) {
          Text_PlaceHolderWidth = i;
        }

        if (i.name.Contains("Depth")) {
          Text_PlaceHolderDepth = i;
        }

        if (i.name.Contains("StartingHeight")) {
          Text_PlaceHolderStartingHeight = i;
        }
      }

      // 2. Assign the UI.InputField automatically.
      foreach (var i in GetComponentsInChildren<InputField>()) {
        if (i.name.Contains("Height")) {
          InputField_Height = i;
        }

        if (i.name.Contains("Width")) {
          InputFiled_Width = i;
        }

        if (i.name.Contains("Depth")) {
          InputField_Depth = i;
        }

        if (i.name.Contains("StartingHeight")) {
          InputField_StartingHeight = i;
        }
      }
    }

    public override void OnMenuButtonSelected() {

    }
  };
};
