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
        public string fileName;

        void OnDisable()
        {
            PlayerPrefs.SetString("ImageGeneratorFileSave-savePath", savePath);
            PlayerPrefs.SetString("ImageGeneratorFileSave-fileName", fileName);
        }
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            string prevSavePath = PlayerPrefs.GetString("ImageGeneratorFileSave-savePath", default);
            if (!string.IsNullOrEmpty(prevSavePath))
            {
                savePath = prevSavePath;
                panel.GetChild(1).GetComponent<Text>().text = savePath;
                DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
            }
            fileSavePathButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveFilePath(panel)); }
            );

            var fileNameInputField = panel.GetChild(2).GetComponent<InputField>();
            string prevFileName = PlayerPrefs.GetString("ImageGeneratorFileSave-fileName", default);
            if (!string.IsNullOrEmpty(prevFileName))
            {
                fileName = prevFileName;
                fileNameInputField.text = fileName;
            }
            fileNameInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    fileName = val;
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
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
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);
            var path = panel.GetChild(1).GetComponent<Text>();
            path.text = SimpleFileBrowser.FileBrowser.Result[0];
        }
    };
};
