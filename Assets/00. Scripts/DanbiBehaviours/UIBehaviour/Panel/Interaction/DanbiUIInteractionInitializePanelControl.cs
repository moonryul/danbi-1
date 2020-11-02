using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionInitializePanelControl : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        Button initButton;

        [SerializeField, Readonly]
        public TMP_Text statusDisplayText;

        protected override void SaveValues()
        {
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // 1. bind the init button.
            initButton = panel.GetChild(0).GetComponent<Button>();
            initButton.onClick.AddListener(
                () =>
                {
                    // DanbiKinectControl.Call_OnKinectInit?.Invoke(statusDisplayText);

                }
            );

            // 2. bind the init status display text.
            statusDisplayText = panel.GetChild(1).GetComponent<TMP_Text>();

            LoadPreviousValues();
        }
    };
};
