using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIProjectionImageProjectPanelControl : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        Texture2D m_projectImage;

        Button m_projectButton;
        Button m_stopProjectButton;

        protected override void SaveValues()
        {
            base.SaveValues();
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            base.LoadPreviousValues(uiElements);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();            

            var panel = Panel.transform;

            // bind project button.
            m_projectButton = panel.GetChild(0).GetComponent<Button>();
            m_projectButton.onClick.AddListener(
                () =>
                {
                    DanbiManager.instance.prevSimulatorMode = DanbiManager.instance.simulatorMode;
                    DanbiManager.instance.simulatorMode = EDanbiSimulatorMode.Project;
                }
            );

            // bind stop project button.
            m_stopProjectButton = panel.GetChild(1).GetComponent<Button>();
            m_stopProjectButton.onClick.AddListener(
                () =>
                {
                    DanbiManager.instance.simulatorMode = DanbiManager.instance.prevSimulatorMode;
                }
            );
        }
    };
};
