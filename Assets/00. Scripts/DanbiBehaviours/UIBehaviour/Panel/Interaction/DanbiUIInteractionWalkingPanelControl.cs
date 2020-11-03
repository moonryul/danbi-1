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
        public float DetectionSensitivity;
        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("InteractionWalking-DetectionSensitivity", DetectionSensitivity);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevDetectionSensitivity = PlayerPrefs.GetFloat("InteractionWalking-DetectionSensitivity", default);
            DetectionSensitivity = prevDetectionSensitivity;
            (uiElements[0] as TMP_InputField).text = DetectionSensitivity.ToString();
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var detectionSensitivityInputField = panel.GetChild(0).GetComponent<TMP_InputField>();
            detectionSensitivityInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        DetectionSensitivity = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(detectionSensitivityInputField);
        }
    };
};
