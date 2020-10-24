using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionWalkingPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float DetectionIntensity;
        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("InteractionWalking-DetectionIntensity", DetectionIntensity);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevDetectionIntensity = PlayerPrefs.GetFloat("InteractionWalking-DetectionIntensity", default);
            DetectionIntensity = prevDetectionIntensity;
            (uiElements[0] as TMP_InputField).text = DetectionIntensity.ToString();
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
                        DetectionIntensity = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(detectionIntensityInputField);
        }
    };
};
