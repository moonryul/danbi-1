using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIControl : MonoBehaviour {
    #region Exposed
    [Readonly, SerializeField, Header("Used for the result name.")]
    InputField InputField_SaveFile;

    [Readonly, SerializeField, Header("Used for creating the result.")]
    Button Button_CreateResult;

    [Readonly, SerializeField, Header("Used for moving the indicator to the next")]
    Button Button_MoveToNext;

    [Readonly, SerializeField, Header("Used for moving the indicator to the previous")]
    Button Button_MoveToPrevious;
    
    #endregion Exposed

    #region Internal            

    DanbiStageIndicatorControl IndicatorControl;

    #endregion Internal

    #region Delegate

    delegate void OnStageMoved(EDanbiIndicatorMoveDirection direction);
    event OnStageMoved Call_OnStageMoved;        

    #endregion Delegate

    //void Awake() {      
    //  DontDestroyOnLoad(this);      
    //}

    //void OnLevelWasLoaded(int level) {
    //  if (level == 1) {
    //    Button_MoveToNext = null;
    //    Button_MoveToPrevious = null;
    //    IndicatorControl = null;

    //    // TODO: Bind the Create Result.
    //    // TOOD: Bind the Save File.
    //    //Button_CreateResult.onClick.AddListener(DanbiControl.UnityEvent_CreatePredistortedImage);
    //    //InputField_SaveFile.onEndEdit.AddListener(DanbiControl.UnityEvent_SaveImageAt);
    //  }
    //}

    void Start() {

      // 1. Acquire the resources of the UI control buttons.

      // if button isn't manually assigned.

      Button_MoveToNext = GameObject.Find("Next (Button)").GetComponent<Button>();
      Button_MoveToPrevious = GameObject.Find("Previous (Button)").GetComponent<Button>();

      // Assign Indicator Control Automatically
      IndicatorControl = GameObject.FindObjectOfType<DanbiStageIndicatorControl>();

      // 2. Bind the listeners.
      Button_MoveToNext.onClick.AddListener(this.OnMoveToNextSetting);
      Button_MoveToPrevious.onClick.AddListener(this.OnMoveToPreviousSetting);

      Call_OnStageMoved += IndicatorControl.Caller_OnStageMoved;
    }

    void Update() {
      if (Input.GetKeyDown(KeyCode.RightArrow)) {
        OnMoveToNextSetting();
      }

      if (Input.GetKeyDown(KeyCode.LeftArrow)) {
        OnMoveToPreviousSetting();
      }
    }

    public void OnMoveToNextSetting() => Call_OnStageMoved?.Invoke(EDanbiIndicatorMoveDirection.Right);

    public void OnMoveToPreviousSetting() => Call_OnStageMoved?.Invoke(EDanbiIndicatorMoveDirection.Left);

    /// <summary>
    /// Called when the Save File button clicked!
    /// </summary>
    /// <param name="call"></param>
    void OnSaveFile(string call) {
      // TODO: 윈도우즈 익스플로러를 연결하여 사용해야함.
      if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
        DanbiControl.Call_OnSaveImage?.Invoke();
      }
    }

    void OnCreateResult() {

    }
  };

}; // namespace Danbi.
