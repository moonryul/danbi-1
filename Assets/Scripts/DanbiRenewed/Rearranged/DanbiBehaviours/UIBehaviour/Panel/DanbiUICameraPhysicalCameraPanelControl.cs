using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUICameraPhysicalCameraPanelControl : DanbiUIPanelControl
    {
        public float FocalLength { get; set; }
        public (float width, float height) SensorSize;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // bind the focal length.
            var focalLengthInputField = panel.GetChild(2).GetComponent<InputField>();
            focalLengthInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        FocalLength = asFloat;
                    }
                }
            );

            // bind the width of the sensor size.
            var sensorSizeWidthInputField = panel.GetChild(4).GetComponent<InputField>();
            sensorSizeWidthInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        SensorSize.width = asFloat;
                    }
                }
            );

            // bind the height of the sensor size.
            var sensorSizeHeightInputField = panel.GetChild(5).GetComponent<InputField>();
            sensorSizeHeightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        SensorSize.height = asFloat;
                    }
                }
            );

            var physicalCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            physicalCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    // Keep on listening the toggle is turned on and off
                    // and apply for the each inputfields.
                    if (isOn)
                    {
                        focalLengthInputField.ActivateInputField();
                        focalLengthInputField.interactable = true;
                        
                        sensorSizeWidthInputField.ActivateInputField();
                        sensorSizeWidthInputField.interactable = true;

                        sensorSizeHeightInputField.ActivateInputField();
                        sensorSizeHeightInputField.interactable = true;
                    }
                    else
                    {
                        focalLengthInputField.DeactivateInputField();
                        focalLengthInputField.interactable = false;

                        sensorSizeWidthInputField.DeactivateInputField();
                        sensorSizeWidthInputField.interactable = false;

                        sensorSizeHeightInputField.DeactivateInputField();
                        sensorSizeHeightInputField.interactable = false;
                    }
                }
            );
        }
    };
};
