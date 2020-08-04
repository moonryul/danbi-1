using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  /// <summary>
  /// 
  /// </summary>
  public class DanbiInitialSetRoomDetail : DanbiInitialDetail {
    [SerializeField]
    float RoomOpacity;
    Text Text_PlaceHolderRoomOpacity;
    InputField InputField_RoomOpacity;

    [SerializeField]
    Texture2D RoomTex;
    Text Text_PlaceHolderRoomTex;
    // TODO: Expose the current texture selection.
    // TODO: Decide the user can choose a texture on the explorer.


    protected override void Start() {
      base.Start();

      foreach (var i in GetComponentsInChildren<Text>()) {
        if (i.name.Contains("RoomOpacity")) {
          Text_PlaceHolderRoomOpacity = i;
        }

        if (i.name.Contains("RoomTex")) {
          Text_PlaceHolderRoomTex = i;
        }
      }

      foreach (var i in GetComponentsInChildren<InputField>()) {
        if (i.name.Contains("RoomOpacity")) {
          InputField_RoomOpacity = i;
        }
      }
    }
  };
};
