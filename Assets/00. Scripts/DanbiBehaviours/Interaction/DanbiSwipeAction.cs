using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    // 1. rotate the cmaera about the z-axis when swipe left / right is detected!
    // 2. detector 2.
    // 3. 
    public class DanbiSwipeAction : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        float m_handRotationToCamRotationRatio;

        [SerializeField, Readonly]
        bool m_anyLeftDetected = false;

        [SerializeField, Readonly]
        bool m_anyRightDetected = false;

        [SerializeField, Readonly]
        float m_swipedLeftAngle;

        [SerializeField, Readonly]
        float m_swipedRightAngle;

        [SerializeField]
        float m_stepAngle = 30.0f;

        float m_prevSwipeLeftProgress;
        float m_prevSwipeRightProgress;

        void Start()
        {
            DanbiGestureListener.onSwipeLeftDetected += OnSwipeLeftDetected;
            DanbiGestureListener.onSwipeLeftComplete += OnSwipeLeftCompleted;
            DanbiGestureListener.onSwipeRightDetected += OnSwipeRightDetected;
            DanbiGestureListener.onSwipeRightComplete += OnSwipeRightCompleted;

            DanbiUISync.onPanelUpdate += OnPanelUpdate;

            // DanbiManager.instance.videoDisplay.transform.localRotation = Quaternion.Euler(90.0f, 90.0f, 90.0f);
        }

        void OnDisable()
        {
            DanbiGestureListener.onSwipeLeftDetected -= OnSwipeLeftDetected;
            DanbiGestureListener.onSwipeLeftComplete -= OnSwipeLeftCompleted;
            DanbiGestureListener.onSwipeRightDetected -= OnSwipeRightDetected;
            DanbiGestureListener.onSwipeRightComplete -= OnSwipeRightCompleted;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            // TODO: Update the step angle.
            // if (control is DanbiUIInteractionSwipeToLeftPanelControl)
            // {
            //     var swipeControl = control as DanbiUIInteractionSwipeToLeftPanelControl;
            //     //
            // }
        }

        void OnSwipeLeftDetected(float progress)
        {
            if (progress == 0.0f)
            {
                return;
            }

            float deltaProgress = progress - m_prevSwipeLeftProgress;
            m_prevSwipeLeftProgress = progress;

            // m_anyLeftDetected = true;
            m_swipedLeftAngle = -(m_stepAngle * deltaProgress);

            // start rotating along the direction.         
            // var dstQuat = DanbiManager.instance.videoDisplay.transform.rotation * Quaternion.Euler(0.0f, 0.0f, m_swipedLeftAngle);\
            Debug.Log($"L: {m_swipedLeftAngle}");
            var dstQuat = DanbiManager.instance.m_videoDisplay.transform.rotation * Quaternion.Euler(0.0f, m_swipedLeftAngle, 0.0f);
            // DanbiManager.instance.videoDisplay.transform.rotation = dstQuat;

            // dstQuat.eulerAngles = new Vector3(dstQuat.eulerAngles.x, 90.0f, 90.0f);
            DanbiManager.instance.m_videoDisplay.transform.localRotation = dstQuat;
            Danbi.DanbiSwipeToLeftTimer.instance.StartChecking();
        }

        void OnSwipeRightDetected(float progress)
        {
            if (progress == 0.0f)
            {
                return;
            }

            float deltaProgress = progress - m_prevSwipeRightProgress;
            m_prevSwipeRightProgress = progress;

            // m_anyRightDetected = true;
            m_swipedRightAngle = m_stepAngle * deltaProgress;

            // start rotating along the direction.         
            // var dstQuat = DanbiManager.instance.videoDisplay.transform.rotation * Quaternion.Euler(0.0f, 0.0f, m_swipedRightAngle);
            Debug.Log($"R: {m_swipedRightAngle}");
            var dstQuat = DanbiManager.instance.m_videoDisplay.transform.rotation * Quaternion.Euler(0.0f, m_swipedRightAngle, 0.0f);
            // DanbiManager.instance.videoDisplay.transform.rotation = dstQuat;
            // dstQuat.eulerAngles = new Vector3(dstQuat.eulerAngles.x, 90.0f, 90.0f);
            DanbiManager.instance.m_videoDisplay.transform.localRotation = dstQuat;
            Danbi.DanbiSwipeToRightTimer.instance.StartChecking();
        }

        void OnSwipeLeftCompleted()
        {
            // m_anyLeftDetected = false;
            m_swipedLeftAngle = 0.0f;
            m_prevSwipeLeftProgress = 0.0f;
            Danbi.DanbiSwipeToLeftTimer.instance.EndChecking();
        }

        void OnSwipeRightCompleted()
        {
            // m_anyRightDetected = false;
            m_swipedRightAngle = 0.0f;
            m_prevSwipeRightProgress = 0.0f;
            Danbi.DanbiSwipeToRightTimer.instance.EndChecking();
        }
    };
};
