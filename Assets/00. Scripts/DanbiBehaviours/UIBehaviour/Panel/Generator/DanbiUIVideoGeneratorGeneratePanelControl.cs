using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorGeneratePanelControl : DanbiUIPanelControl
    {
        // [SerializeField, Readonly]
        // bool isFFMPEGexecutableFound;

        // [SerializeField, Readonly]
        // public string FFMPEGexecutableLocation;

        Button m_generateButton;
        Button m_saveButton;

        TMP_Text progressDisplayText;
        TMP_Text statusDisplayText;

        public delegate void OnAllVideoClipBatchesCompleted();
        public static OnAllVideoClipBatchesCompleted Call_OnAllVideoClipBatchesCompleted;

        public delegate void OnVideoSave();
        public static OnVideoSave onVideoSave;

        protected override void SaveValues()
        {
            // var prevFFMPEGexecutableLocation = PlayerPrefs.GetString("videoGenerator-ffmpegExecutableLocation", default);
            // if (!string.IsNullOrEmpty(prevFFMPEGexecutableLocation))
            // {
            //     isFFMPEGexecutableFound = true;
            //     FFMPEGexecutableLocation = prevFFMPEGexecutableLocation;
            // }
            // else
            // {
            //     isFFMPEGexecutableFound = false;
            // }
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            // PlayerPrefs.SetString("videoGenerator-ffmpegExecutableLocation", FFMPEGexecutableLocation);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // DanbiVideoHelper.ConcatenateVideoClips(new string[] { "acd.mp4" });

            var panel = Panel.transform;

            Call_OnAllVideoClipBatchesCompleted += () =>
            {
                // reactivate both generate and save button after all video clips batches are completed!
                m_generateButton.interactable = true;                
                // TODO: Update the progress and the status display texts that all the processes are finished!
            };

            // 1. bind the select ffmpeg executable button.
            // var selectFFMPEGexecutableButton = panel.GetChild(0).GetComponent<Button>();
            // selectFFMPEGexecutableButton.onClick.AddListener(
            //     () =>
            //     {
            //         StartCoroutine(Coroutine_SelectFFMPEGexecutable());
            //     }
            // );

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
                }
            );

            // 4. bind the progress display text.
            progressDisplayText = panel.GetChild(3).GetComponent<TMP_Text>();

            // 5. bind the status display text.
            statusDisplayText = panel.GetChild(4).GetComponent<TMP_Text>();
        }

        //         IEnumerator Coroutine_SelectFFMPEGexecutable()
        //         {
        //             var filters = new string[] { ".exe" };
        //             string startingPath = default;
        // #if UNITY_EDITOR
        //             startingPath = Application.dataPath;
        // #else
        //             startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        // #endif

        //             yield return DanbiFileSys.OpenLoadDialog(startingPath,
        //                                                      filters,
        //                                                      "Select FFMPEG Executable",
        //                                                      "Select");

        //             DanbiFileSys.GetResourcePathIntact(out FFMPEGexecutableLocation, out _);
        //             if (!string.IsNullOrEmpty(FFMPEGexecutableLocation))
        //             {
        //                 isFFMPEGexecutableFound = true;

        //                 generateButton.interactable = true;
        //                 saveButton.interactable = true;
        //             }

        //             DanbiUISync.onPanelUpdated?.Invoke(this);
        //         }
    };
};
