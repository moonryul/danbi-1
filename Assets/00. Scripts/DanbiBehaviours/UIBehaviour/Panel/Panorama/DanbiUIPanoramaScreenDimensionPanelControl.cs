using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    #pragma warning disable 3001
    public class DanbiUIPanoramaScreenDimensionPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public DanbiUIPanoramaCubeDimension Cube;
        [Readonly]
        public DanbiUIPanoramaCylinderDimension Cylinder;
        int prevSelectedPanel = 0;
        int selectedPanel = 0;
        GameObject[] DimensionPanel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        void Start()
        {
            Cube = new DanbiUIPanoramaCubeDimension(this);
            Cylinder = new DanbiUIPanoramaCylinderDimension(this);

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

            Cube.BindInput(DimensionPanel[0].transform);
            Cylinder.BindInput(DimensionPanel[1].transform);

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
