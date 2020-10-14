using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIProjectorCalibrationPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public bool useCalbiratedCamera;

        [Readonly]
        public EDanbiCameraUndistortionMethod undistortionMethod = EDanbiCameraUndistortionMethod.Direct;

        [Readonly]
        public float newtonThreshold;

        [Readonly]
        public float iterativeThreshold;

        [Readonly]
        public float iterativeSafetyCounter;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ProjectorCalibrationPanel-useCalibratedCamera", useCalbiratedCamera == true ? 1 : 0);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-undistortionMethod", (int)undistortionMethod);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-newtonThreshold", newtonThreshold);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-iterativeThreshold", iterativeThreshold);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-iterativeSafetyCounter", iterativeSafetyCounter);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            bool prevUseCalibratedCamera = PlayerPrefs.GetInt("ProjectorCalibrationPanel-useCalibratedCamera", default) == 1;
            useCalbiratedCamera = prevUseCalibratedCamera;
            (uiElements[0] as Toggle).isOn = prevUseCalibratedCamera;

            var prevUndistortionMethod = (EDanbiCameraUndistortionMethod)PlayerPrefs.GetInt("ProjectorCalibrationPanel-undistortionMethod", -1);
            undistortionMethod = prevUndistortionMethod;
            (uiElements[1] as Dropdown).value = (int)prevUndistortionMethod;

            var prevNewtonThreshold = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-newtonThreshold", default);
            newtonThreshold = prevNewtonThreshold;
            (uiElements[2] as InputField).text = prevNewtonThreshold.ToString();

            var prevIterativeThreshold = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-iterativeThreshold", default);
            iterativeThreshold = prevIterativeThreshold;
            (uiElements[3] as InputField).text = prevIterativeThreshold.ToString();

            var prevIterativeSafetyCounter = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-iterativeSafetyCounter", default);
            iterativeSafetyCounter = prevIterativeSafetyCounter;
            (uiElements[4] as InputField).text = prevIterativeSafetyCounter.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            Dropdown undistortionMethodDropdown = default;
            GameObject newtonProperties = default;
            GameObject iterativeProperties = default;
            InputField newtonThresholdInputField = default;
            InputField iterativeThresholdInputField = default;
            InputField iterativeSafetyCounterInputField = default;

            // 1. bind the "Calibrated Camera" toggle.
            var useCalibratedCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            useCalibratedCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useCalbiratedCamera = isOn;
                    undistortionMethodDropdown.interactable = isOn;
                    newtonProperties.SetActive(isOn);
                    iterativeProperties.SetActive(isOn);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            // bind the undistortion Method dropdown.
            undistortionMethodDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            undistortionMethodDropdown.AddOptions(new List<string> { "direct", "iterative", "newton" });
            undistortionMethodDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0: // direct
                            undistortionMethod = EDanbiCameraUndistortionMethod.Direct;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(false);
                            break;

                        case 1: // iterative
                            undistortionMethod = EDanbiCameraUndistortionMethod.Iterative;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(true);
                            break;

                        case 2: // newton
                            undistortionMethod = EDanbiCameraUndistortionMethod.Newton;
                            newtonProperties.SetActive(true);
                            iterativeProperties.SetActive(false);
                            break;
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );
            undistortionMethodDropdown.value = 0;

            // bind the newton properties and turn it off.
            newtonProperties = panel.GetChild(2).gameObject;

            // bind the newton threshold.
            newtonThresholdInputField = newtonProperties.transform.GetChild(0).GetComponent<InputField>();
            newtonThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        newtonThreshold = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            newtonProperties.SetActive(false);

            // bind the iterative properties and turn it off.
            iterativeProperties = panel.GetChild(3).gameObject;

            // bind the iterative threshold.
            iterativeThresholdInputField = iterativeProperties.transform.GetChild(0).GetComponent<InputField>();
            iterativeThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        iterativeThreshold = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // bind the iterative safety counter.
            iterativeSafetyCounterInputField = iterativeProperties.transform.GetChild(1).GetComponent<InputField>();
            iterativeSafetyCounterInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        iterativeSafetyCounter = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            iterativeProperties.SetActive(false);

            LoadPreviousValues(useCalibratedCameraToggle, undistortionMethodDropdown, newtonThresholdInputField, iterativeThresholdInputField, iterativeSafetyCounterInputField);
        }
    };
};
