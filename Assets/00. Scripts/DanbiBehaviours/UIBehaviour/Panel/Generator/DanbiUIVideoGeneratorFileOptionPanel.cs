using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorFileOptionPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        string m_videoPath;

        [SerializeField, Readonly]
        string m_videoName;

        [SerializeField, Readonly]
        EDanbiVideoExt m_videoExt;

        [SerializeField, Readonly]
        EDanbiOpencvCodec_fourcc_ m_videoCodec;

        [SerializeField, Readonly]
        int m_targetFrameRate;

        string m_savePathAndName => $"{m_videoPath}/{m_videoName}.{m_videoExt}";

        public delegate void OnVideoExtChange(EDanbiVideoExt ext);
        public static OnVideoExtChange onVideoExtChange;

        public delegate void OnVideoCodecChange(EDanbiOpencvCodec_fourcc_ codec);
        public static OnVideoCodecChange onVideoCodecChange;

        public delegate void OnTargetFrameRateChange(int targetFrameRate);
        public static OnTargetFrameRateChange onTargetFrameRateChange;

        public delegate void OnSavedVideoPathAndNameChange(string savedVideoPathAndName);
        public static OnSavedVideoPathAndNameChange onSavedVideoPathAndNameChange;

        public delegate void OnSavedVideoPathChange(string videoPath);
        public static OnSavedVideoPathChange onSavedVideoPathChange;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("VideoGeneratorFileOption-vidPathOnly", m_videoPath);
            PlayerPrefs.SetString("VideoGeneratorFileOption-vidNameOnly", m_videoName);
            PlayerPrefs.SetInt("VideoGeneratorFileOption-vidExtOnly", (int)m_videoExt);
            PlayerPrefs.SetInt("VideoGeneratorFileOption-vidCodec", (int)m_videoCodec);
            PlayerPrefs.SetInt("VideoGeneratorFileOption-targetFrameRate", m_targetFrameRate);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            // load previous video path
            string prevVidPathOnly = PlayerPrefs.GetString("VideoGeneratorFileOption-vidPath", default);
            if (!string.IsNullOrEmpty(prevVidPathOnly))
            {
                m_videoPath = prevVidPathOnly;
                Panel.transform.GetChild(5).GetComponent<TMP_Text>().text = m_videoPath;
                onSavedVideoPathChange?.Invoke(m_videoPath);
            }

            // load previous video name
            string prevVidNameOnly = PlayerPrefs.GetString("VideoGeneratorFileOption-vidNameOnly", default);
            if (!string.IsNullOrEmpty(prevVidNameOnly))
            {
                m_videoName = prevVidNameOnly;
                Panel.transform.GetChild(1).GetComponent<TMP_InputField>().text = m_videoName;
            }

            // load previous video (format)extension
            var prevVidExtOnly = (EDanbiVideoExt)PlayerPrefs.GetInt("VideoGeneratorFileOption-vidExtOnly", default);
            m_videoExt = prevVidExtOnly;
            Panel.transform.GetChild(2).GetComponent<TMP_Dropdown>().value = (int)m_videoExt;
            onVideoExtChange?.Invoke(m_videoExt);

            // load previous video codec.
            var prevVidCodec = (EDanbiOpencvCodec_fourcc_)PlayerPrefs.GetInt("VideoGeneratorFileOption-vidCodec", default);
            m_videoCodec = prevVidCodec;
            Panel.transform.GetChild(3).GetComponent<TMP_Dropdown>().value = (int)m_videoCodec;
            onVideoCodecChange?.Invoke(m_videoCodec);

            // load previous target frame rate.
            var prevTargetFrameRate = PlayerPrefs.GetInt("VideoGeneratorFileOption-targetFrameRate", default);
            m_targetFrameRate = prevTargetFrameRate;
            Panel.transform.GetChild(4).GetComponent<TMP_InputField>().text = m_targetFrameRate.ToString();
            onTargetFrameRateChange?.Invoke(m_targetFrameRate);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // bind video save location text
            var videoSaveLocationText = panel.GetChild(5).GetComponent<TMP_Text>();

            // bind video save path button.
            var vidSavePathButton = panel.GetChild(0).GetComponent<Button>();
            vidSavePathButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveFilePath(videoSaveLocationText)));

            // bind video name inputfield.
            var vidNameInputField = panel.GetChild(1).GetComponent<TMP_InputField>();
            vidNameInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    m_videoName = val;
                    videoSaveLocationText.text = $"File Location : {m_savePathAndName}";
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
                    m_videoExt = (EDanbiVideoExt)option;
                    onVideoExtChange?.Invoke(m_videoExt);
                    videoSaveLocationText.text = $"File Location : {m_savePathAndName}";
                }
            );

            // bind video codect dropdown
            var vidCodecOptions = new List<string> { "h264", "h265", "divx", "mpeg4", "hevc" };
            var vidCodecDropdown = panel.GetChild(3).GetComponent<TMP_Dropdown>();
            vidCodecDropdown.AddOptions(vidCodecOptions);
            vidCodecDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_videoCodec = (EDanbiOpencvCodec_fourcc_)option;
                    onVideoCodecChange?.Invoke(m_videoCodec);
                }
            );

            // bind video target frame rate
            var vidTargetFrameRateInputField = panel.GetChild(4).GetComponent<TMP_InputField>();
            vidTargetFrameRateInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var res))
                    {
                        m_targetFrameRate = res;
                        onTargetFrameRateChange?.Invoke(m_targetFrameRate);
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
            DanbiFileSys.GetResourcePathIntact(out m_videoPath, out _);
            displayText.text = $"File Location : {m_savePathAndName}";
            onSavedVideoPathAndNameChange?.Invoke(m_savePathAndName);
        }
    };
};
