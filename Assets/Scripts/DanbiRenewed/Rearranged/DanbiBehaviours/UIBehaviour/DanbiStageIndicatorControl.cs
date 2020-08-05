using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Danbi {
  public class DanbiStageIndicatorControl : MonoBehaviour {
    [SerializeField]
    RectTransform Indicator;

    [SerializeField, Readonly]
    List<RectTransform> Positions = new List<RectTransform>();

    int CurrentPositionIndex = 0;

    public delegate void OnStageMoved(EDanbiIndicatorMoveDirection direction);
    public OnStageMoved Call_OnStageMoved;

    void Start() {
      var rts = GetComponentsInChildren<RectTransform>();
      for (int i = 0; i < rts.Length; ++i) {
        if (rts[i].CompareTag("Indicator Tag")) {
          Positions.Add(rts[i]);
        }
      }
      Call_OnStageMoved += Caller_OnStageMoved;
    }

    void Caller_OnStageMoved(EDanbiIndicatorMoveDirection direction) {
      switch (direction) {
        case EDanbiIndicatorMoveDirection.Left:
          CurrentPositionIndex = Mathf.Max(0, CurrentPositionIndex - 1);
          break;

        case EDanbiIndicatorMoveDirection.Right:          
          CurrentPositionIndex = Mathf.Min(Positions.Count - 1, CurrentPositionIndex + 1);
          break;
      }
      Indicator.localPosition = Positions[CurrentPositionIndex].localPosition;
    }
  };
};

