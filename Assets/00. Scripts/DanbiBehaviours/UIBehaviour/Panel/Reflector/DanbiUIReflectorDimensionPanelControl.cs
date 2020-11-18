using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorDimensionPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiUIReflectorDome Dome;

        [HideInInspector]
        public DanbiUIReflectorCone Cone;

        // public DanbiUIReflectorParaboloid Paraboloid;
        int prevSelectedPanel = 0;
        int selectedPanel = 0;

        readonly GameObject[] DimensionPanel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);

        public static OnTypeChanged Call_OnTypeChanged;

        void Start()
        {
            Dome = new DanbiUIReflectorDome(this);
            Cone = new DanbiUIReflectorCone(this);
            // Paraboloid = new DanbiUIReflectorParaboloid(this);

            for (int i = 0; i < 2; ++i)
            {
                DimensionPanel[i] = transform.GetChild(i + 1).gameObject;
                if (!DimensionPanel[i].name.Contains("Panel"))
                {
                    DimensionPanel[i] = null;
                }
                else
                {
                    var parentSize = transform.parent.GetComponent<RectTransform>().rect;
                    DimensionPanel[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 0);
                }
            }

            for (int i = 0; i < DimensionPanel.Length; ++i)
            {
                DimensionPanel[i].gameObject.SetActive(false);
            }

            Dome.BindInput(DimensionPanel[0].transform);
            Cone.BindInput(DimensionPanel[1].transform);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }


        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ReflectorDome-distance", Dome.distance);
            PlayerPrefs.SetFloat("ReflectorDome-height", Dome.height);
            PlayerPrefs.SetFloat("ReflectorDome-radius", Dome.bottomRadius);
            PlayerPrefs.SetFloat("ReflectorDome-maskingRatio", Dome.maskingRatio);

            PlayerPrefs.SetFloat("ReflectorCone-distance", Cone.distance);
            PlayerPrefs.SetFloat("ReflectorCone-height", Cone.height);
            PlayerPrefs.SetFloat("ReflectorCone-radius", Cone.radius);
        }

        void Caller_OnTypeChanged(int selectedPanel)
        {
            prevSelectedPanel = this.selectedPanel;
            this.selectedPanel = selectedPanel;

            for (int i = 0; i < DimensionPanel.Length; ++i)
            {
                DimensionPanel[i].gameObject.SetActive(false);
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

            DimensionPanel[prevSelectedPanel].SetActive(false);
            isPanelOpened = !isPanelOpened;
            DimensionPanel[selectedPanel].SetActive(isPanelOpened);
        }
    };
};
