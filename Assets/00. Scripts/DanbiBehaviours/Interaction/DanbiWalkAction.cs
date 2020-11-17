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
            DanbiUISync.onPanelUpdate += OnPanelUpdate;

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

        /// <summary>
        /// called while the progress == 1.0
        /// </summary>
        void OnWalkDetected()
        {
            if (m_vp.isPaused)
            {
                m_vp.Play();
                DanbiWalkTimer.instance.EndChecking();
            }
        }

        /// <summary>
        /// called when state 1, 2 is timeout
        /// </summary>
        void OnWalkComplete()
        {
            if (m_vp.isPlaying)
            {
                m_vp.Pause(); // video is paused during the state == 0.
            }
        }
    };
};
