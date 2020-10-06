using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{


    public class DanbiUIProjectorScreenPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public int aspectRatioWidth = 16;

        [Readonly]
        public int aspectRatioHeight = 9;

        [Readonly]
        public int resolutionWidth = 2560;

        [Readonly]
        public int resolutionHeight = 1440;

        [Readonly]
        public float fov = 33.187f;

        [Readonly]
        public EDanbiFOVDirection fovDirection;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ProjectorScreenPanel-aspectRatio-width", aspectRatioWidth);
            PlayerPrefs.SetInt("ProjectorScreenPanel-aspectRatio-height", aspectRatioHeight);
            PlayerPrefs.SetInt("ProjectorScreenpanel-resolution-width", resolutionWidth);
            PlayerPrefs.SetInt("ProjectorScreenpanel-resolution-height", resolutionHeight);
            PlayerPrefs.SetFloat("ProjectorScreenPanel-fov", fov);
            PlayerPrefs.SetInt("ProjectorScreenPanel-fov-direction", fovDirection == EDanbiFOVDirection.Horizontal ? 0 : 1);
        }

        void LoadPreviousValues(params Selectable[] uiElements)
        {
            int prevAspectRatioWidth = PlayerPrefs.GetInt("ProjectorScreenPanel-aspectRatio-width", 16);
            aspectRatioWidth = prevAspectRatioWidth;

            int prevAspectRatioHeight = PlayerPrefs.GetInt("ProjectorScreenPanel-aspectRatio-height", 9);
            aspectRatioHeight = prevAspectRatioHeight;

            int prevResolutionWidth = PlayerPrefs.GetInt("ProjectorScreenPanel-resolution-width", 2560);
            resolutionWidth = prevResolutionWidth;

            int prevResolutionHeight = PlayerPrefs.GetInt("ProjectorScreenPanel-resolution-height", 1440);
            resolutionHeight = prevResolutionHeight;

            float prevFOV = PlayerPrefs.GetFloat("ProjectorScreenPanel-fov", default);
            fov = prevFOV;
            (uiElements[0] as InputField).text = prevFOV.ToString();

            int prevFOVDirection = PlayerPrefs.GetInt("ProjectorScreenPanel-fov-direction", default);
            fovDirection = (EDanbiFOVDirection)prevFOVDirection;
            (uiElements[1] as Dropdown).value = prevFOVDirection;

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        float heightByAspectRatio(float width, float denominator, float numerator) => width * denominator / numerator;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            // Initial value sync.
            Dropdown aspectRatioDropdown = default;
            Dropdown resolutionDropdown = default;

            var resolutions = new float[] {               
               1920,
               2560,
               3840
            };            

            var panel = Panel.transform;
            // 1. Populate the resolution dropdown list.
            var resolutionDropdownList = new List<string>();
            for (int i = 0; i < resolutions.Length; ++i)
            {
                var width = resolutions[i];
                resolutionDropdownList.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
            }

            // 2. bind the aspect ratio.
            aspectRatioDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            aspectRatioDropdown.AddOptions(new List<string> { "16 : 9", "16 : 10" });
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    resolutionDropdownList.Clear();
                    resolutionDropdown.options.Clear();

                    // Decide which resolutions populates the dropdown list on dropdown change.
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

                    // Apply for the resolution dropdown change.
                    resolutionDropdown.AddOptions(resolutionDropdownList);
                    resolutionDropdown.RefreshShownValue();
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            // 3. bind the dropdown.
            resolutionDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            resolutionDropdown.AddOptions(resolutionDropdownList);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    var splitted = resolutionDropdownList[option].Split('x');

                    if (int.TryParse(splitted[0], out var widthAsInt))
                    {
                        resolutionWidth = widthAsInt;
                    }

                    if (int.TryParse(splitted[1], out var heightAsInt))
                    {
                        resolutionHeight = heightAsInt;
                    }

                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    resolutionDropdown.RefreshShownValue();
                }
            );

            // 4. bind the fov inputfield.
            var fovInputField = panel.GetChild(2).GetComponent<InputField>();
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

            // 5. bind the fov direction.
            var fovDirectionDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            fovDirectionDropdown.AddOptions(new List<string> { "Horizontal", "Vertical" });
            fovDirectionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    fovDirection = (EDanbiFOVDirection)option;
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    fovDirectionDropdown.RefreshShownValue();
                }
            );

            LoadPreviousValues(fovInputField, fovDirectionDropdown);
        }
    };
};
