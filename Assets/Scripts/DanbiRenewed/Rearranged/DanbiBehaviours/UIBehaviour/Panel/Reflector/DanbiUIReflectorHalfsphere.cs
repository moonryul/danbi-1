
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIReflectorHalfsphere
    {
        [Readonly]
        public float distance;

        [Readonly]
        public float height;

        [Readonly]
        public float diameter;

        DanbiUIReflectorDimensionPanelControl Owner;

        public DanbiUIReflectorHalfsphere(DanbiUIReflectorDimensionPanelControl owner)
        {
            Owner = owner;
        }

        void LoadPreviousValues(params Selectable[] uiElements)
        {
            float prevDistance = PlayerPrefs.GetFloat("ReflectorHalfsphere-distance", default);
            (uiElements[0] as InputField).text = prevDistance.ToString();
            distance = prevDistance;

            float prevHeight = PlayerPrefs.GetFloat("ReflectorHalfsphere-height", 0.0f);
            height = prevHeight;
            (uiElements[1] as InputField).text = prevHeight.ToString();

            float prevRadius = PlayerPrefs.GetFloat("ReflectorHalfsphere-radius", 0.0f);
            diameter = prevRadius;
            (uiElements[2] as InputField).text = prevRadius.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
        }

        public void BindInput(Transform panel)
        {
            // bind the distance From Projector
            var distanceInputField = panel.GetChild(0).GetComponent<InputField>();
            distanceInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        distance = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(2).GetComponent<InputField>();
            radiusInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        diameter = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            LoadPreviousValues(distanceInputField, heightInputField, radiusInputField);
        }
    };
};