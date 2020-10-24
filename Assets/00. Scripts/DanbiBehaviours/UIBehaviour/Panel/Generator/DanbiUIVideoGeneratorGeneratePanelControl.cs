using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVideoGeneratorGeneratePanelControl : DanbiUIPanelControl
    {
        /// <summary>
        /// Update the video process to the status text
        /// </summary>
        /// <param name="currentIdx"></param>
        /// <param name="lastIdx"></param>
        public delegate void OnVideoProcess(int currentIdx, int lastIdx);
        public static OnVideoProcess Call_OnVideoProcess;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            Call_OnVideoProcess += Caller_OnVideoProcess;

            var panel = Panel.transform;
            var saveButton = default(Button);
            // 1. bind the generate button.
            var generateButton = panel.GetChild(0).GetComponent<Button>();
            generateButton.onClick.AddListener(
                () =>
                {
                    DanbiUIControl.GenerateVideo();
                    saveButton.interactable = true;
                    StartCoroutine(Coroutine_Generate(panel));
                }
            );

            saveButton = panel.GetChild(1).GetComponent<Button>();
            saveButton.onClick.AddListener(() => DanbiUIControl.SaveVideo());
            saveButton.interactable = false;
        }

        protected override void OnDisable()
        {
            Call_OnVideoProcess -= Caller_OnVideoProcess;
            base.OnDisable();
        }

        IEnumerator Coroutine_Generate(Transform panel)
        {
            // TODO: Generate Image!

            // TODO: Update the generated result.
            var progressDisplayText = panel.GetChild(2).GetComponent<Text>();
            progressDisplayText.text = $"Start to warp" +
                                      "(500 / 25510) " +
                                      "(1.96001%)";

            var statusDisplayText = panel.GetChild(3).GetComponent<Text>();
            statusDisplayText.text = "Image generating succeed!";

            yield break;
        }

        void Caller_OnVideoProcess(int currentIdx, int lastIdx)
        {

        }
    };
};
