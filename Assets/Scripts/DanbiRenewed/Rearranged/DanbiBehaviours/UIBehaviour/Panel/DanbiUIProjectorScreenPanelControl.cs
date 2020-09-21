using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIProjectorScreenPanelControl : DanbiUIPanelControl
    {
        List<string> ResolutionContents = new List<string>();

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.ProjectorScreen, this);

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

            var panel = Panel.transform;

            for (int i = 0; i < resolutions.Length; ++i)
            {
                var width = resolutions[i];
                ResolutionContents.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
            }

            // bind the dropdown.
            var resolutionDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            resolutionDropdown.AddOptions(ResolutionContents);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    // TODO: update the screen resolution related to the result image.
                }
            );

            // bind the aspect ratio.
            var aspectRatioDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            aspectRatioDropdown.AddOptions(new List<string> { "16 : 9", "16 : 10" });
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    ResolutionContents.Clear();
                    resolutionDropdown.options.Clear();
                    // Decide which resolutions populates the dropdown list.
                    switch (option)
                    {
                        case 0: // 16 : 9
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                ResolutionContents.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
                            }
                            break;

                        case 1: // 16 : 10
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
                }
            );
        }
    };
};
