using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiUIProjectorInfoPanel : DanbiUIPanelControl
    {
        int m_aspectRatioHeight = 9;

        [SerializeField, Readonly]
        int m_resolutionWidth = 2560;

        [SerializeField, Readonly]
        int m_resolutionHeight = 1440;

        [SerializeField, Readonly]
        float m_fov = 32.7f;

        [SerializeField, Readonly]
        float m_projectorHeight;

        public delegate void OnResolutionWidthUpdate(int width);
        public static OnResolutionWidthUpdate onResolutionWidthUpdate;

        public delegate void OnResolutionHeightUpdate(int height);
        public static OnResolutionHeightUpdate onResolutionHeightUpdate;

        public delegate void OnFovUpdate(float fov);
        public static OnFovUpdate onFovUpdate;

        public delegate void OnProjectorHeightUpdate(float height);
        public static OnProjectorHeightUpdate onProjectorHeightUpdate;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ProjectorScreenPanel-aspectRatio-height", m_aspectRatioHeight);
            PlayerPrefs.SetInt("ProjectorScreenpanel-resolution-width", m_resolutionWidth);
            PlayerPrefs.SetInt("ProjectorScreenpanel-resolution-height", m_resolutionHeight);
            PlayerPrefs.SetFloat("ProjectorScreenPanel-fov", m_fov);
            PlayerPrefs.SetFloat("ProjectorScreenPanel-project-height", m_projectorHeight);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            int prevAspectRatioHeight = PlayerPrefs.GetInt("ProjectorScreenPanel-aspectRatio-height", 9);
            m_aspectRatioHeight = prevAspectRatioHeight;

            int prevResolutionWidth = PlayerPrefs.GetInt("ProjectorScreenPanel-resolution-width", 3840);
            m_resolutionWidth = prevResolutionWidth;
            onResolutionWidthUpdate?.Invoke(m_resolutionWidth);

            int prevResolutionHeight = PlayerPrefs.GetInt("ProjectorScreenPanel-resolution-height", 2160);
            m_resolutionHeight = prevResolutionHeight;
            onResolutionHeightUpdate?.Invoke(m_resolutionHeight);

            float prevFOV = PlayerPrefs.GetFloat("ProjectorScreenPanel-fov", default);
            m_fov = prevFOV;
            (uiElements[0] as TMPro.TMP_InputField).text = m_fov.ToString();
            onFovUpdate?.Invoke(m_fov);

            float prevProjectorHeight = PlayerPrefs.GetFloat("ProjectorScreenPanel-project-height", default);
            m_projectorHeight = prevProjectorHeight;
            (uiElements[1] as TMPro.TMP_InputField).text = m_projectorHeight.ToString();
            onProjectorHeightUpdate?.Invoke(m_projectorHeight);
        }

        float HeightByAspectRatio(float width, float denominator, float numerator)
            => width * denominator / numerator;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // Initial value sync.
            Dropdown aspectRatioDropdown = default;
            Dropdown resolutionDropdown = default;
            TMPro.TMP_InputField fovInputField = default;

            var resolutions = new float[] { 1920, 2560, 3840 };
            var panel = Panel.transform;

            // 1. Populate the resolution dropdown list.
            var resolutionDropdownList16_9 = new List<string>();
            for (int i = 0; i < resolutions.Length; ++i)
            {
                float width = resolutions[i];
                resolutionDropdownList16_9.Add($"{width} x {HeightByAspectRatio(width, 9, 16)}");
            }

            var resolutionDropdownList16_10 = new List<string>();
            for (int i = 0; i < resolutions.Length; ++i)
            {
                float width = resolutions[i];
                resolutionDropdownList16_10.Add($"{width} x {HeightByAspectRatio(width, 10, 16)}");
            }

            // 2. bind the aspect ratio.
            aspectRatioDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            aspectRatioDropdown.AddOptions(new List<string> { "16 : 9", "16 : 10" });
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    resolutionDropdown.options.Clear();

                    // Decide which resolutions populates the dropdown list on dropdown change.
                    switch (option)
                    {
                        case 0: // 16 : 9                              
                            m_aspectRatioHeight = 9;
                            resolutionDropdown.AddOptions(resolutionDropdownList16_9);
                            break;

                        case 1: // 16 : 10                                                                                   
                            m_aspectRatioHeight = 10;
                            resolutionDropdown.AddOptions(resolutionDropdownList16_10);
                            break;
                    }

                    // Apply for the resolution dropdown change.                    
                    onResolutionHeightUpdate?.Invoke(m_aspectRatioHeight);
                    resolutionDropdown.RefreshShownValue();
                }
            );

            // 3. bind the dropdown.
            var splitted = new string[3];
            resolutionDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            resolutionDropdown.AddOptions(resolutionDropdownList16_9);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (m_aspectRatioHeight)
                    {
                        case 9: // 16 : 9
                            splitted = resolutionDropdownList16_9[option].Split('x');
                            break;

                        case 10: // 16 : 10
                            splitted = resolutionDropdownList16_10[option].Split('x');
                            break;
                    }

                    if (int.TryParse(splitted[0], out var width))
                    {
                        m_resolutionWidth = width;
                        onResolutionWidthUpdate?.Invoke(m_resolutionWidth);
                    }

                    if (int.TryParse(splitted[1], out var height))
                    {
                        m_resolutionHeight = height;
                        onResolutionHeightUpdate?.Invoke(m_resolutionHeight);
                    }

                    resolutionDropdown.RefreshShownValue();
                }
            );
            // select 4k first.
            resolutionDropdown.value = 2;

            // 4. bind the field of view
            fovInputField = panel.GetChild(2).GetComponent<TMPro.TMP_InputField>();
            fovInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_fov = res;
                        onFovUpdate?.Invoke(m_fov);
                    }
                }
            );

            // bind the projector height
            var projectorHeightInputField = panel.GetChild(3).GetComponent<TMPro.TMP_InputField>();
            projectorHeightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_projectorHeight = result;
                        onProjectorHeightUpdate?.Invoke(m_projectorHeight);
                    }
                }
            );

            LoadPreviousValues(fovInputField, projectorHeightInputField);
        }
    };
};
