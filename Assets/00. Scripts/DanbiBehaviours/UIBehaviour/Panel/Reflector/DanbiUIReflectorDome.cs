
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIReflectorDome
    {
        [Readonly]
        public float distance;

        [Readonly]
        public float height;

        [Readonly]
        public float bottomRadius;

        [Readonly]
        public float maskingRatio;

        DanbiUIReflectorDimensionPanelControl Owner;

        public DanbiUIReflectorDome(DanbiUIReflectorDimensionPanelControl owner)
        {
            Owner = owner;
        }

        void LoadPreviousValues(params Selectable[] uiElements)
        {
            float prevDistance = PlayerPrefs.GetFloat("ReflectorDome-distance", default);
            (uiElements[0] as InputField).text = prevDistance.ToString();
            distance = prevDistance;

            float prevHeight = PlayerPrefs.GetFloat("ReflectorDome-height", 0.0f);
            height = prevHeight;
            (uiElements[1] as InputField).text = prevHeight.ToString();

            float prevRadius = PlayerPrefs.GetFloat("ReflectorDome-radius", 0.0f);
            bottomRadius = prevRadius;
            (uiElements[2] as InputField).text = prevRadius.ToString();

            float prevMaskingRatio = PlayerPrefs.GetFloat("ReflectorDome-maskingRatio", 0.0f);
            maskingRatio = prevMaskingRatio;
            (uiElements[3] as InputField).text = prevMaskingRatio.ToString();

            DanbiUISync.onPanelUpdated?.Invoke(Owner);
        }

        public void BindInput(Transform panel)
        {
            // bind the distance From Projector
            var distanceInputField = panel.GetChild(0).GetComponent<InputField>();
            distanceInputField.onEndEdit.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        distance = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField.onEndEdit.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(2).GetComponent<InputField>();
            radiusInputField.onEndEdit.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        bottomRadius = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            // bind the masking ratio
            var maskingRatioInputField = panel.GetChild(3).GetComponent<InputField>();
            maskingRatioInputField.onEndEdit.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        maskingRatio = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            LoadPreviousValues(distanceInputField, heightInputField, radiusInputField, maskingRatioInputField);
        }
    };
};