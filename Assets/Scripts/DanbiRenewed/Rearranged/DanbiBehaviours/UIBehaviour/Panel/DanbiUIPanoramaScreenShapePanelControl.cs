using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenShapePanelControl : DanbiUIPanelControl
    {
        public DanbiUIPanoramaCube Cube;
        public DanbiUIPanoramaCylinder Cylinder;
        int selectedPanel = 0;
        new GameObject[] Panel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        new void Start()
        {
            Cube = new DanbiUIPanoramaCube(this);
            Cylinder = new DanbiUIPanoramaCylinder(this);

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
                AddListenerForPanelFields();
            }
        }

        protected override void AddListenerForPanelFields()
        {
            if (Panel[selectedPanel].Null())
            {
                return;
            }

            var panel = Panel[selectedPanel].transform;
            switch (selectedPanel)
            {
                case 0:
                    Cube.BindInput(panel);
                    break;

                case 1:
                    Cylinder.BindInput(panel);
                    break;
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
