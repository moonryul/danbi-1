using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIReflectorCone
    {
        [Readonly]
        public float distance;

        [Readonly]
        public float height;

        [Readonly]
        public float radius;

        DanbiUIReflectorDimensionPanelControl Owner;

        public DanbiUIReflectorCone(DanbiUIReflectorDimensionPanelControl owner)
        {
            Owner = owner;
        }

        void LoadPreviousValues(params Selectable[] uiElements)
        {
            float prevDistance = PlayerPrefs.GetFloat("ReflectorCone-distance", 0.0f);
            (uiElements[0] as InputField).text = prevDistance.ToString();
            distance = prevDistance;

            float prevHeight = PlayerPrefs.GetFloat("ReflectorCone-height", 0.0f);
            (uiElements[1] as InputField).text = prevHeight.ToString();
            height = prevHeight;

            float prevRadius = PlayerPrefs.GetFloat("ReflectorCone-radius", 0.0f);
            (uiElements[2] as InputField).text = prevRadius.ToString();
            radius = prevRadius;

            DanbiUISync.onPanelUpdate?.Invoke(Owner);
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
                        DanbiUISync.onPanelUpdate?.Invoke(Owner);
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
                        DanbiUISync.onPanelUpdate?.Invoke(Owner);
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
                        radius = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            LoadPreviousValues(distanceInputField, heightInputField, radiusInputField);
        }
    };
};