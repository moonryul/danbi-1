using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionSwipeLeftToRightPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float detectionIntensity;

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("InteractionSwipeLeftToRight-detectionIntensity", detectionIntensity);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevDetectionIntensity = PlayerPrefs.GetFloat("InteractionSwipeLeftToRight-detectionIntensity", default);
            detectionIntensity = prevDetectionIntensity;
            (uiElements[0] as TMP_InputField).text = detectionIntensity.ToString();
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var detectionIntensityInputField = panel.GetChild(0).GetComponent<TMP_InputField>();
            detectionIntensityInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        detectionIntensity = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(detectionIntensityInputField);
        }
    };
};
