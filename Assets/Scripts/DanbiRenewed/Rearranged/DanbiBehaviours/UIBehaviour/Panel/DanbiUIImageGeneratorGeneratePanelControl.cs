﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorGeneratePanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.ImageGeneratorGenerate, this);

            var panel = Panel.transform;

            var generateButton = panel.GetChild(0).GetComponent<Button>();
            generateButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_Generate(panel)); }
            );
        }

        IEnumerator Coroutine_Generate(Transform panel)
        {
            // TODO: Generate Image!
            DanbiUIControl.instance.GenerateImage();
            // TODO: Update the generated result.
            var statueDisplayText = panel.GetChild(1).GetComponent<Text>();
            statueDisplayText.text = $"~~";

            yield break;
        }
    };
};
