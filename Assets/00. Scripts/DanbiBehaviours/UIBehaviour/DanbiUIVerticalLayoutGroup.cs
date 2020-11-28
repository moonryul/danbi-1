using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVerticalLayoutGroup : MonoBehaviour
    {
        void Start()
        {
            // Reset the vertical layout group spacing to 0.0
            GetComponent<VerticalLayoutGroup>().spacing = 0.0f;
        }
    };
};
