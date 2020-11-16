using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionSwipeToLeftPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float detectionSensitivity;
        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("InteractionSwipeToLeft-detectionSensitivity", detectionSensitivity);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevDetectionSensitivity = PlayerPrefs.GetFloat("InteractionSwipeToLeft-detectionSensitivity", default);
            detectionSensitivity = prevDetectionSensitivity;
            (uiElements[0] as TMP_InputField).text = detectionSensitivity.ToString();
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
                        detectionSensitivity = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(detectionSensitivityInputField);
        }
    };
};
