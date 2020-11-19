using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorFileOptionPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public string vidPathOnly;
        [Readonly]
        public string vidNameOnly;
        [Readonly]
        public EDanbiVideoExt vidExtOnly;
        [Readonly]
        public EDanbiOpencvCodec_fourcc_ vidCodec;
        [Readonly]
        public float targetFrameRate;

        public string savePathAndNameFull => $"{vidPathOnly}/{vidNameOnly}.{vidExtOnly}";

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("VideoGeneratorFileOption-vidPathOnly", vidPathOnly);
            PlayerPrefs.SetString("VideoGeneratorFileOption-vidNameOnly", vidNameOnly);
            PlayerPrefs.SetInt("VideoGeneratorFileOption-vidExtOnly", (int)vidExtOnly);
            PlayerPrefs.SetInt("VideoGeneratorFileOption-vidCodec", (int)vidCodec);
            PlayerPrefs.SetFloat("VideoGeneratorFileOption-targetFrameRate", targetFrameRate);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            // load previous video path
            string prevVidPathOnly = PlayerPrefs.GetString("VideoGeneratorFileOption-vidPath", default);
            if (!string.IsNullOrEmpty(prevVidPathOnly))
            {
                vidPathOnly = prevVidPathOnly;
                Panel.transform.GetChild(5).GetComponent<TMP_Text>().text = vidPathOnly;
            }

            // load previous video name
            string prevVidNameOnly = PlayerPrefs.GetString("VideoGeneratorFileOption-vidNameOnly", default);
            if (!string.IsNullOrEmpty(prevVidNameOnly))
            {
                vidNameOnly = prevVidNameOnly;
                Panel.transform.GetChild(1).GetComponent<TMP_InputField>().text = vidNameOnly;
            }

            // load previous video (format)extension
            var prevVidExtOnly = (EDanbiVideoExt)PlayerPrefs.GetInt("VideoGeneratorFileOption-vidExtOnly", default);
            vidExtOnly = prevVidExtOnly;
            Panel.transform.GetChild(2).GetComponent<TMP_Dropdown>().value = (int)vidExtOnly;

            // load previous video codec.
            var prevVidCodec = (EDanbiOpencvCodec_fourcc_)PlayerPrefs.GetInt("VideoGeneratorFileOption-vidCodec", default);
            vidCodec = prevVidCodec;
            Panel.transform.GetChild(3).GetComponent<TMP_Dropdown>().value = (int)vidCodec;

            // load previous target frame rate.
            var prevTargetFrameRate = PlayerPrefs.GetFloat("VideoGeneratorFileOption-targetFrameRate", default);
            targetFrameRate = prevTargetFrameRate;
            Panel.transform.GetChild(4).GetComponent<TMP_InputField>().text = targetFrameRate.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // bind video save location text
            var vidSaveLocationText = panel.GetChild(5).GetComponent<TMP_Text>();

            // bind video save path button.
            var vidSavePathButton = panel.GetChild(0).GetComponent<Button>();
            vidSavePathButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveFilePath(vidSaveLocationText)));

            // bind video name inputfield.
            var vidNameInputField = panel.GetChild(1).GetComponent<TMP_InputField>();
            vidNameInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    vidNameOnly = val;
                    vidSaveLocationText.text = $"File Location : {savePathAndNameFull}";
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // bind video extension dropdown
            var vidExtOptions = new List<string> { ".mp4", ".avi", "m4v", ".mov", ".webm", ".wmv" };
            var vidExtDropdown = panel.GetChild(2).GetComponent<TMP_Dropdown>();
            vidExtDropdown.AddOptions(vidExtOptions);
            vidExtDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    // vidExtOnly = vidExtOptions[option];
                    vidExtOnly = (EDanbiVideoExt)option;
                    vidSaveLocationText.text = $"File Location : {savePathAndNameFull}";
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // bind video codect dropdown
            var vidCodecOptions = new List<string> { "h264", "h265", "divx", "mpeg4", "hevc" };
            var vidCodecDropdown = panel.GetChild(3).GetComponent<TMP_Dropdown>();
            vidCodecDropdown.AddOptions(vidCodecOptions);
            vidCodecDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    vidCodec = (EDanbiOpencvCodec_fourcc_)option;
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // bind video target frame rate
            var vidTargetFrameRateInputField = panel.GetChild(4).GetComponent<TMP_InputField>();
            vidTargetFrameRateInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        targetFrameRate = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SaveFilePath(TMP_Text displayText)
        {
            string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     null,
                                                     "Select Save Video File Path",
                                                     "Select",
                                                     true);
            DanbiFileSys.GetResourcePathIntact(out vidPathOnly, out _);
            displayText.text = $"File Location : {savePathAndNameFull}";
            DanbiUISync.onPanelUpdate?.Invoke(this);
        }
    };
};
