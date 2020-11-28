using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorShapePanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        int m_selectedReflectorIndex;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var reflectorTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            reflectorTypeDropdown.AddOptions(new List<string> { "Dome", "Cone" });
            reflectorTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_selectedReflectorIndex = option;
                    DanbiUIReflectorDimensionPanel.onTypeChanged?.Invoke(option);
                    panel.gameObject.SetActive(false);
                    // DanbiUISync.onPanelUpdate?.Invoke(this);
                });
            reflectorTypeDropdown.onValueChanged?.Invoke(0);
        }
    };
};
