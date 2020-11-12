using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiUIProjectorInfoPanelControl : DanbiUIPanelControl
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
        public float fov = 32.7f;

        [Readonly]
        public float projectorHeight;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ProjectorScreenPanel-aspectRatio-width", aspectRatioWidth);
            PlayerPrefs.SetInt("ProjectorScreenPanel-aspectRatio-height", aspectRatioHeight);
            PlayerPrefs.SetInt("ProjectorScreenpanel-resolution-width", resolutionWidth);
            PlayerPrefs.SetInt("ProjectorScreenpanel-resolution-height", resolutionHeight);
            PlayerPrefs.SetFloat("ProjectorScreenPanel-fov", fov);
            PlayerPrefs.SetFloat("ProjectorScreenPanel-project-height", projectorHeight);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            int prevAspectRatioWidth = PlayerPrefs.GetInt("ProjectorScreenPanel-aspectRatio-width", 16);
            aspectRatioWidth = prevAspectRatioWidth;

            int prevAspectRatioHeight = PlayerPrefs.GetInt("ProjectorScreenPanel-aspectRatio-height", 9);
            aspectRatioHeight = prevAspectRatioHeight;

            int prevResolutionWidth = PlayerPrefs.GetInt("ProjectorScreenPanel-resolution-width", 3840);
            resolutionWidth = prevResolutionWidth;

            int prevResolutionHeight = PlayerPrefs.GetInt("ProjectorScreenPanel-resolution-height", 2160);
            resolutionHeight = prevResolutionHeight;

            float prevFOV = PlayerPrefs.GetFloat("ProjectorScreenPanel-fov", default);
            fov = prevFOV;

            float prevProjectorHeight = PlayerPrefs.GetFloat("ProjectorScreenPanel-project-height", default);
            projectorHeight = prevProjectorHeight;
            // TODO: bind the InputField!!

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        float heightByAspectRatio(float width, float denominator, float numerator)
            => width * denominator / numerator;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // Initial value sync.
            Dropdown aspectRatioDropdown = default;
            Dropdown resolutionDropdown = default;
            TMPro.TMP_InputField fovInputField = default;

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
                    DanbiUISync.onPanelUpdate?.Invoke(this);
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

                    resolutionDropdown.RefreshShownValue();
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // select 4k first.
            resolutionDropdown.value = 2;

            // 4. bind the field of view
            fovInputField = panel.GetChild(2).GetComponent<TMPro.TMP_InputField>();
            fovInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        fov = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues();
        }
    };
};
