using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIProjectorCalibrationPanelControl : DanbiUIPanelControl
    {
        public bool useCalbiratedCamera;
        public EDanbiCameraUndistortionMethod undistortionMethod = EDanbiCameraUndistortionMethod.Direct;
        public int newtonThreshold;
        public int iterativeThreshold;
        public int iterativeSafetyCounter;

        void OnDisable()
        {
            PlayerPrefs.SetInt("ProjectorCalibrationPanel-useCalibratedCamera", useCalbiratedCamera == true ? 1 : 0);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-undistortionMethod", (int)undistortionMethod);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-newtonThreshold", newtonThreshold);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-iterativeThreshold", iterativeThreshold);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-iterativeSafetyCounter", iterativeSafetyCounter);
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

            // bind the "Calibrated Camera" toggle.
            var useCalibratedCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            bool prevUseCalibratedCamera = PlayerPrefs.GetInt("ProjectorCalibrationPanel-useCalibratedCamera", default) == 1;
            useCalbiratedCamera = prevUseCalibratedCamera;
            useCalibratedCameraToggle.isOn = prevUseCalibratedCamera;
            useCalibratedCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useCalbiratedCamera = isOn;
                    if (isOn)
                    {
                        undistortionMethodDropdown.interactable = true;
                    }
                    else
                    {
                        undistortionMethodDropdown.interactable = false;
                        newtonProperties.SetActive(false);
                        iterativeProperties.SetActive(false);
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            // bind the undistortion Method dropdown.
            undistortionMethodDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            undistortionMethodDropdown.AddOptions(new List<string> { "direct", "iterative", "newton" });
            var prevUndistortionMethod = (EDanbiCameraUndistortionMethod)PlayerPrefs.GetInt("ProjectorCalibrationPanel-undistortionMethod", -1);
            undistortionMethod = prevUndistortionMethod;
            undistortionMethodDropdown.value = (int)prevUndistortionMethod;
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

                        default:
                            Debug.LogError($"{option} is undefined behaviour");
                            break;
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            // bind the newton properties and turn it off.
            newtonProperties = panel.GetChild(2).gameObject;

            // bind the newton threshold.
            newtonThresholdInputField = newtonProperties.transform.GetChild(0).GetComponent<InputField>();
            int prevNewtonThreshold = PlayerPrefs.GetInt("ProjectorCalibrationPanel-newtonThreshold", default);
            newtonThreshold = prevNewtonThreshold;
            newtonThresholdInputField.text = prevNewtonThreshold.ToString();
            newtonThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        newtonThreshold = asInt;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            newtonProperties.SetActive(false);

            // bind the iterative properties and turn it off.
            iterativeProperties = panel.GetChild(3).gameObject;

            // bind the iterative threshold.
            iterativeThresholdInputField = iterativeProperties.transform.GetChild(0).GetComponent<InputField>();
            int prevIterativeThreshold = PlayerPrefs.GetInt("ProjectorCalibrationPanel-iterativeThreshold", default);
            iterativeThreshold = prevIterativeThreshold;
            iterativeThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        iterativeThreshold = asInt;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // bind the iterative safety counter.
            iterativeSafetyCounterInputField = iterativeProperties.transform.GetChild(1).GetComponent<InputField>();
            int prevIterativeSafetyCounter = PlayerPrefs.GetInt("ProjectorCalibrationPanel-iterativeSafetyCounter", default);
            iterativeSafetyCounter = prevIterativeSafetyCounter;
            iterativeSafetyCounterInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        iterativeSafetyCounter = asInt;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            iterativeProperties.SetActive(false);
        } // bind        
    };
};
