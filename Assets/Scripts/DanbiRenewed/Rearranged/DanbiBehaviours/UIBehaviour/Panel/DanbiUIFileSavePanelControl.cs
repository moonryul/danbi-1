using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIFileSavePanelControl : DanbiUIPanelControl
    {
        // public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        // {
        //     base.OnMenuButtonSelected(lastClicked);
        // }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // var panel = Panel.transform;
            var saveButton = GetComponent<Button>();
            saveButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_SaveProfile());
                }
            );
        }

        IEnumerator Coroutine_SaveProfile()
        {
            yield break;
        }
    };
};
