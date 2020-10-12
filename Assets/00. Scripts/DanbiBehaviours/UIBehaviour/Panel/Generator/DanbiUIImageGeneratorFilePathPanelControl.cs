using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorFilePathPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public string filePath;
        [Readonly]
        public string fileName;
        [Readonly]
        public string fileExt;
        public string fileSavePathAndName => $"{filePath}/{fileName}{fileExt}";

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("ImageGeneratorFilePath-filePath", filePath);
            PlayerPrefs.SetString("ImageGeneratorFilePath-fileName", fileName);
            PlayerPrefs.SetString("ImageGeneratorFilePath-fileExt", fileExt);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevSavePath = PlayerPrefs.GetString("ImageGeneratorFilePath-filePath", default);
            if (!string.IsNullOrEmpty(prevSavePath))
            {
                filePath = prevSavePath;
                Panel.transform.GetChild(3).GetComponent<Text>().text = filePath;
            }

            string prevFileName = PlayerPrefs.GetString("ImageGeneratorFilePath-fileName", default);
            if (!string.IsNullOrEmpty(prevFileName))
            {
                fileName = prevFileName;
                (uiElements[0] as InputField).text = fileName;
            }

            string prevFileExt = PlayerPrefs.GetString("ImageGeneratorFilePath-fileExt", default);
            if (!string.IsNullOrEmpty(prevFileExt))
            {
                fileExt = prevFileExt;
            }

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var fileSaveLocationText = panel.GetChild(3).GetComponent<Text>();

            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            fileSavePathButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveFilePath(fileSaveLocationText)));

            var fileNameInputField = panel.GetChild(1).GetComponent<InputField>();
            fileNameInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    fileName = val;
                    fileSaveLocationText.text = $"File Location : {fileName}";
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            var fileExtOptions = new List<string> { ".png", ".jpg" };
            var fileExtDropdown = panel.GetChild(2).GetComponent<Dropdown>();
            fileExtDropdown.AddOptions(fileExtOptions);
            fileExtDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    fileExt = fileExtOptions[option];
                    fileSaveLocationText.text = $"File Location : {fileSavePathAndName}";
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            LoadPreviousValues(fileNameInputField);
        }

        IEnumerator Coroutine_SaveFilePath(Text displayText)
        {
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     null,
                                                     "Select Save File Path",
                                                     "Select",
                                                     true);
            DanbiFileSys.GetResourcePathIntact(out filePath, out _);
            displayText.text = $"File Location : {fileSavePathAndName}";
        }
    };
};
