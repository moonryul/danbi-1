using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIFileLoadPanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var loadButton = GetComponent<Button>();
            loadButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_LoadProfile());
                }
            );
        }

        IEnumerator Coroutine_LoadProfile()
        {
            yield break;
        }
    };
};
