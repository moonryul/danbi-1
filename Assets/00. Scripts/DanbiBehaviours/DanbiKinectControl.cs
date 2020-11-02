using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Kinect;
using System.Text;
using TMPro;

namespace Danbi
{
    public class DanbiKinectControl : MonoBehaviour
    {
        [Readonly]
        public EDanbiGestureState GestureState;

        [SerializeField, Readonly]
        bool anyMovement = false;

        // [Readonly]
        // public GameObject Player;

        // DanbiInteractionClapping clappingScript;

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

        List<DanbiGestureDetector> gestureDetectors = new List<DanbiGestureDetector>();

        [SerializeField]
        float walkingDetectionConfiance = 0.65f;

        [SerializeField]
        float swipeRightToLeftDetectionConfiance = 0.65f;

        [SerializeField]
        float swipeLeftToRightDetectionConfiance = 0.65f;

        [SerializeField, Readonly]
        string gdbFilePathAndLocation;

        public TMP_Text DetectedText;
        public TMP_Text Gesture1Text;
        public TMP_Text Gesture2Text;
        public TMP_Text Gesture3Text;

        public delegate void OnKinectInit(TMP_Text statusText);
        public static OnKinectInit Call_OnKinectInit;

        bool isInitialized;

        void Awake()
        {
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
            Call_OnKinectInit += Caller_Call_OnKinectInit;
        }

        void OnDisable()
        {
            Call_OnKinectInit -= Caller_Call_OnKinectInit;
        }

        void Caller_Call_OnKinectInit(TMP_Text statusText)
        {
            // clappingScript = Player.GetComponent<DanbiInteractionClapping>();
            DetectedText = GameObject.Find("Detected").GetComponent<TMP_Text>();

            sensor = KinectSensor.GetDefault();
            if (sensor is null)
            {
                // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, "<color=red>ERROR! sensor didn't connected!</color>");
                Debug.LogError($"<color=red>sensor isn't detected yet!</color>");
                DetectedText.text = "Connection failed!";
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
            bodies = new Body[bodyCount];
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
                DetectedText.text = "Nothing detected yet";
                gestureDetectors.Add(new DanbiGestureDetector(gdbFilePathAndLocation, sensor));
            }

            sensor.Open();
            isInitialized = true;
            statusText.text = $"Result : <color=#ff0000>{(isInitialized ? "success!" : "failed!")}";
        }

        void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            // https://hongjinhyeon.tistory.com/92
            //
            // + You don't have to check the IDisposable object for null. using will not throw an exception and Dispose() will not be called:
            bool isNewBodyDataDetected = false;
            using (var bodyFrame = bodyFrameReader.AcquireLatestFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    isNewBodyDataDetected = true;
                }
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
                        if (newTrackingID != gestureDetectors[i].trackingID)
                        {
                            gestureDetectors[i].trackingID = newTrackingID;
                            gestureDetectors[i].isPaused = newTrackingID == 0;
                            // If the current body is tracked, resume the detector to get VisualGestureBuilderFrameArrived events,
                            // otherwise pause the detector so we don't waste any resources that trying to get invalid gesture results.
                            gestureDetectors[i].Call_OnGesturesDetected += Caller_makeOnGestureDetected(i);
                        }
                    }
                    else
                    {
                        // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, $"<color=red>ERROR! Can't tracking {bodyCount}th body!</color>");
                    }
                }
            }
        }

        /// <summary>
        /// factory making a delegate to use one more param(index)
        /// </summary>
        /// <param name="bodyIndex"></param>
        /// <returns></returns>
        EventHandler<DanbiGestureEventArg> Caller_makeOnGestureDetected(int bodyIndex) =>
            (object sender, DanbiGestureEventArg e) => OnGestureDetected(sender, e, bodyIndex);

        void OnGestureDetected(object sender, DanbiGestureEventArg e, int bodyIndex)
        {
            Debug.Log($"Detected! {e.gestureID}");

            bool isDetected = e.isBodyTrackingIDValid && e.isGestureDetected;

            DetectedText.text = "Detected : " + isDetected;
            // DanbiUIInteractionDetectionPanel.Call_OnDetectStatusChanged?.Invoke(this, $"<color=teal>Detected! {isDetected}</color>");

            if (e.gestureID == DanbiInteractionHelper.GestureWalking)
            {
                GestureState = EDanbiGestureState.Walking;

                if (e.detectionConfiance > walkingDetectionConfiance)
                {
                    anyMovement = true;
                }
                else
                {
                    anyMovement = false;
                }
            }

            if (e.gestureID == DanbiInteractionHelper.GestureSwipeLtoR)
            {
                GestureState = EDanbiGestureState.SwipeLeftToRight;

                if (e.detectionConfiance > swipeLeftToRightDetectionConfiance)
                {
                    anyMovement = true;
                }
                else
                {
                    anyMovement = false;
                }
            }

            if (e.gestureID == DanbiInteractionHelper.GestureSwipeRtoL)
            {
                GestureState = EDanbiGestureState.SwipeRightToLeft;

                if (e.detectionConfiance > swipeRightToLeftDetectionConfiance)
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

        private void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIInteractionDatabasePanelControl)
            {
                var dbControl = control as DanbiUIInteractionDatabasePanelControl;

                gdbFilePathAndLocation = dbControl.gbdPath;
                // TODO: update the gdb file path or file itself.
                // TODO: update the gesture confiances.
            }
        }
    };
};
