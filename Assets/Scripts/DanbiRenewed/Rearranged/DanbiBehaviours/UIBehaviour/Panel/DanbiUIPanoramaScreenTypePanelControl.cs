using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenTypePanelControl : DanbiUIPanelControl
    {
        List<string> PanoramaTypeContents = new List<string>();

        void OnDisable()
        {
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            // PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaScreenTypePanel-content1", "Cube"));
            // PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaScreenTypePanel-content2", "Cylinder"));

            var panel = Panel.transform;
            var panoramaTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            panoramaTypeDropdown?.AddOptions(new List<string>{"Cube", "Cylinder"});
            panoramaTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    DanbiUIPanoramaScreenShapePanelControl.Call_OnTypeChanged?.Invoke(option);
                }
            );
        }
    };
};