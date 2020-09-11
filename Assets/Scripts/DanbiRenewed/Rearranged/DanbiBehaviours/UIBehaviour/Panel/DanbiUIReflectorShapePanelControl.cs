using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorShapePanelControl : DanbiUIPanelControl
    {
        DanbiUIReflectorCone Cone = new DanbiUIReflectorCone();
        DanbiUIReflectorHalfsphere Halfsphere = new DanbiUIReflectorHalfsphere();
        DanbiUIReflectorParaboloid Paraboloid = new DanbiUIReflectorParaboloid();

        int selectedPanel = 0;
        new GameObject[] Panel;

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        new void Start()
        {
            // i == 0 => Cube
            // i == 1 => Cylinder          
            Panel = new GameObject[2];
            for (int i = 0; i < 2; ++i)
            {
                var vertical = transform.GetChild(1);
                Panel[i] = vertical.GetChild(i).gameObject;
                if (!Panel[i].name.Contains("Panel"))
                {
                    Panel[i] = null;
                }
                else
                {
                    Panel[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }

            Panel[0].gameObject.SetActive(true);

            for (int i = 1; i < Panel.Length; ++i)
            {
                Panel[i].gameObject.SetActive(false);
            }

            BindPanelFields();
            //DanbiUIControl.Call_OnPanelDataUpdated(this);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }

        void Caller_OnTypeChanged(int selectedPanel)
        {
            bool isChanged = this.selectedPanel != selectedPanel;
            this.selectedPanel = selectedPanel;

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
            }
        }

        protected override void BindPanelFields()
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
                        Cone.BindInput(panel);
                        break;

                    case 1:
                        Halfsphere.BindInput(panel);
                        break;

                    case 2:
                        //Paraboloid.BindInput(panel);
                        break;
                }
            }


        } // BindPanelField()
    };
};
