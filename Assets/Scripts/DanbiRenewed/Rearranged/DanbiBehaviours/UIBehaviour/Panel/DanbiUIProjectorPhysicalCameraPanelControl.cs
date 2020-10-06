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
        public (float horizontal, float vertical) fov;
        [Readonly]
        public bool isToggled = false;
        [Readonly]
        public EDanbiFOVDirection fovDirection;

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-focalLength", focalLength);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-sensorSize-width", sensorSize.width);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-sensorSize-height", sensorSize.height);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-fov-horizontal", fov.horizontal);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-fov-vertical", fov.vertical);
            PlayerPrefs.SetInt("ProjectorPhysicalCamera-isToggled", isToggled ? 1 : 0);
            PlayerPrefs.SetInt("ProjectorPhysicalCamera-fov-direction", fovDirection == EDanbiFOVDirection.Horizontal ? 0 : 1);
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

            int prevFOVDirection = PlayerPrefs.GetInt("ProjectorPhysicalCamera-fov-direction", default);
            fovDirection = (EDanbiFOVDirection)prevFOVDirection;
            (uiElements[4] as Dropdown).value = prevFOVDirection;

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            InputField focalLengthInputField = default;
            InputField sensorSizeWidthInputField = default;
            InputField sensorSizeHeightInputField = default;
            var fovText = panel.GetChild(4).GetComponent<Text>();
            Dropdown fovDirectionDropdown = default;

            // 1. bind the physical camera toggle.
            var physicalCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            physicalCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    isToggled = isOn;
                    focalLengthInputField.interactable = isOn;
                    sensorSizeWidthInputField.interactable = isOn;
                    sensorSizeHeightInputField.interactable = isOn;
                    fovDirectionDropdown.interactable = isOn;
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
                        CalculateFOV(fovText);
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
                        CalculateFOV(fovText);
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
                        CalculateFOV(fovText);
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            fovDirectionDropdown = panel.GetChild(5).GetComponent<Dropdown>();            
            fovDirectionDropdown.AddOptions(new List<string> { "Horizontal", "Vertical" });
            fovDirectionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    fovDirection = (EDanbiFOVDirection)option;                    
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    fovDirectionDropdown.RefreshShownValue();
                }
            );

            LoadPreviousValues(physicalCameraToggle, focalLengthInputField, sensorSizeWidthInputField, sensorSizeHeightInputField, fovDirectionDropdown);
            CalculateFOV(fovText);
        } // 

        void CalculateFOV(Text fovText)
        {
            // TODO: need to verify.
            fov.horizontal = 2 * Mathf.Atan(sensorSize.width * 0.5f / focalLength);
            fov.vertical = 2 * Mathf.Atan(sensorSize.height * 0.5f / focalLength);
            fovText.text = $"FOV : {fov.horizontal}, {fov.vertical}";
        }
    };
};
