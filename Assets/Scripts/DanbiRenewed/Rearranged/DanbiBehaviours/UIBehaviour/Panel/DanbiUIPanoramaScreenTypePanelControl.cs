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

        void OnDisable()
        {
            for (var i = 0; i < PanoramaTypeContents.Count; ++i)
            {
                PlayerPrefs.SetString($"PanoramaScreenTypePanel-content${i}", PanoramaTypeContents[i]);
            }
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaScreenTypePanel-content1", "Cube"));
            PanoramaTypeContents.Add(PlayerPrefs.GetString("PanoramaScreenTypePanel-content2", "Cylinder"));

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