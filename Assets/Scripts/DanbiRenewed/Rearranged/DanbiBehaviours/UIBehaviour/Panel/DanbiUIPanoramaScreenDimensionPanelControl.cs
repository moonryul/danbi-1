using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenDimensionPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public DanbiUIPanoramaCubeDimension Cube;
        [Readonly]
        public DanbiUIPanoramaCylinderDimension Cylinder;
        int prevSelectedPanel = 0;
        int selectedPanel = 0;
        new GameObject[] Panel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        new void Start()
        {
            Cube = new DanbiUIPanoramaCubeDimension(this);
            Cylinder = new DanbiUIPanoramaCylinderDimension(this);

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

            Cube.BindInput(Panel[0].transform);
            Cylinder.BindInput(Panel[1].transform);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("PanoramaCubeDimension-width", Cube.width);
            PlayerPrefs.SetFloat("PanoramaCubeDimension-depth", Cube.depth);
            PlayerPrefs.SetFloat("PanoramaCubeDimension-ch", Cube.ch);
            PlayerPrefs.SetFloat("PanoramaCubeDimension-cl", Cube.cl);

            PlayerPrefs.SetFloat("PanoramaCylinderDimension-radius", Cylinder.radius);
            PlayerPrefs.SetFloat("PanoramaCylinderDimension-ch", Cylinder.ch);
            PlayerPrefs.SetFloat("PanoramaCylinderDimension-cl", Cylinder.cl);
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
