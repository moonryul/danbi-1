using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Danbi
{
    public class DanbiWalkAction : MonoBehaviour
    {
        [SerializeField, Readonly, Space(10)]
        bool m_isPaused;
        VideoPlayer m_vp;
        RenderTexture m_videoTargetRT;

        void Start()
        {
            DanbiGestureListener.onWalkDetected += OnWalkDetected;
            DanbiGestureListener.onWalkComplete += OnWalkComplete;
            DanbiUISync.onPanelUpdated += OnPanelUpdate;

            m_vp = GetComponent<VideoPlayer>();
            DanbiUIProjectionVideoPanelControl.onProjectionVideoUpdate +=
            (VideoClip clip) =>
            {
                m_vp.clip = clip;
                m_vp.playOnAwake = false;
                // m_videoTargetRT = new RenderTexture((int)m_vp.clip.width, (int)m_vp.clip.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
                // m_vp.targetTexture = m_videoTargetRT;
                // DanbiManager.instance.interactionCamera.targetTexture = m_vp.targetTexture;
                // m_vp.targetCamera = DanbiManager.instance.interactionCamera;
                // m_vp.targetCamera = Camera.main;
                // DanbiManager.instance.projectorControl.m_projectImageRT = m_videoTargetRT;
                var vidDisplay = DanbiManager.instance.videoDisplay.GetComponent<MeshRenderer>();
                vidDisplay.material.SetTexture("_MainTex", m_vp.targetTexture, UnityEngine.Rendering.RenderTextureSubElement.Color);
            };
        }

        void OnDisable()
        {
            DanbiGestureListener.onWalkDetected -= OnWalkDetected;
            DanbiGestureListener.onWalkComplete -= OnWalkComplete;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {

            if (control is DanbiUIInteractionWalkingPanelControl)
            {
                var walkingControl = control as DanbiUIInteractionWalkingPanelControl;
                // 
            }
        }

        bool isInput = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_vp.isPaused)
                {
                    OnWalkDetected();
                }
                else
                {
                    OnWalkComplete();
                }
            }
        }

        void OnWalkDetected()
        {
            m_vp.Play();
        }

        void OnWalkComplete()
        {
            m_vp.Pause();
        }
    };
};
