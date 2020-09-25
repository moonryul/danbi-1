using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIProjectorPhysicalCameraPanelControl : DanbiUIPanelControl
    {
        public float focalLength { get; set; }
        public (float width, float height) sensorSize;
        public (float horizontal, float vertical) fov;
        public bool isToggled = false;
        public EDanbiFOVDirection fovDirection;

        void OnDisable()
        {
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-focalLength", focalLength);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-sensorSize-width", sensorSize.width);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-sensorSize-height", sensorSize.height);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-fov-horizontal", fov.horizontal);
            PlayerPrefs.SetFloat("ProjectorPhysicalCamera-fov-vertical", fov.vertical);
            PlayerPrefs.SetInt("ProjectorPhysicalCamera-isToggled", isToggled ? 1 : 0);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            InputField focalLengthInputField = default;
            InputField sensorSizeWidthInputField = default;
            InputField sensorSizeHeightInputField = default;
            var fovText = panel.GetChild(4).GetComponent<Text>();
            Dropdown selectFOVDirectionDropdown = default;

            // bind the physical camera toggle.
            var physicalCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            var prevIsToggled = PlayerPrefs.GetInt("ProjectorPhysicalCamera-isToggled", 0);
            physicalCameraToggle.isOn = prevIsToggled == 1;
            isToggled = prevIsToggled == 1;
            physicalCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    isToggled = isOn;
                    if (isOn)
                    {
                        focalLengthInputField.interactable = true;
                        sensorSizeWidthInputField.interactable = true;
                        sensorSizeHeightInputField.interactable = true;
                        selectFOVDirectionDropdown.interactable = true;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                    else
                    {
                        focalLengthInputField.interactable = false;
                        sensorSizeWidthInputField.interactable = false;
                        sensorSizeHeightInputField.interactable = false;
                        selectFOVDirectionDropdown.interactable = false;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // bind the focal length.
            focalLengthInputField = panel.GetChild(1).GetComponent<InputField>();
            var prevFocalLength = PlayerPrefs.GetFloat("ProjectorPhysicalCamera-focalLength", default);
            focalLengthInputField.text = prevFocalLength.ToString();
            focalLength = prevFocalLength;
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

            // bind the width of the sensor size.
            sensorSizeWidthInputField = panel.GetChild(2).GetComponent<InputField>();
            var prevSensorSizeWidth = PlayerPrefs.GetFloat("ProjectorPhysicalCamera-sensorSize-width", default);
            sensorSizeWidthInputField.text = prevSensorSizeWidth.ToString();
            sensorSize.width = prevSensorSizeWidth;
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

            // bind the height of the sensor size.
            sensorSizeHeightInputField = panel.GetChild(3).GetComponent<InputField>();
            var prevSensorSizeHeight = PlayerPrefs.GetFloat("ProjectorPhysicalCamera-sensorSize-height", default);
            sensorSizeHeightInputField.text = prevSensorSizeHeight.ToString();
            sensorSize.height = prevSensorSizeHeight;
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

            selectFOVDirectionDropdown = panel.GetChild(5).GetComponent<Dropdown>();
            selectFOVDirectionDropdown.AddOptions(new List<string> { "Horizontal", "Vertical" });
            selectFOVDirectionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0:
                            fovDirection = EDanbiFOVDirection.Horizontal;
                            break;

                        case 1:
                            fovDirection = EDanbiFOVDirection.Vertical;
                            break;

                        default:
                            Debug.LogError($"Option {option} is invalid!");
                            break;
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    selectFOVDirectionDropdown.RefreshShownValue();
                }
            );

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
