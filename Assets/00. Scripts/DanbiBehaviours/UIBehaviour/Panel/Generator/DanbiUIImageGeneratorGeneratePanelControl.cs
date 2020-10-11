using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorGeneratePanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var saveButton = default(Button);
            // 1. bind the generate button.
            var generateButton = panel.GetChild(0).GetComponent<Button>();
            generateButton.onClick.AddListener(() =>
            {
                DanbiUIControl.GenerateImage();
                // TODO: Update the generated result.
                saveButton.interactable = true;
                var statueDisplayText = panel.GetChild(2).GetComponent<Text>();
                statueDisplayText.text = $"~~";
            });

            saveButton = panel.GetChild(1).GetComponent<Button>();
            saveButton.onClick.AddListener(() => DanbiUIControl.SaveImage());
            saveButton.interactable = false;
        }

        // IEnumerator Coroutine_Generate(Transform panel, Button saveBtn)
        // {

        // }
    };
};
