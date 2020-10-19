using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiInteractionClapping : MonoBehaviour
    {
        [Readonly]
        public bool isClapping = false;

        void FixedUpdate()
        {
            if (isClapping)
            {
                Debug.Log("Clapping!");
            }
        }
    };
};
