using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorDimensionPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiUIReflectorHalfsphere Halfsphere;

        [HideInInspector]
        public DanbiUIReflectorCone Cone;
        
        // public DanbiUIReflectorParaboloid Paraboloid;

        int selectedPanel = 0;

        new readonly GameObject[] Panel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);

        public static OnTypeChanged Call_OnTypeChanged;

        void Start()
        {
            Cone = new DanbiUIReflectorCone(this);
            Halfsphere = new DanbiUIReflectorHalfsphere(this);
            // Paraboloid = new DanbiUIReflectorParaboloid(this);
            // i == 0 => Cube
            // i == 1 => Cylinder                      
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

            Panel[0].gameObject.SetActive(true);
            for (int i = 1; i < Panel.Length; ++i)
            {
                Panel[i].gameObject.SetActive(false);
            }

            AddListenerForPanelFields();
            Call_OnTypeChanged += Caller_OnTypeChanged;
        }


        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ReflectorHalfsphere-distance", Halfsphere.distance);
            PlayerPrefs.SetFloat("ReflectorHalfsphere-height", Halfsphere.height);
            PlayerPrefs.SetFloat("ReflectorHalfsphere-radius", Halfsphere.diameter);

            PlayerPrefs.SetFloat("ReflectorCone-distance", Cone.distance);
            PlayerPrefs.SetFloat("ReflectorCone-height", Cone.height);
            PlayerPrefs.SetFloat("ReflectorCone-radius", Cone.radius);
        }

        void Caller_OnTypeChanged(int selectedPanel)
        {
            bool isChanged = this.selectedPanel != selectedPanel;
            this.selectedPanel = selectedPanel;

            // Popup the corresponding panel.
            if (isChanged)
            {
                for (int i = 0; i < Panel.Length; ++i)
                {
                    if (i == this.selectedPanel)
                    {
                        Panel[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        Panel[i].gameObject.SetActive(false);
                    }

                }
                AddListenerForPanelFields();
            }
        }

        protected override void AddListenerForPanelFields()
        {
            if (Panel[selectedPanel].Null())
            {
                return;
            }

            for (int i = 0; i < Panel.Length; ++i)
            {
                var panel = Panel[i].transform;
                switch (selectedPanel)
                {
                    case 0:
                        Halfsphere.BindInput(panel);
                        break;

                    case 1:
                        Cone.BindInput(panel);
                        break;

                    case 2:
                        //Paraboloid.BindInput(panel);
                        break;
                }
            }
        } // BindPanelField()

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            if (isPanelOpened)
            {
                if (lastClicked.Count > 0)
                {
                    lastClicked.Pop();
                }
            }

            isPanelOpened = !isPanelOpened;
            Panel[selectedPanel].SetActive(isPanelOpened);
        }
    };
};
