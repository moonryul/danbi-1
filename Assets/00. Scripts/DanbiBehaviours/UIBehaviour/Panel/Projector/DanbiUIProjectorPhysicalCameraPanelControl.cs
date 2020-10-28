using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIProjectorPhysicalCameraPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float focalLength;
        [Readonly]
        public (float width, float height) sensorSize;
        [Readonly]
        public bool isToggled = false;

        public delegate void OnFOVCalcuated(float newFOV);
        public OnFOVCalcuated Call_OnFOVCalcuated;

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-focalLength", focalLength);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-sensorSize-width", sensorSize.width);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-sensorSize-height", sensorSize.height);
            PlayerPrefs.SetInt("ProjectorPhysicalCamera-isToggled", isToggled ? 1 : 0);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevIsToggled = PlayerPrefs.GetInt("ProjectorPhysicalCamera-isToggled", default);
            isToggled = prevIsToggled == 1;
            (uiElements[0] as Toggle).isOn = isToggled;

            var prevFocalLength = PlayerPrefs.GetFloat("ProjectorPhysicalCamera-focalLength", default);
            focalLength = prevFocalLength;
            (uiElements[1] as InputField).text = prevFocalLength.ToString();

            var prevSensorSizeWidth = PlayerPrefs.GetFloat("ProjectorPhysicalCamera-sensorSize-width", default);
            sensorSize.width = prevSensorSizeWidth;
            (uiElements[2] as InputField).text = prevSensorSizeWidth.ToString();

            float prevSensorSizeHeight = PlayerPrefs.GetFloat("ProjectorPhysicalCamera-sensorSize-height", default);
            sensorSize.height = prevSensorSizeHeight;
            (uiElements[3] as InputField).text = prevSensorSizeHeight.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        void updateFOVdisplay(float newFOV)
        {
            var fovDisplayText = Panel.transform.GetChild(4).GetComponent<Text>();
            fovDisplayText.text = $"FOV : {newFOV}";
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            Call_OnFOVCalcuated += (float newFOV) => updateFOVdisplay(newFOV);

            var panel = Panel.transform;

            InputField focalLengthInputField = default;
            InputField sensorSizeWidthInputField = default;
            InputField sensorSizeHeightInputField = default;
            var fovText = panel.GetChild(4).GetComponent<Text>();

            // 1. bind the physical camera toggle.
            var physicalCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            physicalCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    isToggled = isOn;
                    focalLengthInputField.interactable = isOn;
                    sensorSizeWidthInputField.interactable = isOn;
                    sensorSizeHeightInputField.interactable = isOn;
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            // 2. bind the focal length.
            focalLengthInputField = panel.GetChild(1).GetComponent<InputField>();
            focalLengthInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        focalLength = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // 3. bind the width of the sensor size.
            sensorSizeWidthInputField = panel.GetChild(2).GetComponent<InputField>();
            sensorSizeWidthInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        sensorSize.width = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // 4. bind the height of the sensor size.
            sensorSizeHeightInputField = panel.GetChild(3).GetComponent<InputField>();
            sensorSizeHeightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        sensorSize.height = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(physicalCameraToggle, focalLengthInputField, sensorSizeWidthInputField, sensorSizeHeightInputField);
        } //         
    };
};
