using System;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIControl : MonoBehaviour {
    [Readonly, SerializeField, Header("Used for the result name.")]
    InputField InputField_SaveFile;
    public InputField InputField_saveFile { get => InputField_SaveFile; set => InputField_SaveFile = value; }

    [Readonly, SerializeField, Header("Used for creating the result.")]
    Button Button_CreateResult;
    public Button Button_createResult { get => Button_CreateResult; set => Button_CreateResult = value; }

    [SerializeField]
    Button Button_MoveToNext;
    public Button button_MoveToNext { get => Button_MoveToNext; set => Button_MoveToNext = value; }

    [SerializeField]
    Button Button_MoveToPrevious;
    public Button button_MoveToPrevious { get => Button_MoveToPrevious; set => Button_MoveToPrevious = value; }

    [SerializeField, Header("Pluggable Detail Canvas.")]
    DanbiInitialDetail Detail;

    DanbiStageIndicatorControl IndicatorControl;
    public DanbiInitialDetail changeDetail { set => Detail = value; }

    void Start() {
      // 1. Bind the button callers.
      //InputField_SaveFile.onEndEdit.AddListener(OnSaveFile);
      //Button_CreateResult.onClick.AddListener(OnCreateResult);

      // if button isn't manually assigned.
      foreach (var i in GetComponentsInChildren<Button>()) {
        if (i.name.Contains("Next")) {
          Button_MoveToNext = i;
        }

        if (i.name.Contains("Previous")) {
          Button_MoveToPrevious = i;
        }
      }

      Button_MoveToNext.onClick.AddListener(this.OnMoveToNextSetting);
      Button_MoveToPrevious.onClick.AddListener(this.OnMoveToPreviousSetting);

      // Assign Indicator Control Automatically
      foreach (var i in GetComponentsInChildren<DanbiStageIndicatorControl>()) {
        IndicatorControl = i;
      }
    }

    void Update() {
      if (Input.GetKeyDown(KeyCode.RightArrow)) {
        OnMoveToNextSetting();
      }

      if (Input.GetKeyDown(KeyCode.LeftArrow)) {
        OnMoveToPreviousSetting();
      }
    }

    public void OnMoveToNextSetting() {
      IndicatorControl.Call_OnStageMoved(EDanbiIndicatorMoveDirection.Right);
      Debug.Log("Move to Next!");
    }

    public void OnMoveToPreviousSetting() {
      IndicatorControl.Call_OnStageMoved(EDanbiIndicatorMoveDirection.Left);
      Debug.Log("Move to Previous!");
    }

    /// <summary>
    /// Called when the Save File button clicked!
    /// </summary>
    /// <param name="call"></param>
    void OnSaveFile(string call) {
      // TODO: 윈도우즈 익스플로러를 연결하여 사용해야함.
      if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
        DanbiControl_Internal.Call_SaveImage.Invoke();
      }
    }

    void OnCreateResult() {

    }
  };

}; // namespace Danbi.
