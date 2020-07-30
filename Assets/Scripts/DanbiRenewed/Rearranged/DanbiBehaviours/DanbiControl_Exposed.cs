using System.Collections;
using System.Collections.Generic;

using DirectShowLib;

using UnityEngine;

namespace Danbi {
  [RequireComponent(typeof(DanbiUIControl))]
  public class DanbiControl_Exposed : MonoBehaviour {
    [SerializeField, Readonly]
    DanbiControl_Internal Control;

    [SerializeField, Readonly]
    DanbiUIControl UIControl;

    void Start() {
      Control.NullFinally(() => {
        Debug.LogError("Control must be assigned properly!", this);
      });

      Control = GetComponent<DanbiControl_Internal>();

      // 2. Bind the functions to UI.
      UIControl = GetComponent<DanbiUIControl>();
      UIControl.createResult_Button.onClick.AddListener(UnityEvent_CreatePredistortedImage);
      UIControl.saveFile_InputField.onEndEdit.AddListener(UnityEvent_SaveImageAt);
    }

    public void UnityEvent_CreatePredistortedImage() {
      DanbiControl_Internal.Call_RenderStated.Invoke(Control.targetPanoramaTex);
      
    }

    public void UnityEvent_SaveImageAt(string path) {
      DanbiControl_Internal.Call_SaveImage();
    }

  };
};
