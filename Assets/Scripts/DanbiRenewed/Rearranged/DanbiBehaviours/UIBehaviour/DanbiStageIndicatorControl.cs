using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiStageIndicatorControl : MonoBehaviour {
    [SerializeField, Readonly]
    RectTransform Indicator;

    [SerializeField, Readonly]
    RectTransform[] Positions;

    int CurrentPositionIndex = 0;

    public delegate void OnStageMoved(EDanbiIndicatorMoveDirection direction);
    public static OnStageMoved Call_OnStageMoved;

    void Start() {
      var rts = GetComponentsInChildren<RectTransform>();
      Positions = new RectTransform[rts.Length];
      for (int i = 0; i < rts.Length; ++i) {
        if (rts[i].CompareTag("Indicator Tag")) {
          Positions[i] = rts[i];
        }
      }

      Call_OnStageMoved += Caller_OnStageMoved;
    }

    void Caller_OnStageMoved(EDanbiIndicatorMoveDirection direction) {
      switch (direction) {
        case EDanbiIndicatorMoveDirection.Left:
          CurrentPositionIndex = Mathf.Max(Positions.Length, CurrentPositionIndex + 1);
          break;

        case EDanbiIndicatorMoveDirection.Right:
          CurrentPositionIndex = Mathf.Min(0, CurrentPositionIndex - 1);
          break;
      }
      Indicator.localPosition = Positions[CurrentPositionIndex].localPosition;
    }
  };
};

