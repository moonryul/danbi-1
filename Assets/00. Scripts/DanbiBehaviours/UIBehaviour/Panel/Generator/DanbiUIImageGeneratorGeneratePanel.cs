using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorGeneratePanel : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var saveButton = default(Button);

            // bind generate button.
            var generateButton = panel.GetChild(0).GetComponent<Button>();
            generateButton.onClick.AddListener(() =>
            {
                saveButton.interactable = true;
                DanbiManager.instance.GenerateImage(panel.GetChild(2).GetComponent<TMPro.TMP_Text>(), null);
            });

            // bind save button
            saveButton = panel.GetChild(1).GetComponent<Button>();
            saveButton.onClick.AddListener(() => DanbiManager.instance.SaveImage());
            saveButton.interactable = false;
        }
    };
};
