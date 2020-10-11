using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVerticalLayoutGroupControl : MonoBehaviour
    {
        void Start()
        {
            GetComponent<VerticalLayoutGroup>().spacing = 0.0f;
        }
    };
};
