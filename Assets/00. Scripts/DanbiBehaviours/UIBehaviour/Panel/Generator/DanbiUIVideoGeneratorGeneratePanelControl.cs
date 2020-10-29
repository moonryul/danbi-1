using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorGeneratePanelControl : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        bool isFFMPEGexecutableFound;

        [SerializeField, Readonly]
        public string FFMPEGexecutableLocation;

        Button generateButton;
        Button saveButton;

        TMP_Text progressDisplayText;
        TMP_Text statusDisplayText;

        public delegate void OnAllVideoClipBatchesCompleted();
        public static OnAllVideoClipBatchesCompleted Call_OnAllVideoClipBatchesCompleted;

        protected override void SaveValues()
        {
            var prevFFMPEGexecutableLocation = PlayerPrefs.GetString("videoGenerator-ffmpegExecutableLocation", default);
            if (!string.IsNullOrEmpty(prevFFMPEGexecutableLocation))
            {
                isFFMPEGexecutableFound = true;
                FFMPEGexecutableLocation = prevFFMPEGexecutableLocation;
            }
            else
            {
                isFFMPEGexecutableFound = false;
            }
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            PlayerPrefs.SetString("videoGenerator-ffmpegExecutableLocation", FFMPEGexecutableLocation);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // DanbiVideoHelper.ConcatenateVideoClips(new string[] { "acd.mp4" });

            var panel = Panel.transform;

            Call_OnAllVideoClipBatchesCompleted += () =>
            {
                // reactivate both generate and save button after all video clips batches are completed!
                generateButton.interactable = true;
                saveButton.interactable = true;
                // TODO: Update the progress and the status display texts that all the processes are finished!
            };

            // 1. bind the select ffmpeg executable button.
            var selectFFMPEGexecutableButton = panel.GetChild(0).GetComponent<Button>();
            selectFFMPEGexecutableButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_SelectFFMPEGexecutable());
                }
            );

            // 2. bind the generate button.
            generateButton = panel.GetChild(1).GetComponent<Button>();
            generateButton.onClick.AddListener(
                () =>
                {
                    // Turn off both generate and save button during the generating videos.
                    generateButton.interactable = false;
                    saveButton.interactable = false;
                    DanbiUIControl.GenerateVideo(progressDisplayText, statusDisplayText);
                }
            );
            generateButton.interactable = false;

            // 3. bind the save button.
            saveButton = panel.GetChild(2).GetComponent<Button>();
            saveButton.onClick.AddListener(
                () =>
                {
                    // Deactivate both during save the video due to concatenate all the temporary video clips.
                    generateButton.interactable = false;
                    saveButton.interactable = false;
                    DanbiUIControl.SaveVideo(FFMPEGexecutableLocation);
                }
            );
            saveButton.interactable = false;

            // 4. bind the progress display text.
            progressDisplayText = panel.GetChild(3).GetComponent<TMP_Text>();

            // 5. bind the status display text.
            statusDisplayText = panel.GetChild(4).GetComponent<TMP_Text>();
        }

        IEnumerator Coroutine_SelectFFMPEGexecutable()
        {
            var filters = new string[] { ".exe" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath;
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif

            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filters,
                                                     "Select FFMPEG Executable",
                                                     "Select");

            DanbiFileSys.GetResourcePathIntact(out FFMPEGexecutableLocation, out _);
            if (!string.IsNullOrEmpty(FFMPEGexecutableLocation))
            {
                isFFMPEGexecutableFound = true;

                generateButton.interactable = true;
                saveButton.interactable = true;
            }

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
