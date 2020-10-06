using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorTypePanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var reflectorTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            reflectorTypeDropdown.AddOptions(new List<string> { "Halfsphere", "Cone", "Pyramid" });
            reflectorTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    DanbiUIReflectorShapePanelControl.Call_OnTypeChanged?.Invoke(option);
                }
            );
        }
    };
};
