using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Kinect;
using System.Text;

namespace Danbi
{
    public class DanbiKinectControl : MonoBehaviour
    {
        [Readonly]
        public EDanbiGestureState GestureState;

        [SerializeField, Readonly]
        bool anyMovement = false;

        // [Readonly]
        public GameObject Player;
        DanbiInteractionClapping clappingScript;

        KinectSensor sensor;
        /// <summary>
        /// 
        /// </summary>
        ColorFrameReader colorFrameReader;
        byte[] colorData;
        Texture2D colorTex;
        BodyFrameReader bodyFrameReader;
        int bodyCount;
        Body[] bodies;
        Color[] bodyColors;
        List<GestureDetector> gestureDetectors = new List<GestureDetector>();

        [SerializeField, Readonly]
        float DetectIntensity_walking;

        [SerializeField, Readonly]
        float DetectIntensity_swipeRtoL;

        [SerializeField, Readonly]
        float DetectIntensity_swipeLtoR;

        void Start()
        {
            clappingScript = Player.GetComponent<DanbiInteractionClapping>();

            sensor = KinectSensor.GetDefault();
            if (sensor is null)
            {
                // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, "<color=red>ERROR! sensor didn't connected!</color>");
                Debug.LogError($"<color=red>sensor isn't detected yet!</color>");
                return;
            }

            bodyCount = sensor.BodyFrameSource.BodyCount;
            colorFrameReader = sensor.ColorFrameSource.OpenReader();
            if (colorFrameReader is null)
            {
                Debug.LogError($"<color=red>Color Frame Reador failed to retrieve from KinectSensor!</color>");
                return;
            }
            // Create buffer from RGBA frame description.
            var desc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            if (bodyFrameReader is null)
            {
                Debug.LogError($"<color=red>Body Frame Reader failed to retrieve from KinectSensor!</color>");
                return;
            }

            // Init the gesture detection objects for our gestures.
            for (var i = 0; i < bodyCount; ++i)
            {
                // TODO: put updated UI stuff here for no gesture detection!
                // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, "<color=magenta>nothing detected!</color>");

                gestureDetectors.Add(new GestureDetector(sensor));
            }
        }

        void Update()
        {
            // https://hongjinhyeon.tistory.com/92
            //
            // + You don't have to check the IDisposable object for null. using will not throw an exception and Dispose() will not be called:
            bool isNewBodyDataDetected = false;
            using (var bodyFrame = bodyFrameReader.AcquireLatestFrame())
            {
                bodyFrame.GetAndRefreshBodyData(bodies);
                isNewBodyDataDetected = true;
            }

            if (isNewBodyDataDetected)
            {
                // update gesture detectors with the correct tracking ID.
                for (var i = 0; i < bodyCount; ++i)
                {
                    var body = bodies[i];
                    if (!(body is null))
                    {
                        var newTrackingID = body.TrackingId;
                        // if the current body TrackingID changed, update the corresponding gesture detector with a new value.
                        if (newTrackingID != gestureDetectors[i].TrackingId)
                        {
                            gestureDetectors[i].TrackingId = newTrackingID;
                            gestureDetectors[i].IsPaused = newTrackingID == 0;
                            // If the current body is tracked, resume the detector to get VisualGestureBuilderFrameArrived events,
                            // otherwise pause the detector so we don't waste any resources that trying to get invalid gesture results.
                            gestureDetectors[i].OnGestureDetected += Caller_makeOnGestureDetected(i);
                        }
                    }
                    else
                    {
                        // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, $"<color=red>ERROR! Can't tracking {bodyCount}th body!</color>");
                    }
                }
            }
        }

        EventHandler<GestureEventArgs> Caller_makeOnGestureDetected(int bodyIndex) =>
            (object sender, GestureEventArgs e) => OnGestureDetected(sender, e, bodyIndex);

        void OnGestureDetected(object sender, GestureEventArgs e, int bodyIndex)
        {

            Debug.Log($"Detected! {e.GestureID}");
            var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;
            // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, $"<color=teal>Detected! {isDetected}</color>");

            if (e.GestureID == DanbiInteractionHelper.GestureWalking)
            {
                GestureState = EDanbiGestureState.Walking;

                if (e.DetectionConfidence > DetectIntensity_walking)
                {
                    anyMovement = true;
                }
                else
                {
                    anyMovement = false;
                }
            }

            if (e.GestureID == DanbiInteractionHelper.GestureSwipeLtoR)
            {
                GestureState = EDanbiGestureState.SwipeLeftToRight;

                if (e.DetectionConfidence > DetectIntensity_swipeLtoR)
                {
                    anyMovement = true;
                }
                else
                {
                    anyMovement = false;
                }
            }

            if (e.GestureID == DanbiInteractionHelper.GestureSwipeRtoL)
            {
                GestureState = EDanbiGestureState.SwipeRightToLeft;

                if (e.DetectionConfidence > DetectIntensity_swipeRtoL)
                {
                    anyMovement = true;
                }
                else
                {
                    anyMovement = false;
                }
            }
        }

        void OnApplicationQuit()
        {
            if (!(colorFrameReader is null))
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if (!(bodyFrameReader is null))
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            if (!(sensor is null))
            {
                if (sensor.IsOpen)
                {
                    sensor.Close();
                }
                sensor = null;
            }
        }
    };
};
