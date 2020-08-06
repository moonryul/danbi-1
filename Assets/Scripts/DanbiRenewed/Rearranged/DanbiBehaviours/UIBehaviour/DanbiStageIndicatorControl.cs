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
    List<RectTransform> PredefinedPositions = new List<RectTransform>();

    int CurrentPositionIndex = 0;    

    void Start() {
      // 1. Assign Indicator positions.
      foreach (var i in GetComponentsInChildren<RectTransform>()) {
        if (i.CompareTag("Indicator Tag")) {
          PredefinedPositions.Add(i);
        }
      }      
    }

    public void Caller_OnStageMoved(EDanbiIndicatorMoveDirection direction) {
      // 1. Decide the direction!
      switch (direction) {
        case EDanbiIndicatorMoveDirection.Left:
          CurrentPositionIndex = Mathf.Max(0, CurrentPositionIndex - 1);
          break;

        case EDanbiIndicatorMoveDirection.Right:
          CurrentPositionIndex = Mathf.Min(PredefinedPositions.Count - 1, CurrentPositionIndex + 1);
          break;
      }
      // 2. Move Indicator!
      Indicator.anchoredPosition = PredefinedPositions[CurrentPositionIndex].localPosition;
    }
  };
};

