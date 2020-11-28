using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIImageGeneratorFilePathPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        string m_filePath;

        [SerializeField, Readonly]
        string m_fileName;

        [SerializeField, Readonly]
        string m_fileExt;

        [SerializeField, Readonly]
        EDanbiImageExt m_imageExt;

        public string fileSavePathAndName => $"{m_filePath}/{m_fileName}{m_fileExt}";

        public delegate void OnFilePathChanged(string filePath);
        public static OnFilePathChanged onFilePathChanged;

        public delegate void OnFileExtChanged(EDanbiImageExt imgExt);
        public static OnFileExtChanged onFileExtChanged;

        public delegate void OnFileSavePathAndNameChanged(string savePathAndName);
        public static OnFileSavePathAndNameChanged onFileSavePathAndNameChanged;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("ImageGeneratorFilePath-filePath", m_filePath);
            PlayerPrefs.SetString("ImageGeneratorFilePath-fileName", m_fileName);
            PlayerPrefs.SetString("ImageGeneratorFilePath-fileExt", m_fileExt);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevSavePath = PlayerPrefs.GetString("ImageGeneratorFilePath-filePath", default);
            if (!string.IsNullOrEmpty(prevSavePath))
            {
                m_filePath = prevSavePath;
                Panel.transform.GetChild(3).GetComponent<TMP_Text>().text = m_filePath;
                onFilePathChanged?.Invoke(m_filePath);
            }

            string prevFileName = PlayerPrefs.GetString("ImageGeneratorFilePath-fileName", default);
            if (!string.IsNullOrEmpty(prevFileName))
            {
                m_fileName = prevFileName;
                Panel.transform.GetChild(1).GetComponent<TMP_InputField>().text = m_fileName;
            }

            string prevFileExt = PlayerPrefs.GetString("ImageGeneratorFilePath-fileExt", default);
            if (!string.IsNullOrEmpty(prevFileExt))
            {
                m_fileExt = prevFileExt;
                EDanbiImageExt prevExt = default;
                if (m_fileExt == ".jpg")
                {
                    prevExt = EDanbiImageExt.jpg;
                }
                else if (m_fileExt == ".png")
                {
                    prevExt = EDanbiImageExt.png;
                }
                onFileExtChanged?.Invoke(prevExt);
            }
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var fileSaveLocationText = panel.GetChild(3).GetComponent<TMP_Text>();
            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            fileSavePathButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveFilePath(fileSaveLocationText)));

            var fileNameInputField = panel.GetChild(1).GetComponent<TMP_InputField>();
            fileNameInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    m_fileName = val;
                    fileSaveLocationText.text = $"File Location : {fileSavePathAndName}";
                    onFileSavePathAndNameChanged?.Invoke(fileSavePathAndName);
                }
            );

            var fileExtOptions = new List<string> { ".png", ".jpg" };
            var fileExtDropdown = panel.GetChild(2).GetComponent<TMP_Dropdown>();
            fileExtDropdown.AddOptions(fileExtOptions);
            fileExtDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_fileExt = fileExtOptions[option];
                    m_imageExt = (EDanbiImageExt)option;
                    fileSaveLocationText.text = $"File Location : {fileSavePathAndName}";
                    onFileExtChanged?.Invoke(m_imageExt);
                    onFileSavePathAndNameChanged?.Invoke(fileSavePathAndName);
                }
            );

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SaveFilePath(TMP_Text displayText)
        {
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     null,
                                                     "Select Save Image File Path",
                                                     "Select",
                                                     true);
            DanbiFileSys.GetResourcePathIntact(out m_filePath, out _);
            onFilePathChanged?.Invoke(m_filePath);
            displayText.text = $"File Location : {fileSavePathAndName}";
        }
    };
};
