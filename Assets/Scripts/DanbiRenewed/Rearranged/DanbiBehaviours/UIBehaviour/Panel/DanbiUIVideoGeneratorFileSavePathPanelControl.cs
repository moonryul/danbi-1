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

            // DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.VideoGeneratorFileSavePath, this);

            var panel = Panel.transform;

            var fileSavePathButton = panel.GetChild(0).GetComponent<Button>();
            fileSavePathButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveFilePath(panel)); }
            );
        }

        IEnumerator Coroutine_SaveFilePath(Transform panel)
        {
            var startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileBrowser.OpenLoadDialog(startingPath,
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
