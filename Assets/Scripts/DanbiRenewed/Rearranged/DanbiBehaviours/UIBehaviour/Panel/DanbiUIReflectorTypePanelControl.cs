using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorTypePanelControl : DanbiUIPanelControl
    {
        List<string> PanoramaTypeContents = new List<string>();
        void OnDisable()
        {
            for (var i = 0; i < PanoramaTypeContents.Count; ++i)
            {
                PlayerPrefs.SetString($"PanoramaReflectorTypePanel-content${i}", PanoramaTypeContents[i]);
            }
        }
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaReflectorTypePanel-content1", "Halfsphere"));
            PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaReflectorTypePanel-content2", "Cone"));
            PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaReflectorTypePanel-content3", "Pyramid"));

            var panel = Panel.transform;
            var reflectorTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            reflectorTypeDropdown.AddOptions(PanoramaTypeContents);
            reflectorTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    DanbiUIReflectorShapePanelControl.Call_OnTypeChanged?.Invoke(option);
                }
            );
        }
    };
};
