using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{


    public class DanbiUIProjectorScreenPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float aspectRatioWidth = 16.0f;
        [Readonly]
        public float aspectRatioHeight = 9.0f;
        [Readonly]
        public float resolutionWidth = 2560.0f;
        [Readonly]
        public float resolutionHeight = 1440.0f;
        [Readonly]
        public float fov = 33.187f;
        [Readonly]
        public EDanbiFOVDirection fovDirection;


        void OnDisable()
        {
            // PlayerPrefs.SetFloat("ProjectorScreenPanel-aspectRatio-width", aspectRatioWidth);
            // PlayerPrefs.SetFloat("ProjectorScreenPanel-aspectRatio-height", aspectRatioHeight);
            // PlayerPrefs.SetFloat("ProjectorScreenPanel-resolution-width", resolutionWidth);
            // PlayerPrefs.SetFloat("ProjectorScreenPanel-resolution-height", resolutionHeight);
            PlayerPrefs.SetFloat("ProjectorScreenPanel-fov", fov);
            PlayerPrefs.SetInt("ProjectorScreenPanel-fov-direction", fovDirection == EDanbiFOVDirection.Horizontal ? 0 : 1);
        }

        float heightByAspectRatio(float width, float denominator, float numerator) => width * denominator / numerator;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            // PlayerPrefs.DeleteAll();
            // Initial value sync.            

            Dropdown aspectRatioDropdown = default;
            Dropdown resolutionDropdown = default;

            var resolutions = new float[] {
               1280,
               1920,
               2560,
               3840
            };

            var panel = Panel.transform;
            var resolutionDropdownList = new List<string>();
            for (int i = 0; i < resolutions.Length; ++i)
            {
                var width = resolutions[i];
                resolutionDropdownList.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
            }

            // bind the aspect ratio.
            aspectRatioDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            aspectRatioDropdown.AddOptions(new List<string> { "16 : 9", "16 : 10" });
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    resolutionDropdownList.Clear();
                    resolutionDropdown.options.Clear();
                    // Decide which resolutions populates the dropdown list.
                    switch (option)
                    {
                        case 0: // 16 : 9                            
                            aspectRatioWidth = 16;
                            aspectRatioHeight = 9;
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                resolutionDropdownList.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
                            }
                            break;

                        case 1: // 16 : 10                            
                            aspectRatioWidth = 16;
                            aspectRatioHeight = 10;
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                resolutionDropdownList.Add($"{width} x {heightByAspectRatio(width, 10, 16)}");
                            }
                            break;
                    }
                    // Apply for the resolution dropdown.
                    resolutionDropdown.AddOptions(resolutionDropdownList);
                    resolutionDropdown.RefreshShownValue();
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            // bind the dropdown.
            resolutionDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            resolutionDropdown.AddOptions(resolutionDropdownList);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    var splitted = resolutionDropdownList[option].Split('x');
                    resolutionWidth = float.Parse(splitted[0]);
                    resolutionHeight = float.Parse(splitted[1]);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    resolutionDropdown.RefreshShownValue();
                }
            );

            var fovInputField = panel.GetChild(2).GetComponent<InputField>();
            float prevFOV = PlayerPrefs.GetFloat("ProjectorScreenPanel-fov", default);
            fovInputField.text = prevFOV.ToString();
            fov = prevFOV;
            fovInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        fov = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            var fovDirectionDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            int prevFOVDirection = PlayerPrefs.GetInt("ProjectorScreenPanel-fov-direction", default);
            fovDirection = (EDanbiFOVDirection)prevFOVDirection;
            fovDirectionDropdown.AddOptions(new List<string> { "Horizontal", "Vertical" });
            fovDirectionDropdown.value = prevFOVDirection;
            fovDirectionDropdown.onValueChanged.AddListener(
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
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    fovDirectionDropdown.RefreshShownValue();
                }
            );
        }
    };
};
