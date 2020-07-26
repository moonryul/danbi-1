using System;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIControl : MonoBehaviour {
    [Readonly, SerializeField, Header("Used for the result name.")]
    InputField InputField_SaveFile;

    public InputField saveFile_InputField { get => InputField_SaveFile; set => InputField_SaveFile = value; }
    

    void Start() {
      InputField_SaveFile.onEndEdit.AddListener(OnSaveFile);
    }


    void OnSaveFile(string call) {
      if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
        DanbiControl_Internal.Call_SaveImage.Invoke();
      }
    }
  };

}; // namespace Danbi.
