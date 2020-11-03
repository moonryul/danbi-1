using UnityEngine;
using TMPro;
using System.Collections;
using System;
//using Windows.Kinect;

public class DanbiGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [SerializeField, Readonly, Space(5), Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    int m_playerIndex = 0;

    TMP_Text m_gestureInfoText;

    // internal variables to track if progress message has been displayed
    bool m_progressDisplayed;
    float m_progressGestureTime;

    // whether the needed gesture has been detected or not
    bool m_isSwipeLeft = false;
    public bool isSwipeLeft => !(m_isSwipeLeft = !m_isSwipeLeft);

    bool m_isSwipeRight = false;
    public bool isSwipeRight => !(m_isSwipeRight = !m_isSwipeRight);

    bool m_isWalking = false;
    public bool isWalking => m_isWalking;

    float m_swipeAngle;

    public delegate void OnSwipeLeftDetected(float progress);
    public static OnSwipeLeftDetected onSwipeLeftDetected;

    public delegate void OnSwipeRightDetected(float progress);
    public static OnSwipeRightDetected onSwipeRightDetected;

    public delegate void OnSwipeLeftCompleted();
    public static OnSwipeLeftCompleted onSwipeLeftCompleted;

    public delegate void OnSwipeRightCompleted();
    public static OnSwipeRightCompleted onSwipeRightCompleted;


    public delegate void OnWalkDetected();
    public static OnWalkDetected onWalkDetected;

    /// <summary>
    /// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    public void UserDetected(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only        
        if (userIndex != m_playerIndex)
        {
            return;
        }

        // detect these user specific gestures.
        // Declare what gestures to track.
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Walk);

        m_gestureInfoText.text = "Swipe left, right or up to change the slides.";
    }

    /// <summary>
    /// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    public void UserLost(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != m_playerIndex)
        {
            return;
        }

        m_gestureInfoText.text = default;
    }

    /// <summary>
    /// Invoked when a gesture is in progress.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="progress">Gesture progress [0..1]</param>
    /// <param name="joint">Joint type</param>
    /// <param name="screenPos">Normalized viewport position</param>
    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != m_playerIndex)
        {
            return;
        }

        if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            onSwipeLeftDetected?.Invoke(progress);
        }
        else if (gesture == KinectGestures.Gestures.SwipeRight)
        {
            onSwipeRightDetected?.Invoke(progress);
        }
        else if (gesture == KinectGestures.Gestures.Walk && progress > 0.5f)
        {
            if (m_gestureInfoText != null)
            {
                string sGestureText = string.Format("{0} - progress: {1:F0}%", gesture, progress * 100);
                Debug.Log(sGestureText);
                m_gestureInfoText.text = sGestureText;

                m_progressDisplayed = true;
                m_progressGestureTime = Time.realtimeSinceStartup;
            }
        }

        #region unused
        // else if ((gesture == KinectGestures.Gestures.ZoomOut || gesture == KinectGestures.Gestures.ZoomIn) && progress > 0.5f)
        // {
        //     if (gestureInfo != null)
        //     {
        //         string sGestureText = string.Format("{0} - {1:F0}%", gesture, screenPos.z * 100f);
        //         gestureInfo.text = sGestureText;

        //         progressDisplayed = true;
        //         progressGestureTime = Time.realtimeSinceStartup;
        //     }
        // }
        // else if ((gesture == KinectGestures.Gestures.Wheel || gesture == KinectGestures.Gestures.LeanLeft ||
        //          gesture == KinectGestures.Gestures.LeanRight) && progress > 0.5f)
        // {
        //     if (gestureInfo != null)
        //     {
        //         string sGestureText = string.Format("{0} - {1:F0} degrees", gesture, screenPos.z);
        //         gestureInfo.text = sGestureText;

        //         progressDisplayed = true;
        //         progressGestureTime = Time.realtimeSinceStartup;
        //     }
        // }        
        #endregion unused
    }

    /// <summary>
    /// Invoked if a gesture is completed.
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="joint">Joint type</param>
    /// <param name="screenPos">Normalized viewport position</param>
    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != m_playerIndex)
        {
            return false;
        }

        m_gestureInfoText.text = $"{gesture} detected";

        if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            onSwipeLeftCompleted?.Invoke();
        }
        else
        if (gesture == KinectGestures.Gestures.SwipeRight)
        {
            onSwipeLeftCompleted?.Invoke();
        }

        return true;
    }

    /// <summary>
    /// Invoked if a gesture is cancelled.
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="joint">Joint type</param>
    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != m_playerIndex)
        {
            return false;
        }

        if (m_progressDisplayed)
        {
            m_progressDisplayed = false;
            m_gestureInfoText.text = default;
        }

        return true;
    }

    void Awake()
    {
        Danbi.DanbiUISync.onPanelUpdated += OnPanelUpdate;
    }

    void Update()
    {
        if (m_progressDisplayed && ((Time.realtimeSinceStartup - m_progressGestureTime) > 2f))
        {
            m_progressDisplayed = false;
            m_gestureInfoText.text = default;

            Debug.Log("Forced progress to end.");
        }
    }

    void OnPanelUpdate(Danbi.DanbiUIPanelControl control)
    {
        if (control is Danbi.DanbiUIInteractionSwipeToLeftPanelControl)
        {
            var swipeControl = control as Danbi.DanbiUIInteractionSwipeToLeftPanelControl;

            m_swipeAngle = swipeControl.m_swipeAngle;
        }
    }
}
