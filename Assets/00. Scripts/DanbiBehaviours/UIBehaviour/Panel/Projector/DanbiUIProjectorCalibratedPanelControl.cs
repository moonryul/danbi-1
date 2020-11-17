using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIProjectorCalibratedPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public bool useCalibratedCamera;

        // TODO : add -1(Not Used) into the dropdown.
        [Readonly]
        public EDanbiLensUndistortMode lensUndistortMode = EDanbiLensUndistortMode.NotUsing;

        [Readonly]
        public float newtonThreshold;

        [Readonly]
        public float iterativeThreshold;

        [Readonly]
        public float iterativeSafetyCounter;

        public delegate void OnSetUseCalibratedCamera(bool use);
        public static OnSetUseCalibratedCamera onSetUseCalibratedCamera;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ProjectorCalibrationPanel-useCalibratedCamera", useCalibratedCamera == true ? 1 : 0);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-lensDistortionMode", (int)lensUndistortMode);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-newtonThreshold", newtonThreshold);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-iterativeThreshold", iterativeThreshold);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-iterativeSafetyCounter", iterativeSafetyCounter);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            bool prevUseCalibratedCamera = PlayerPrefs.GetInt("ProjectorCalibrationPanel-useCalibratedCamera", default) == 1;
            useCalibratedCamera = prevUseCalibratedCamera;
            (uiElements[0] as Toggle).isOn = prevUseCalibratedCamera;
            onSetUseCalibratedCamera?.Invoke(prevUseCalibratedCamera);

            var prevLensDistortionMode = (EDanbiLensUndistortMode)PlayerPrefs.GetInt("ProjectorCalibrationPanel-lensDistortionMode", -1);
            lensUndistortMode = prevLensDistortionMode;
            (uiElements[1] as Dropdown).value = (int)prevLensDistortionMode;

            var prevNewtonThreshold = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-newtonThreshold", default);
            newtonThreshold = prevNewtonThreshold;
            (uiElements[2] as InputField).text = prevNewtonThreshold.ToString();

            var prevIterativeThreshold = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-iterativeThreshold", default);
            iterativeThreshold = prevIterativeThreshold;
            (uiElements[3] as InputField).text = prevIterativeThreshold.ToString();

            var prevIterativeSafetyCounter = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-iterativeSafetyCounter", default);
            iterativeSafetyCounter = prevIterativeSafetyCounter;
            (uiElements[4] as InputField).text = prevIterativeSafetyCounter.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            Dropdown lensUndistortionModeDropdown = default;
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
                    onSetUseCalibratedCamera?.Invoke(isOn);
                    useCalibratedCamera = isOn;
                    lensUndistortionModeDropdown.interactable = isOn;                    
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // bind the undistortion Method dropdown.
            lensUndistortionModeDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            lensUndistortionModeDropdown.AddOptions(new List<string> { "no undistort", "direct", "iterative", "newton" });
            lensUndistortionModeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0: // no undistort
                            lensUndistortMode = EDanbiLensUndistortMode.NotUsing;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(false);
                            break;

                        case 1: // direct
                            lensUndistortMode = EDanbiLensUndistortMode.Direct;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(false);
                            break;

                        case 2: // iterative
                            lensUndistortMode = EDanbiLensUndistortMode.Iterative;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(true);
                            break;

                        case 3: // newton
                            lensUndistortMode = EDanbiLensUndistortMode.Newton;
                            newtonProperties.SetActive(true);
                            iterativeProperties.SetActive(false);
                            break;
                    }
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

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
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            // newtonThresholdInputField.gameObject.SetActive(false);
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
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            iterativeProperties.SetActive(false);
            lensUndistortionModeDropdown.value = 0;

            LoadPreviousValues(useCalibratedCameraToggle, lensUndistortionModeDropdown, newtonThresholdInputField, iterativeThresholdInputField, iterativeSafetyCounterInputField);
        }
    };
};
