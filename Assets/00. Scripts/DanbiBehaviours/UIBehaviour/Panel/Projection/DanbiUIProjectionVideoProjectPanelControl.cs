using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Danbi
{
    public class DanbiUIProjectionVideoProjectPanelControl : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        VideoPlayer m_vp;

        Button m_projectButton;
        Button m_stopProjectButton;

        RenderTexture m_videoTargetRT;

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

            m_vp = GetComponent<VideoPlayer>();
            DanbiUIProjectionVideoPanelControl.onProjectionVideoUpdate +=
                (VideoClip clip) =>
                {
                    m_vp.clip = clip;
                    m_videoTargetRT = new RenderTexture((int)m_vp.clip.width, (int)m_vp.clip.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
                    m_vp.targetTexture = m_videoTargetRT;
                    DanbiManager.instance.projectorControl.m_projectImageRT = m_videoTargetRT;
                };

            var panel = Panel.transform;

            // bind project button.
            m_projectButton = panel.GetChild(0).GetComponent<Button>();
            m_projectButton.onClick.AddListener(
                () =>
                {
                    if (m_vp.isPaused)
                    {
                        m_vp.Play();
                    }
                }
            );

            // bind stop project button.
            m_stopProjectButton = panel.GetChild(1).GetComponent<Button>();
            m_stopProjectButton.onClick.AddListener(
                () =>
                {
                    if (m_vp.isPlaying)
                    {
                        m_vp.Pause();
                    }
                }
            );
        }
    };
};
