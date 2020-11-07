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
        VideoPlayer m_projectedVidPlayer;        

        void Start()
        {
            DanbiGestureListener.onWalkDetected += OnWalkDetected;
            DanbiGestureListener.onWalkComplete += OnWalkComplete;
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
        }

        void OnDisable()
        {
            DanbiGestureListener.onWalkDetected -= OnWalkDetected;
            DanbiGestureListener.onWalkComplete += OnWalkComplete;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {

            if (control is DanbiUIInteractionWalkingPanelControl)
            {
                var walkingControl = control as DanbiUIInteractionWalkingPanelControl;
                // 
            }
        }

        void OnWalkDetected()
        {
            m_isPaused = false;
            if (m_projectedVidPlayer.isPaused)
            {
                m_projectedVidPlayer.Play();
            }
        }

        void OnWalkComplete()
        {
            m_isPaused = true;
            if (m_projectedVidPlayer.isPlaying)
            {
                m_projectedVidPlayer.Pause();
            }
        }
    };
};
