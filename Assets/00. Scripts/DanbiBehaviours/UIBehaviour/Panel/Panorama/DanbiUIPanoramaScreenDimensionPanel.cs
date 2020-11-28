using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiUIPanoramaScreenDimensionPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        DanbiUIPanoramaCubeDimension m_cube = new DanbiUIPanoramaCubeDimension();

        [SerializeField, Readonly]
        DanbiUIPanoramaCylinderDimension m_cylinder = new DanbiUIPanoramaCylinderDimension();

        int m_prevSelectedPanel = 0;

        int m_selectedPanel = 0;

        GameObject[] m_dimensionPanel = new GameObject[2];

        public delegate void OnTypeChange(int selectedPanel);
        public static OnTypeChange onTypeChange;

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

            m_cube.BindInput(m_dimensionPanel[0].transform);
            m_cylinder.BindInput(m_dimensionPanel[1].transform);

            onTypeChange += Caller_OnTypeChanged;
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("PanoramaCubeDimension-width", m_cube.m_width);
            PlayerPrefs.SetFloat("PanoramaCubeDimension-depth", m_cube.m_depth);
            PlayerPrefs.SetFloat("PanoramaCubeDimension-ch", m_cube.m_ch);
            PlayerPrefs.SetFloat("PanoramaCubeDimension-cl", m_cube.m_cl);

            PlayerPrefs.SetFloat("PanoramaCylinderDimension-radius", m_cylinder.m_radius);
            PlayerPrefs.SetFloat("PanoramaCylinderDimension-ch", m_cylinder.m_ch);
            PlayerPrefs.SetFloat("PanoramaCylinderDimension-cl", m_cylinder.m_cl);
        }

        void Caller_OnTypeChanged(int selectedPanel)
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
