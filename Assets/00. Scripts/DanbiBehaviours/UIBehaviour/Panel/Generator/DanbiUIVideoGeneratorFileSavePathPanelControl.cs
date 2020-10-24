using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorFileSavePathPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public string filePath;
        [Readonly]
        public string fileName;
        [Readonly]
        public string fileExt;
        public EDanbiVideoType videoType;
        public string fileSavePathAndName => $"{filePath}/{fileName}{fileExt}";

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("VideoGeneratorFilePath-filePath", filePath);
            PlayerPrefs.SetString("VideoGeneratorFilePath-fileName", fileName);
            PlayerPrefs.SetString("VideoGeneratorFilePath-fileExt", fileExt);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevSavePath = PlayerPrefs.GetString("VideoGeneratorFilePath-filePath", default);
            if (!string.IsNullOrEmpty(prevSavePath))
            {
                filePath = prevSavePath;
                Panel.transform.GetChild(3).GetComponent<TMP_Text>().text = filePath;
            }

            string prevFileName = PlayerPrefs.GetString("VideoGeneratorFilePath-fileName", default);
            if (!string.IsNullOrEmpty(prevFileName))
            {
                fileName = prevFileName;
                Panel.transform.GetChild(1).GetComponent<TMP_InputField>().text = fileName;
            }

            string prevFileExt = PlayerPrefs.GetString("VideoGeneratorFilePath-fileExt", default);
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

            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            fileSavePathButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveFilePath(panel)); }
            );
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
            // DanbiFileBrowser.getActualResourcePath(out actualPath, out _);
            var path = panel.GetChild(1).GetComponent<Text>();
            path.text = SimpleFileBrowser.FileBrowser.Result[0];
        }
    };
};
