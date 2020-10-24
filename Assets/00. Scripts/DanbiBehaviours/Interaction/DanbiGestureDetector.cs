using System.Collections;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using UnityEngine;

namespace Danbi
{
    public class DanbiGestureDetector : System.IDisposable
    {
        [SerializeField, Readonly]
        string GDBFilePathAndLocation;
        string GDBFilePath;

        // [SerializeField, Readonly]
        // string GestureID

        /// <summary>
        /// Gesture frame source to which should be bound a body tracking ID.
        /// </summary>
        VisualGestureBuilderFrameSource vgbFrameSrc;
        /// <summary>
        /// Gesture frame reader to which handles gesture events coming from the sensor.<!---->
        /// </summary>
        VisualGestureBuilderFrameReader vgbFrameReader;

        public event System.EventHandler<DanbiGestureEventArg> Call_OnGesturesDetected;

        /// <summary>
        /// Tracking ID is changeable along the body comes in / out of the range of the sensor.
        /// </summary>
        /// <value></value>
        public ulong trackingID
        {
            get => vgbFrameSrc.TrackingId;
            set => vgbFrameSrc.TrackingId = value;
        }
        /// <summary>
        /// Detector pause when the body trackingID isn't valid.
        /// </summary>
        /// <value></value>
        public bool isPaused
        {
            get => vgbFrameReader.IsPaused;
            set => vgbFrameReader.IsPaused = value;
        }

        public DanbiGestureDetector(string gdbFilePathAndLocation, KinectSensor sensor)
        {
            if (sensor is null)
            {
                throw new System.ArgumentNullException("Kinect Sensor is null!");
            }

            // Create the VGB source. the associated body tracking ID will be set when a valid body frame arrvies
            // from the sensor.
            vgbFrameSrc = VisualGestureBuilderFrameSource.Create(sensor, 0);
            //
            vgbFrameSrc.TrackingIdLost += (object sender, TrackingIdLostEventArgs e) =>
            {
                Call_OnGesturesDetected?.Invoke(this, new DanbiGestureEventArg(false, false, 0.0f, "none"));
            };

            // open the reader for the vgb fram
            vgbFrameReader = vgbFrameSrc.OpenReader();
            if (vgbFrameReader != null)
            {
                vgbFrameReader.IsPaused = true;
                vgbFrameReader.FrameArrived += OnGestureFrameArrived;
            }

            // Load the "Seated" gesture from the gesture database file (.gdb).
            using (var db = VisualGestureBuilderDatabase.Create(gdbFilePathAndLocation))
            {
                // we could load all available gestures in the database with a call to vgbFrameSrc.AddGestures(db.AvailableGestures);
                // but for this program, we only want to track on discrete gesture from the database.
                foreach (var g in db.AvailableGestures)
                {
                    vgbFrameSrc.AddGesture(g);
                    DanbiUIInteractionKinectPanelControl.Call_OnGestureIDAdded?.Invoke(g.Name);
                    // TODO: Update when adding a gesture.
                }
            }
        }

        static DanbiGestureDetector makeDanbiGestureDetector(string gdbFilePathAndLocation, KinectSensor sensor)
        {
            var newDetector = new DanbiGestureDetector(gdbFilePathAndLocation, sensor);
            return newDetector;
        }

        void OnGestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            var frameRef = e.FrameReference;
            using (var frame = frameRef.AcquireFrame())
            {
                if (frame is null)
                {
                    Debug.LogError($"<color=red>Arrived Gesture Frame is Null!</color>");
                    return;
                }

                // get the discrete gesture results which arrvied with the latest frame.
                var discreteResults = frame.DiscreteGestureResults;

                if (discreteResults is null)
                {
                    Debug.LogError($"<color=red>No discrete gesture results has been found!</color>");
                    return;
                }

                foreach (var g in vgbFrameSrc.Gestures)
                {
                    if (g.GestureType == GestureType.Discrete && discreteResults.TryGetValue(g, out var res))
                    {
                        // TODO: Update the ID when the gesture frame has been received from the sensor.
                        Call_OnGesturesDetected?.Invoke(this, new DanbiGestureEventArg(true, res.Detected, res.Confidence, g.Name));
                    }
                }
            }
        }

        public void Dispose()
        {
            if (vgbFrameReader != null)
            {
                vgbFrameReader.FrameArrived -= OnGestureFrameArrived;
                vgbFrameReader.Dispose();
                vgbFrameReader = null;
            }

            if (vgbFrameSrc != null)
            {
                vgbFrameSrc.Dispose();
                vgbFrameSrc = null;
            }

            System.GC.SuppressFinalize(this);
        }
    };
};
