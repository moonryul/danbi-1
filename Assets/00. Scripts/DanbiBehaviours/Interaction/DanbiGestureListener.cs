using UnityEngine;
using TMPro;
using System.Collections;
using System;
//using Windows.Kinect;

public class DanbiGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [SerializeField, Readonly, Space(5), Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    int m_playerIndex = 0;

    public delegate void OnSwipeLeftDetected(float progress);
    public static OnSwipeLeftDetected onSwipeLeftDetected;

    public delegate void OnSwipeRightDetected(float progress);
    public static OnSwipeRightDetected onSwipeRightDetected;

    public delegate void OnSwipeLeftComplete();
    public static OnSwipeLeftComplete onSwipeLeftComplete;

    public delegate void OnSwipeRightComplete();
    public static OnSwipeRightComplete onSwipeRightComplete;

    public delegate void OnWalkDetected();
    public static OnWalkDetected onWalkDetected;

    public delegate void OnWalkComplete();
    public static OnWalkComplete onWalkComplete;

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

        // m_gestureInfoText.text = "Swipe left, right or up to change the slides.";
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

        // m_gestureInfoText.text = default;
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

        // respond when the progress is more than 0.1 in the case of swipe
        if (gesture == KinectGestures.Gestures.SwipeLeft && progress > 0.1f)
        {
            onSwipeLeftDetected?.Invoke(progress);
        }
        // respond when the progress is more than 0.1 in the case of swipe
        else if (gesture == KinectGestures.Gestures.SwipeRight && progress > 0.1f)
        {
            onSwipeRightDetected?.Invoke(progress);
        }
        // respond when the progress is more than 0.5 in the case of walk 
        // walk is detected by the difference between the height of the left and the right knee.
        else if (gesture == KinectGestures.Gestures.Walk && progress == 1.0f)
        {
            onWalkDetected?.Invoke();            
        }
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

        if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            onSwipeLeftComplete?.Invoke();
        }
        else if (gesture == KinectGestures.Gestures.SwipeRight)
        {
            onSwipeRightComplete?.Invoke();
        }
        else if (gesture == KinectGestures.Gestures.Walk)
        {
            onWalkComplete?.Invoke();
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

        // if (m_progressDisplayed)
        // {
        //     m_progressDisplayed = false;
        //     m_gestureInfoText.text = default;
        // }

        return true;
    }

    void Awake()
    {
        Danbi.DanbiUISync.onPanelUpdate += OnPanelUpdate;
    }

    void Update()
    {
        // if (m_progressDisplayed && ((Time.realtimeSinceStartup - m_progressGestureTime) > 2f))
        // {
        //     m_progressDisplayed = false;
        //     m_gestureInfoText.text = default;

        //     Debug.Log("Forced progress to end.");
        // }
    }

    void OnPanelUpdate(Danbi.DanbiUIPanelControl control)
    {

    }
}
