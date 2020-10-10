using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiUIReflectorOpticalPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public DanbiUIReflectorDomeOptical Dome;
        [Readonly]
        public DanbiUIReflectorConeOptical Cone;
        int prevSelectedPanel = 0;
        int selectedPanel = 0;
        new GameObject[] Panel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        new void Start()
        {
            Dome = new DanbiUIReflectorDomeOptical(this);
            Cone = new DanbiUIReflectorConeOptical(this);

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
