using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUICameraCalibrationPanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // forward the reference of the newton properties and turn it off.
            var newtonProperties = panel.GetChild(4).gameObject;
            newtonProperties.SetActive(false);

            // forward the reference of the iterative properties and turn it off.
            var iterativeProperties = panel.GetChild(5).gameObject;
            iterativeProperties.SetActive(false);

            // 2-1. bind the "Lens Distortion Methode" dropdown.
            var undistortionMethodDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            undistortionMethodDropdown.AddOptions(new List<string> { "direct", "iterative", "newton" });
            undistortionMethodDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0: // direct
                                // no parametres are required to be passed into the shader.
                                // so turn all the properties.
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(false);
                            break;

                        case 1: // iterative
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(true);
                            break;

                        case 2: // newton
                            newtonProperties.SetActive(true);
                            iterativeProperties.SetActive(false);
                            break;
                    }
                }
            );

            // 2. bind the "Use Lens Distortion" toggle.
            var useLensDistortionToggle = panel.GetChild(1).GetComponent<Toggle>();
            useLensDistortionToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
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
                }
            );
            useLensDistortionToggle.isOn = false;

            // 1. bind the "Calibrated Camera" toggle.
            var useCalibratedCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            useCalibratedCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    //DanbiUICameraExternalParametersPanelControl.Call_OnToggleCalibratedCameraChanged(isOn);
                    if (isOn)
                    {
                        // Activate the elements as follow.
                        // 4 of elements.
                        useLensDistortionToggle.interactable = true;
                    }
                    else
                    {
                        useLensDistortionToggle.interactable = false;
                        // désactivez les éléments si-dessous.
                    }
                }
            );
            useCalibratedCameraToggle.isOn = false;
        } // bind        
    };
};
