using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionDevicePanelControl : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        bool useInteraction;

        [Readonly]
        public TMP_Text connectionStatusDisplayText;

        [Readonly]
        public TMP_Text statusDisplayText;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("InteractionDevice-useInteraction", useInteraction ? 1 : 0);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevInitToggleInteractable = PlayerPrefs.GetInt("InteractionDevice-useInteraction", default);
            useInteraction = prevInitToggleInteractable == 1;
            (uiElements[0] as Toggle).interactable = useInteraction;

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var initToggle = panel.GetChild(0).GetComponent<Toggle>();
            initToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useInteraction = isOn;
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // 2. bind the init status display text.
            connectionStatusDisplayText = panel.GetChild(1).GetComponent<TMP_Text>();
            statusDisplayText = panel.GetChild(2).GetComponent<TMP_Text>();

            LoadPreviousValues(initToggle);
        }
    };
};
