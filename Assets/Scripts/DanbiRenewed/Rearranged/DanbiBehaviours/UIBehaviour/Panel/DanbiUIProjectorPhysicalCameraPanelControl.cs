using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIProjectorPhysicalCameraPanelControl : DanbiUIPanelControl
    {
        public float FocalLength { get; set; }
        public (float width, float height) SensorSize;
        public (float horizontalFov, float verticalFov) FOV;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.ProjectorPhysicalCamera, this);

            var panel = Panel.transform;

            // bind the focal length.
            var focalLengthInputField = panel.GetChild(1).GetComponent<InputField>();
            focalLengthInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        FocalLength = asFloat;
                        CalculateFOV(panel);
                    }
                }
            );

            // bind the width of the sensor size.
            var sensorSizeWidthInputField = panel.GetChild(2).GetComponent<InputField>();
            sensorSizeWidthInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        SensorSize.width = asFloat;
                        CalculateFOV(panel);
                    }
                }
            );

            // bind the height of the sensor size.
            var sensorSizeHeightInputField = panel.GetChild(3).GetComponent<InputField>();
            sensorSizeHeightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        SensorSize.height = asFloat;
                        CalculateFOV(panel);
                    }
                }
            );

            // bind the physical camera toggle.
            var physicalCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            physicalCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    // Keep on listening the toggle is turned on and off
                    // and apply for the each inputfields.
                    if (isOn)
                    {
                        focalLengthInputField.interactable = true;
                        sensorSizeWidthInputField.interactable = true;
                        sensorSizeHeightInputField.interactable = true;
                    }
                    else
                    {
                        focalLengthInputField.interactable = false;
                        sensorSizeWidthInputField.interactable = false;
                        sensorSizeHeightInputField.interactable = false;
                    }
                }
            );
        } // 

        void CalculateFOV(Transform panel)
        {
            FOV.horizontalFov = 2 * Mathf.Atan(SensorSize.width * 0.5f / FocalLength);
            FOV.verticalFov = 2 * Mathf.Atan(SensorSize.height * 0.5f / FocalLength);
            var fovText = panel.GetChild(4).GetComponent<Text>();
            fovText.text = $"'[{FOV.horizontalFov}, {FOV.verticalFov}']";
        }
    };
};
