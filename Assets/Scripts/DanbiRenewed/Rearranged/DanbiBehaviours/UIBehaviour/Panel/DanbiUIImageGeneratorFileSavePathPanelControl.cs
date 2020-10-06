using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorFileSavePathPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public string savePath;
        [Readonly]
        public EDanbiImageType imageType;

        [Readonly]
        public string fileName;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("ImageGeneratorFileSave-savePath", savePath);
            PlayerPrefs.SetString("ImageGeneratorFileSave-fileName", fileName);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevSavePath = PlayerPrefs.GetString("ImageGeneratorFileSave-savePath", default);
            if (!string.IsNullOrEmpty(prevSavePath))
            {
                savePath = prevSavePath;
                Panel.transform.GetChild(1).GetComponent<Text>().text = savePath;
            }

            string prevFileName = PlayerPrefs.GetString("ImageGeneratorFileSave-fileName", default);
            if (!string.IsNullOrEmpty(prevFileName))
            {
                fileName = prevFileName;
                (uiElements[0] as InputField).text = fileName;
            }

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            fileSavePathButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveFilePath(panel)));

            var fileNameInputField = panel.GetChild(2).GetComponent<InputField>();
            fileNameInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    fileName = val;
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            var fileFormatDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            fileFormatDropdown.AddOptions(new List<string> { ".png", ".jpg" });
            fileFormatDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    imageType = (EDanbiImageType)option;
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );

            LoadPreviousValues(fileNameInputField);
        }

        IEnumerator Coroutine_SaveFilePath(Transform panel)
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
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);
            var path = panel.GetChild(1).GetComponent<Text>();
            path.text = SimpleFileBrowser.FileBrowser.Result[0];
        }
    };
};
