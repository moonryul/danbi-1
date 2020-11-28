using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorDimensionPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        DanbiUIReflectorDome m_dome = new DanbiUIReflectorDome();

        [SerializeField, Readonly]
        DanbiUIReflectorCone m_cone = new DanbiUIReflectorCone();

        int m_prevSelectedPanel = 0;
        int m_selectedPanel = 0;

        readonly GameObject[] m_dimensionPanel = new GameObject[2];

        public delegate void OnTypeChange(int selectedPanel);

        public static OnTypeChange onTypeChanged;

        void Start()
        {
            for (int i = 0; i < 2; ++i)
            {
                m_dimensionPanel[i] = transform.GetChild(i + 1).gameObject;
                if (!m_dimensionPanel[i].name.Contains("Panel"))
                {
                    m_dimensionPanel[i] = null;
                }
                else
                {
                    var parentSize = transform.parent.GetComponent<RectTransform>().rect;
                    m_dimensionPanel[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 0);
                }
            }

            for (int i = 0; i < m_dimensionPanel.Length; ++i)
            {
                m_dimensionPanel[i].gameObject.SetActive(false);
            }

            m_dome.BindInput(m_dimensionPanel[0].transform);
            m_cone.BindInput(m_dimensionPanel[1].transform);

            onTypeChanged += ChangeType;
        }


        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ReflectorDome-distance", m_dome.m_distance);
            PlayerPrefs.SetFloat("ReflectorDome-height", m_dome.m_height);
            PlayerPrefs.SetFloat("ReflectorDome-radius", m_dome.m_bottomRadius);
            PlayerPrefs.SetFloat("ReflectorDome-maskingRatio", m_dome.m_maskingRatio);

            PlayerPrefs.SetFloat("ReflectorCone-distance", m_cone.m_distance);
            PlayerPrefs.SetFloat("ReflectorCone-height", m_cone.m_height);
            PlayerPrefs.SetFloat("ReflectorCone-radius", m_cone.m_radius);
        }

        void ChangeType(int selectedPanel)
        {
            m_prevSelectedPanel = this.m_selectedPanel;
            this.m_selectedPanel = selectedPanel;

            for (int i = 0; i < m_dimensionPanel.Length; ++i)
            {
                m_dimensionPanel[i].gameObject.SetActive(false);
            }
        }

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            if (isPanelOpened)
            {
                if (lastClicked.Count > 0)
                {
                    lastClicked.Pop();
                }
            }

            m_dimensionPanel[m_prevSelectedPanel].SetActive(false);
            isPanelOpened = !isPanelOpened;
            m_dimensionPanel[m_selectedPanel].SetActive(isPanelOpened);
        }
    };
};
