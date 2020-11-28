
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIReflectorDome
    {
        [Readonly]
        public float m_distance;

        [Readonly]
        public float m_height;

        [Readonly]
        public float m_bottomRadius;

        [Readonly]
        public float m_maskingRatio;

        public delegate void OnDistanceUpdate(float distance);
        public static OnDistanceUpdate onDistanceUpdate;

        public delegate void OnHeightUpdate(float height);
        public static OnHeightUpdate onHeightUpdate;

        public delegate void OnBottomRadiusUpdate(float radius);
        public static OnBottomRadiusUpdate onBottomRadiusUpdate;

        public delegate void OnMaskingRatioUpdate(float maskingRatio);
        public static OnMaskingRatioUpdate onMaskingRatioUpdate;

        void LoadPreviousValues(params Selectable[] uiElements)
        {
            float prevDistance = PlayerPrefs.GetFloat("ReflectorDome-distance", default);
            m_distance = prevDistance;
            (uiElements[0] as InputField).text = prevDistance.ToString();
            onDistanceUpdate?.Invoke(m_distance);

            float prevHeight = PlayerPrefs.GetFloat("ReflectorDome-height", 0.0f);
            m_height = prevHeight;
            (uiElements[1] as InputField).text = prevHeight.ToString();
            onHeightUpdate?.Invoke(m_height);

            float prevRadius = PlayerPrefs.GetFloat("ReflectorDome-radius", 0.0f);
            m_bottomRadius = prevRadius;
            (uiElements[2] as InputField).text = prevRadius.ToString();
            onBottomRadiusUpdate?.Invoke(m_bottomRadius);

            float prevMaskingRatio = PlayerPrefs.GetFloat("ReflectorDome-maskingRatio", 0.0f);
            m_maskingRatio = prevMaskingRatio;
            (uiElements[3] as InputField).text = prevMaskingRatio.ToString();
            onMaskingRatioUpdate?.Invoke(m_maskingRatio);
        }

        public void BindInput(Transform panel)
        {
            // bind the distance From Projector
            var distanceInputField = panel.GetChild(0).GetComponent<InputField>();
            distanceInputField.onEndEdit.AddListener(
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
            heightInputField.onEndEdit.AddListener(
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
            radiusInputField.onEndEdit.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_bottomRadius = res;
                        onBottomRadiusUpdate?.Invoke(res);
                    }
                }
            );

            // bind the masking ratio
            var maskingRatioInputField = panel.GetChild(3).GetComponent<InputField>();
            maskingRatioInputField.onEndEdit.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_maskingRatio = res;
                        onMaskingRatioUpdate?.Invoke(res);
                    }
                }
            );

            LoadPreviousValues(distanceInputField, heightInputField, radiusInputField, maskingRatioInputField);
        }
    };
};