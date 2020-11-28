using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionDevicePanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        public bool useInteraction = false;

        [Readonly]
        public TMP_Text connectionStatusDisplayText;

        protected override void SaveValues()
        {
            //
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            //
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
            initToggle.isOn = false;

            // 2. bind the init status display text.
            connectionStatusDisplayText = panel.GetChild(1).GetComponent<TMP_Text>();
            connectionStatusDisplayText.text = "---";

            KinectManager.Instance.onKinectConnectionStatusUpdate
                += (string status)
                    => connectionStatusDisplayText.text = $"Connection Status : {status}";          

            LoadPreviousValues(initToggle);
        }
    };
};
