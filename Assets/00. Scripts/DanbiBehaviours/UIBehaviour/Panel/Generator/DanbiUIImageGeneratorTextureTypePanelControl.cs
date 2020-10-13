using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorTextureTypePanelControl : DanbiUIPanelControl
    {        
        protected override void SaveValues()
        {
            PlayerPrefs.SetString("", "");
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var textureTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            textureTypeDropdown.AddOptions(new List<string> { "Normal", "Panorama"});
            textureTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                                        
                }
            );

            LoadPreviousValues(textureTypeDropdown);
        }
    };
};
