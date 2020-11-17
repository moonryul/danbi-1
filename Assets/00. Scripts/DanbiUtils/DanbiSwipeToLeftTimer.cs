using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiSwipeToLeftTimer : SingletonAsComponent<DanbiWalkTimer>
    {
        public float startTime = 0.0f;        

        bool isTrackingTime = false;

        public void StartChecking()
        {            
            isTrackingTime = true;
        }

        public void EndChecking()
        {
            isTrackingTime = false;
            Debug.Log($"From the Swipe To Left start to the video play start : {startTime} ms");
            startTime = 0.0f;
        }

        void Update()
        {
            if (!isTrackingTime)
            {                
                return;
            }

            startTime += Time.deltaTime;
        }

    };
};
