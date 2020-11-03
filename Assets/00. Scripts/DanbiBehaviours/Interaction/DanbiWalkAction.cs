using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiWalkAction : MonoBehaviour
    {
        [SerializeField, Readonly]
        bool m_isPaused;

        void Start()
        {
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
        }

        void OnDisable()
        {

        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            
        }
    };
};
