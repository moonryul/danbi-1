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

        void OnDisable()
        {
            PlayerPrefs.SetString("ImageGeneratorFileSavePath-savePath", savePath);
        }
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            string prevSavePath = PlayerPrefs.GetString("ImageGeneratorFileSavePath-savePath", default);
            if (!string.IsNullOrEmpty(prevSavePath))
            {
                savePath = prevSavePath;
                panel.GetChild(1).GetComponent<Text>().text = savePath;
            }
            fileSavePathButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveFilePath(panel)); }
            );
        }

        IEnumerator Coroutine_SaveFilePath(Transform panel)
        {
            var startingPath = Application.dataPath + "/Resources/";
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
