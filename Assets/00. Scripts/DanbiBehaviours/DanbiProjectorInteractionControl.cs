using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiProjectorInteractionControl : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        bool m_isVideoProjected;

        [SerializeField, Readonly]
        bool m_useInteraction;

        public bool useInteraction { get => m_useInteraction; set => m_useInteraction = value; }

        DanbiManager m_danbiManager;
        DanbiProjectorControl m_projectorControl;

        void Awake()
        {
            m_danbiManager = GetComponent<DanbiManager>();
            m_projectorControl = GetComponent<DanbiProjectorControl>();
        }

        void Update()
        {
            if (m_danbiManager.simulatorMode != EDanbiSimulatorMode.Project)
            {
                return;
            }

            if (!m_useInteraction)
            {
                return;
            }

            // 1. detect "walk" and interact

            // 2. detect "swipe to left" and interact

            // 3. detect "swipe to right" and interact  
        }
    };
};
