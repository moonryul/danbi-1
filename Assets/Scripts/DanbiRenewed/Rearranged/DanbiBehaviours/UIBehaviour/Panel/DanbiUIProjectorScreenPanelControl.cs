using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    

    public class DanbiUIProjectorScreenPanelControl : DanbiUIPanelControl
    {
        List<string> AspectRatioContents = new List<string>();
        List<string> ResolutionContents = new List<string>();

        public string aspectRatio = "16 : 9";
        public string resolution;

        void OnDisable()
        {
            PlayerPrefs.SetString("ProjectorScreenPanel-aspectRatio", aspectRatio);
            PlayerPrefs.SetString("ProjectorScreenPanel-resolution", resolution);

            for (var i = 0; i < ResolutionContents.Count; ++i)
            {
                PlayerPrefs.SetString($"ProjectorScreenPanel-ResolutionContents{i}", ResolutionContents[i]);
            }

            for (var i = 0; i < AspectRatioContents.Count; ++i)
            {
                PlayerPrefs.SetString($"ProjectorScreenPanel-AspectRatioContents{i}", AspectRatioContents[i]);
            }
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // Initial value sync.
            aspectRatio = "16 : 9";
            resolution = "1920 x 1080";
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);

            var resolutions = new float[] {
               1280,
               1920,
               2560,
               3840
            };

            // <in, in, in, out> in a row
            System.Func<float, float, float, float> heightByAspectRatio =
             (float width, float denominator, float numerator)
                 => width * denominator / numerator;


            // Fill up the Resolution contents if it's empty.
            if (ResolutionContents.Count == 0)
            {
                for (int i = 0; i < resolutions.Length; ++i)
                {
                    var width = resolutions[i];
                    ResolutionContents.Add(PlayerPrefs.GetString($"ProjectorScreenPanel-ResolutionContents{i}", $"{width} x {heightByAspectRatio(width, 9, 16)}"));
                }
            }

            var panel = Panel.transform;
            // bind the dropdown.
            var resolutionDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            string prevResolution = PlayerPrefs.GetString("ProjectorScreenPanel-resolution", default);
            resolution = prevResolution;
            resolutionDropdown.AddOptions(ResolutionContents);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    resolution = ResolutionContents[option];
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    resolutionDropdown.RefreshShownValue();
                }
            );

            // Fill up the Aspect ratio contents if it's empty.
            if (AspectRatioContents.Count == 0)
            {
                AspectRatioContents.Add(PlayerPrefs.GetString($"ProjectorScreenPanel-AspectRatioContents1", "16 : 9"));
                AspectRatioContents.Add(PlayerPrefs.GetString($"ProjectorScreenPanel-AspectRatioContents2", "16 : 10"));
            }

            // bind the aspect ratio.
            var aspectRatioDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            aspectRatioDropdown.AddOptions(AspectRatioContents);
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    ResolutionContents.Clear();
                    resolutionDropdown.options.Clear();
                    // Decide which resolutions populates the dropdown list.
                    switch (option)
                    {
                        case 0: // 16 : 9                            
                            aspectRatio = "16 : 9";
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                ResolutionContents.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
                            }

                            break;

                        case 1: // 16 : 10                            
                            aspectRatio = "16 : 10";
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                ResolutionContents.Add($"{width} x {heightByAspectRatio(width, 10, 16)}");
                            }
                            break;
                    }
                    // Apply for the resolution dropdown.
                    resolutionDropdown.AddOptions(ResolutionContents);
                    resolutionDropdown.RefreshShownValue();
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );
        }
    };
};
