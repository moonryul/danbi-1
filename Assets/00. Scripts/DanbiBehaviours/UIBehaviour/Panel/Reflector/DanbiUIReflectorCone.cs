using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIReflectorCone
    {
        [Readonly]
        public float m_distance;

        [Readonly]
        public float m_height;

        [Readonly]
        public float m_radius;

        [Readonly]
        public float m_maskingRatio;

        public delegate void OnDistanceUpdate(float distance);
        public static OnDistanceUpdate onDistanceUpdate;

        public delegate void OnHeightUpdate(float height);
        public static OnHeightUpdate onHeightUpdate;

        public delegate void OnRadiusUpdate(float radius);
        public static OnRadiusUpdate onRadiusUpdate;

        public delegate void OnMaskingRatioUpdate(float maskingRatio);
        public static OnMaskingRatioUpdate onMaskingRatioUpdate;

        void LoadPreviousValues(params Selectable[] uiElements)
        {
            float prevDistance = PlayerPrefs.GetFloat("ReflectorCone-distance", 0.0f);
            (uiElements[0] as InputField).text = prevDistance.ToString();
            m_distance = prevDistance;
            onDistanceUpdate?.Invoke(m_distance);

            float prevHeight = PlayerPrefs.GetFloat("ReflectorCone-height", 0.0f);
            (uiElements[1] as InputField).text = prevHeight.ToString();
            m_height = prevHeight;
            onHeightUpdate?.Invoke(m_height);

            float prevRadius = PlayerPrefs.GetFloat("ReflectorCone-radius", 0.0f);
            (uiElements[2] as InputField).text = prevRadius.ToString();
            m_radius = prevRadius;
            onRadiusUpdate?.Invoke(m_radius);
        }

        public void BindInput(Transform panel)
        {
            // bind the distance From Projector
            var distanceInputField = panel.GetChild(0).GetComponent<InputField>();
            distanceInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_distance = res;
                        onDistanceUpdate?.Invoke(res);
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_height = res;
                        onHeightUpdate?.Invoke(res);
                    }
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(2).GetComponent<InputField>();
            radiusInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_radius = res;
                        onRadiusUpdate?.Invoke(res);
                    }
                }
            );

            LoadPreviousValues(distanceInputField, heightInputField, radiusInputField);
        }
    };
};