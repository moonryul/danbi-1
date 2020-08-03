using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiInitialSetRoomDetail : DanbiInitialDetail {
    [SerializeField]
    float Height;
    InputField InputField_Height;

    [SerializeField]
    float Width;
    InputField InputFiled_Width;

    [SerializeField]
    float Depth;
    InputField InputField_Depth;

    [SerializeField]
    float StartingHeight;
    InputField InputField_StartingHeight;


    void Start() {
      //
    }
  };
};
