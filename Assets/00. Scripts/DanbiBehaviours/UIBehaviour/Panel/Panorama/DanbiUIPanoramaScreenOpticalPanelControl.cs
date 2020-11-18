using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiUIPanoramaScreenOpticalPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public DanbiUIPanoramaCubeOptical Cube;
        [Readonly]
        public DanbiUIPanoramaCylinderOptical Cylinder;
        int prevSelectedPanel = 0;
        int selectedPanel = 0;
        GameObject[] OpticalPanel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        void Start()
        {
            Cube = new DanbiUIPanoramaCubeOptical(this);
            Cylinder = new DanbiUIPanoramaCylinderOptical(this);
            
            for (int i = 0; i < 2; ++i)
            {
                OpticalPanel[i] = transform.GetChild(i + 1).gameObject;
                if (!OpticalPanel[i].name.Contains("Panel"))
                {
                    OpticalPanel[i] = null;
                }
                else
                {
                    var parentSize = transform.parent.GetComponent<RectTransform>().rect;
                    OpticalPanel[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 0);
                }
            }

            for (int i = 0; i < OpticalPanel.Length; ++i)
            {
                OpticalPanel[i].gameObject.SetActive(false);
            }

            Cube.BindInput(OpticalPanel[0].transform);
            Cylinder.BindInput(OpticalPanel[1].transform);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("PanoramaCubeOptical-specularR", Cube.specularR);
            PlayerPrefs.SetFloat("PanoramaCubeOptical-specularG", Cube.specularG);
            PlayerPrefs.SetFloat("PanoramaCubeOptical-specularB", Cube.specularB);
            PlayerPrefs.SetFloat("PanoramaCubeOptical-emissionR", Cube.emissionR);
            PlayerPrefs.SetFloat("PanoramaCubeOptical-emissionG", Cube.emissionG);
            PlayerPrefs.SetFloat("PanoramaCubeOptical-emissionB", Cube.emissionB);

            PlayerPrefs.SetFloat("PanoramaCylinderOptical-specularR", Cylinder.specularR);
            PlayerPrefs.SetFloat("PanoramaCylinderOptical-specularG", Cylinder.specularG);
            PlayerPrefs.SetFloat("PanoramaCylinderOptical-specularB", Cylinder.specularB);
            PlayerPrefs.SetFloat("PanoramaCylinderOptical-emissionR", Cylinder.emissionR);
            PlayerPrefs.SetFloat("PanoramaCylinderOptical-emissionG", Cylinder.emissionG);
            PlayerPrefs.SetFloat("PanoramaCylinderOptical-emissionB", Cylinder.emissionB);
        }

        void Caller_OnTypeChanged(int selectedPanel)
        {
            prevSelectedPanel = this.selectedPanel;
            this.selectedPanel = selectedPanel;

            for (int i = 0; i < OpticalPanel.Length; ++i)
            {
                OpticalPanel[i].gameObject.SetActive(false);
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

            OpticalPanel[prevSelectedPanel].SetActive(false);
            isPanelOpened = !isPanelOpened;
            OpticalPanel[selectedPanel].SetActive(isPanelOpened);
        }
    };
};
