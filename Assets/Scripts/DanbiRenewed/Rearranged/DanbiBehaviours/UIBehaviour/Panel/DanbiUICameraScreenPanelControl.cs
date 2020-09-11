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

            var resolutions = new (float, float)[] {
               (720, 486),
               (1280, 720),
               (1920, 1080),
               
            };

            var panel = Panel.transform;
            var aspectRatioDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            aspectRatioDropdown?.AddOptions(ScreenAspectContents);
            aspectRatioDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0: // 16 : 9
                            break;
                            
                        case 1: // 16 : 10
                            break;
                    }
                }
            );

            var resolutionDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            resolutionDropdown?.AddOptions(ResolutionContents);
            resolutionDropdown.onValueChanged.AddListener(
                (int option) =>
                {

                }
            );

        }
    };
};
