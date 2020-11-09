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
