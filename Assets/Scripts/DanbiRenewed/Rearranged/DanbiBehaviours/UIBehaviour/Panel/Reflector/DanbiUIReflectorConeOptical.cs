using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorConeOptical
    {
        [Readonly]
        public float specularR;
        [Readonly]
        public float specularG;
        [Readonly]
        public float specularB;
        [Readonly]
        public float emissionR;
        [Readonly]
        public float emissionG;
        [Readonly]
        public float emissionB;

        readonly DanbiUIReflectorOpticalPanelControl Owner;

        public DanbiUIReflectorConeOptical(DanbiUIReflectorOpticalPanelControl owner) => Owner = owner;

        void LoadPreviousValues(params ILayoutElement[] uiElements)
        {
            float prevSpecularR = PlayerPrefs.GetFloat("ReflectorConeOptical-specularR", default);
            specularR = prevSpecularR;
            (uiElements[0] as InputField).text = prevSpecularR.ToString();

            float prevSpecularG = PlayerPrefs.GetFloat("ReflectorConeOptical-specularG", default);
            specularG = prevSpecularG;
            (uiElements[1] as InputField).text = prevSpecularG.ToString();

            float prevSpecularB = PlayerPrefs.GetFloat("ReflectorConeOptical-specularB", default);
            specularB = prevSpecularB;
            (uiElements[2] as InputField).text = prevSpecularB.ToString();

            float prevEmissionR = PlayerPrefs.GetFloat("ReflectorConeOptical-emissionR", default);
            emissionR = prevEmissionR;
            (uiElements[3] as InputField).text = prevEmissionR.ToString();

            float prevEmissionG = PlayerPrefs.GetFloat("ReflectorConeOptical-emissionG", default);
            emissionG = prevEmissionG;
            (uiElements[4] as InputField).text = prevEmissionG.ToString();

            float prevEmissionB = PlayerPrefs.GetFloat("ReflectorConeOptical-emissionB", default);
            emissionB = prevEmissionB;
            (uiElements[5] as InputField).text = prevEmissionB.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
        }

        public void BindInput(Transform panel)
        {
            #region Bind Speculars
            // bind the specular R
            var specularRInputField = panel.GetChild(0).GetComponent<InputField>();
            specularRInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        specularR = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the specular G
            var specularGInputField = panel.GetChild(1).GetComponent<InputField>();
            specularGInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        specularG = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the specular B
            var specularBInputField = panel.GetChild(2).GetComponent<InputField>();
            specularBInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        specularB = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );
            #endregion Bind Speculars

            #region Bind Emissions

            // bind the emission R
            var emissionRInputField = panel.GetChild(3).GetComponent<InputField>();
            emissionRInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        emissionR = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the emission G
            var emissionGInputField = panel.GetChild(4).GetComponent<InputField>();
            emissionGInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        emissionG = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the emission B
            var emissionBInputField = panel.GetChild(5).GetComponent<InputField>();
            emissionBInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        emissionB = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );
            #endregion Bind Emissions

            LoadPreviousValues(specularRInputField, specularGInputField, specularBInputField, emissionRInputField, emissionGInputField, emissionBInputField);
        }
    };
};
