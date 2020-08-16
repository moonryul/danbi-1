using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Danbi {
  public class DanbiStageIndicatorControl : MonoBehaviour {
    [SerializeField, Readonly]
    Text IndicatorTitle;

    [SerializeField, Readonly]
    string[] PredefinedTitles;

    [SerializeField, Readonly]
    RectTransform Indicator;

    [SerializeField, Readonly]
    List<RectTransform> PredefinedPositions = new List<RectTransform>();

    int CurrentPositionIndex = 0;

    void Start() {
      // 1. Change the name holder.
      PredefinedTitles = new string[5];
      PredefinedTitles[0] = "Room Detail";
      PredefinedTitles[1] = "Panorama Detail";
      PredefinedTitles[2] = "Reflector Detail";
      PredefinedTitles[3] = "Camera Detail";
      PredefinedTitles[4] = "Final Detail";


      IndicatorTitle = GameObject.Find("Indicator Title (Text)").GetComponent<Text>();
      IndicatorTitle.text = PredefinedTitles[0];

      // 2. Assign Indicator positions.
      foreach (var i in GetComponentsInChildren<RectTransform>()) {
        if (i.CompareTag("Indicator Tag")) {
          PredefinedPositions.Add(i);
        }
      }

      Indicator = GameObject.Find("Indicator (Panel)").GetComponent<RectTransform>();

      Indicator.anchoredPosition = PredefinedPositions[0].anchoredPosition;
    }

    public void Caller_OnStageMoved(EDanbiIndicatorMoveDirection direction) {
      // 1. Decide the direction!
      switch (direction) {
        case EDanbiIndicatorMoveDirection.Left:
          CurrentPositionIndex = Mathf.Max(0, CurrentPositionIndex - 1);
          break;

        case EDanbiIndicatorMoveDirection.Right:
          CurrentPositionIndex = Mathf.Min(CurrentPositionIndex + 1, PredefinedPositions.Count - 1);
          break;
      }
      Debug.Log($"Current Indicator Position Index : {CurrentPositionIndex}", this);
      // 2. Move Indicator and detail!
      //DanbiUIDetailControl.Call_OnDetailMove(CurrentPositionIndex);
      Indicator.anchoredPosition = PredefinedPositions[CurrentPositionIndex].anchoredPosition;
      IndicatorTitle.text = PredefinedTitles[CurrentPositionIndex];
    }
  };
};

