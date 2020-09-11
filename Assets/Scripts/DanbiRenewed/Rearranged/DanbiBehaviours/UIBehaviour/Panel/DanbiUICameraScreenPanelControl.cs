using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUICameraScreenPanelControl : DanbiUIPanelControl
    {
        List<string> ScreenAspectContents = new List<string>();

        List<string> ResolutionContents = new List<string>();

        protected override void BindPanelFields()
        {
            base.BindPanelFields();

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

            // bind the dropdown.
            var resolutionDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            resolutionDropdown.AddOptions(ResolutionContents);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    // TODO: update the screen resolution related to the result image.
                }
            );


            var aspectRatioDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            aspectRatioDropdown.AddOptions(ScreenAspectContents);
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    // Decide which resolutions populates the dropdown list.
                    switch (option)
                    {
                        case 0: // 16 : 9
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                ResolutionContents.Add($"{width} x {heightByAspectRatio(width, 9, 16)}");
                                resolutionDropdown.AddOptions(ResolutionContents);
                            }
                            break;

                        case 1: // 16 : 10
                            for (int i = 0; i < resolutions.Length; ++i)
                            {
                                var width = resolutions[i];
                                ResolutionContents.Add($"{width} x {heightByAspectRatio(width, 10, 16)}");
                                resolutionDropdown.AddOptions(ResolutionContents);
                            }
                            break;
                    }
                }
            );
        }
    };
};
