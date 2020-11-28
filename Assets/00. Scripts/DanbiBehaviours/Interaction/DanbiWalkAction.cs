using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Danbi
{
    public class DanbiWalkAction : MonoBehaviour
    {        
        VideoPlayer m_vp;

        void Start()
        {
            DanbiGestureListener.onWalkDetected += OnWalkDetected;
            DanbiGestureListener.onWalkComplete += OnWalkComplete;
            // DanbiUISync.onPanelUpdate += OnPanelUpdate;

            m_vp = GetComponent<VideoPlayer>();
            DanbiUIProjectionVideoPanelControl.onProjectionVideoUpdate +=
            (VideoPlayer vp) =>
            {
                vp.Play();
                // vp.targetTexture = 
            };
        }

        void OnDisable()
        {
            DanbiGestureListener.onWalkDetected -= OnWalkDetected;
            DanbiGestureListener.onWalkComplete -= OnWalkComplete;
        }

        // void OnPanelUpdate(DanbiUIPanelControl control)
        // {

        //     if (control is DanbiUIInteractionWalkingPanelControl)
        //     {
        //         var walkingControl = control as DanbiUIInteractionWalkingPanelControl;
        //         // 
        //     }
        // }

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
            m_vp.Play();
            // if (m_vp.isPaused)
            // {
                
            //     // DanbiWalkTimer.instance.EndChecking();
            // }
        }

        /// <summary>
        /// called when state 1, 2 is timeout
        /// </summary>
        void OnWalkComplete()
        {
            // if (m_vp.isPlaying)
            // {
            //     m_vp.Pause(); // video is paused during the state == 0.
            // }
        }
    };
};
