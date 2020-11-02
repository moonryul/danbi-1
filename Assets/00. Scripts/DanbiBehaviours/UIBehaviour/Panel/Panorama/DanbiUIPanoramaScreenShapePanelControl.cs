using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenShapePanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public int selectedPrewarperSettingIndex;
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var panoramaTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            panoramaTypeDropdown?.AddOptions(new List<string> { "Cube", "Cylinder" });
            panoramaTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    selectedPrewarperSettingIndex = option;
                    DanbiUIPanoramaScreenDimensionPanelControl.Call_OnTypeChanged?.Invoke(option);
                    DanbiUIPanoramaScreenOpticalPanelControl.Call_OnTypeChanged?.Invoke(option);
                    panel.gameObject.SetActive(false);
                    DanbiUISync.onPanelUpdated?.Invoke(this);
                }
            );
        }
    };
};