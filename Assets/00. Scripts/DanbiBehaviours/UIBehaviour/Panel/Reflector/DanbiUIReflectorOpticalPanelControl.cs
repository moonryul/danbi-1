using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    #pragma warning disable 3001
    public class DanbiUIReflectorOpticalPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public DanbiUIReflectorDomeOptical Dome;
        [Readonly]
        public DanbiUIReflectorConeOptical Cone;
        int prevSelectedPanel = 0;
        int selectedPanel = 0;
        GameObject[] OpticalPanel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        void Start()
        {
            Dome = new DanbiUIReflectorDomeOptical(this);
            Cone = new DanbiUIReflectorConeOptical(this);

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

            Dome.BindInput(OpticalPanel[0].transform);
            Cone.BindInput(OpticalPanel[1].transform);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("ReflectorConeOptical-specularR", Cone.specularR);
            PlayerPrefs.SetFloat("ReflectorConeOptical-specularG", Cone.specularG);
            PlayerPrefs.SetFloat("ReflectorConeOptical-specularB", Cone.specularB);
            PlayerPrefs.SetFloat("ReflectorConeOptical-emissionR", Cone.emissionR);
            PlayerPrefs.SetFloat("ReflectorConeOptical-emissionG", Cone.emissionG);
            PlayerPrefs.SetFloat("ReflectorConeOptical-emissionB", Cone.emissionB);

            PlayerPrefs.SetFloat("ReflectorDomeOptical-specularR", Dome.specularR);
            PlayerPrefs.SetFloat("ReflectorDomeOptical-specularG", Dome.specularG);
            PlayerPrefs.SetFloat("ReflectorDomeOptical-specularB", Dome.specularB);
            PlayerPrefs.SetFloat("ReflectorDomeOptical-emissionR", Dome.emissionR);
            PlayerPrefs.SetFloat("ReflectorDomeOptical-emissionG", Dome.emissionG);
            PlayerPrefs.SetFloat("ReflectorDomeOptical-emissionB", Dome.emissionB);
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
