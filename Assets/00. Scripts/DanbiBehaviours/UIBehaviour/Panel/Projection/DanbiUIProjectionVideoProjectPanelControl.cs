﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Danbi
{
    public class DanbiUIProjectionVideoProjectPanelControl : DanbiUIPanelControl
    {
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
                    // m_vp.Play();
                    DanbiGestureListener.onWalkDetected?.Invoke();
                }
            );

            // bind stop project button.
            m_stopProjectButton = panel.GetChild(1).GetComponent<Button>();
            m_stopProjectButton.onClick.AddListener(
                () =>
                {
                    // m_vp.Pause();
                    DanbiGestureListener.onWalkComplete?.Invoke();
                }
            );
        }
    };
};
