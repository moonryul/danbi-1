using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVideoGeneratorFileSavePathPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public string actualPath;
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
