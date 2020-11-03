using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiWalkAction : MonoBehaviour
    {
        [SerializeField, Readonly]
        bool m_isPaused;

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
        }

        void OnWalkComplete()
        {
            m_isPaused = true;
        }
    };
};
