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
                DanbiManager.onGenerateImage?.Invoke();
                saveButton.interactable = true;
                // TODO: Update the generated result.
                var statusDisplayText = panel.GetChild(2).GetComponent<Text>();
                statusDisplayText.text = $"~~";
            });

            saveButton = panel.GetChild(1).GetComponent<Button>();
            saveButton.onClick.AddListener(() => DanbiManager.onSaveImage?.Invoke());
            saveButton.interactable = false;
        }

        // IEnumerator Coroutine_Generate(Transform panel, Button saveBtn)
        // {

        // }
    };
};
