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
        public float m_swipeAngle;
        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("InteractionSwipeToLeft-detectionSensitivity", m_swipeAngle);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevDetectionSensitivity = PlayerPrefs.GetFloat("InteractionSwipeToLeft-detectionSensitivity", default);
            m_swipeAngle = prevDetectionSensitivity;
            (uiElements[0] as TMP_InputField).text = m_swipeAngle.ToString();
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
                        m_swipeAngle = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(detectionSensitivityInputField);
        }
    };
};
