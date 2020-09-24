using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenTypePanelControl : DanbiUIPanelControl
    {
        [SerializeField]
        List<string> PanoramaTypeContents = new List<string>();

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            // DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.PanoramaScreenType, this);
            var panel = Panel.transform;

            var panoramaTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            panoramaTypeDropdown?.AddOptions(PanoramaTypeContents);
            panoramaTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    DanbiUIPanoramaScreenShapePanelControl.Call_OnTypeChanged?.Invoke(option);
                }
            );
        }
    };
};