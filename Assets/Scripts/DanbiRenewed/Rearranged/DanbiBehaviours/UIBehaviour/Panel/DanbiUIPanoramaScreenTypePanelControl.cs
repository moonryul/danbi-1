using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;


namespace Danbi
{
    public class DanbiUIPanoramaScreenTypePanelControl : DanbiUIPanelControl
    {
        [SerializeField]
        List<string> PanoramaTypeContents = new List<string>();

        protected override void BindPanelFields()
        {
            base.BindPanelFields();

            var panel = Panel.transform;
            var panoramaTypeDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            panoramaTypeDropdown?.AddOptions(PanoramaTypeContents);
            panoramaTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0:
                            break;

                        case 1:
                            break;

                        case 2:
                            break;
                    }
                }
            );
        }
    };
};
