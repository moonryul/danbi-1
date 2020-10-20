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

        new readonly GameObject[] Panel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);

        public static OnTypeChanged Call_OnTypeChanged;

        void Start()
        {
            Dome = new DanbiUIReflectorDome(this);
            Cone = new DanbiUIReflectorCone(this);
            // Paraboloid = new DanbiUIReflectorParaboloid(this);

            for (int i = 0; i < 2; ++i)
            {
                Panel[i] = transform.GetChild(i + 1).gameObject;
                if (!Panel[i].name.Contains("Panel"))
                {
                    Panel[i] = null;
                }
                else
                {
                    var parentSize = transform.parent.GetComponent<RectTransform>().rect;
                    Panel[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 0);
                }
            }

            for (int i = 0; i < Panel.Length; ++i)
            {
                Panel[i].gameObject.SetActive(false);
            }

            Dome.BindInput(Panel[0].transform);
            Cone.BindInput(Panel[1].transform);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }


        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ReflectorDome-distance", Dome.distance);
            PlayerPrefs.SetFloat("ReflectorDome-height", Dome.height);
            PlayerPrefs.SetFloat("ReflectorDome-radius", Dome.radius);
            PlayerPrefs.SetFloat("ReflectorDome-maskingRatio", Dome.maskingRatio);

            PlayerPrefs.SetFloat("ReflectorCone-distance", Cone.distance);
            PlayerPrefs.SetFloat("ReflectorCone-height", Cone.height);
            PlayerPrefs.SetFloat("ReflectorCone-radius", Cone.radius);
        }

        void Caller_OnTypeChanged(int selectedPanel)
        {
            prevSelectedPanel = this.selectedPanel;
            this.selectedPanel = selectedPanel;

            for (int i = 0; i < Panel.Length; ++i)
            {
                Panel[i].gameObject.SetActive(false);
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

            Panel[prevSelectedPanel].SetActive(false);
            isPanelOpened = !isPanelOpened;
            Panel[selectedPanel].SetActive(isPanelOpened);
        }
    };
};
