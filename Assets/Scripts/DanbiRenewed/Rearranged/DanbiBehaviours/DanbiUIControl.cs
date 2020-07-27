using System;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIControl : MonoBehaviour {
    [Readonly, SerializeField, Header("Used for the result name.")]
    InputField InputField_SaveFile;
    public InputField saveFile_InputField { get => InputField_SaveFile; set => InputField_SaveFile = value; }

    [Readonly, SerializeField, Header("Used for creating the result.")]
    Button Button_CreateResult;
    public Button createResult_Button { get => Button_CreateResult; set => Button_CreateResult = value; }


    void Start() {
      InputField_SaveFile.onEndEdit.AddListener(OnSaveFile);
      Button_CreateResult.onClick.AddListener(OnCreateResult);
    }


    void OnSaveFile(string call) {
      if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
        DanbiControl_Internal.Call_SaveImage.Invoke();
      }
    }

    void OnCreateResult() {

    }    
  };

}; // namespace Danbi.
