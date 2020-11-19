using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorShapePanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public int selectedReflectorIndex;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var reflectorTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            reflectorTypeDropdown.AddOptions(new List<string> { "Dome", "Cone" });
            reflectorTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    selectedReflectorIndex = option;
                    DanbiUIReflectorDimensionPanelControl.Call_OnTypeChanged?.Invoke(option);
                    DanbiUIReflectorOpticalPanelControl.Call_OnTypeChanged?.Invoke(option);
                    panel.gameObject.SetActive(false);
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                });
            reflectorTypeDropdown.onValueChanged?.Invoke(0);
        }
    };
};
