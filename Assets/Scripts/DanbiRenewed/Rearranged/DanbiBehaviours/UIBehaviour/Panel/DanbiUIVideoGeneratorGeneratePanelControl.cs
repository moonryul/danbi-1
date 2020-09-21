using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVideoGeneratorGeneratePanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.VideoGeneratorGenerate, this);

            var panel = Panel.transform;

            var generateButton = panel.GetChild(0).GetComponent<Button>();
            generateButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_Generate(panel)); }
            );
        }

        IEnumerator Coroutine_Generate(Transform panel)
        {
            // TODO: Generate Image!

            // TODO: Update the generated result.
            var progressDisplayText = panel.GetChild(1).GetComponent<Text>();
            progressDisplayText.text = $"Start to warp" +
                                      "(500 / 25510) " +
                                      "(1.96001%)";

            var statusDisplayText = panel.GetChild(2).GetComponent<Text>();
            statusDisplayText.text = "Image generating succeed!";

            yield break;
        }
    };
};
