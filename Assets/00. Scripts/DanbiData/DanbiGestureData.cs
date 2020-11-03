using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    /// <summary>
    /// Programmatic gesture data container.
    /// </summary>
    public struct DanbiGestureData
    {
        public long userId;
        public EDanbiGestures gesture;
        public int state;
        public float timestamp;
        public int joint;
        public Vector3 jointPos;
        public Vector3 screenPos;
        public float tagFloat;
        public Vector3 tagVector;
        public Vector3 tagVector2;
        public float progress;
        public bool complete;
        public bool cancelled;
        public List<EDanbiGestures> checkForGestures;
        public float startTrackingAtTime;
    };
};
