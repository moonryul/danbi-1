using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorGeneratePanel : DanbiUIPanelControl
    {
        Button m_generateButton;
        Button m_saveButton;

        TMP_Text progressDisplayText;
        TMP_Text statusDisplayText;

        public delegate void OnAllVideoClipBatchesCompleted();
        public static OnAllVideoClipBatchesCompleted onAllVideoClipBatchesCompleted;

        public delegate void OnVideoSave();
        public static OnVideoSave onVideoSave;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // DanbiVideoHelper.ConcatenateVideoClips(new string[] { "acd.mp4" });

            var panel = Panel.transform;

            onAllVideoClipBatchesCompleted += () =>
            {
                // reactivate both generate and save button after all video clips batches are completed!
                m_generateButton.interactable = true;                
                // TODO: Update the progress and the status display texts that all the processes are finished!
            };

            // 2. bind the generate button.
            m_generateButton = panel.GetChild(1).GetComponent<Button>();
            m_generateButton.onClick.AddListener(
                () =>
                {
                    // Turn off both generate and save button during the generating videos.
                    m_generateButton.interactable = false;
                    m_saveButton.interactable = true;
                    DanbiManager.instance.GenerateVideo(progressDisplayText, statusDisplayText);
                }
            );

            m_saveButton = panel.GetChild(2).GetComponent<Button>();
            m_saveButton.onClick.AddListener(
                () =>
                {
                    m_generateButton.interactable = true;
                    onVideoSave?.Invoke();
                }
            );

            // 4. bind the progress display text.
            progressDisplayText = panel.GetChild(3).GetComponent<TMP_Text>();

            // 5. bind the status display text.
            statusDisplayText = panel.GetChild(4).GetComponent<TMP_Text>();
        }
    };
};
