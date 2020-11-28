using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIPanoramaCylinderDimension
    {
        [Readonly]
        public float m_radius;
        [Readonly]
        public float m_ch;
        [Readonly]
        public float m_cl;
        public delegate void OnRadiusChange(float radius);
        public static OnRadiusChange onRadiusChange;

        public delegate void OnCHChange(float ch);
        public static OnCHChange onCHChange;

        public delegate void OnCLChange(float cl);
        public static OnCLChange onCLChange;

        void LoadPreviousValues(params ILayoutElement[] uiElements)
        {
            var prevRadius = PlayerPrefs.GetFloat("PanoramaCylinderDimension-radius", default);
            m_radius = prevRadius;
            (uiElements[0] as InputField).text = prevRadius.ToString();
            onRadiusChange?.Invoke(m_radius);

            var prevCh = PlayerPrefs.GetFloat("PanoramaCylinderDimension-ch", m_ch);
            m_ch = prevCh;
            (uiElements[1] as InputField).text = prevCh.ToString();
            onCHChange?.Invoke(m_ch);

            var prevCl = PlayerPrefs.GetFloat("PanoramaCylinderDimension-cl", m_cl);
            m_cl = prevCl;
            (uiElements[2] as InputField).text = prevCl.ToString();
            onCLChange?.Invoke(m_cl);
        }

        public void BindInput(Transform panel)
        {
            // bind the radius
            var radiusInputField = panel.GetChild(0).GetComponent<InputField>();
            radiusInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_radius = res;
                        onRadiusChange?.Invoke(res);
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(1).GetComponent<InputField>();
            chInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_ch = res;
                        onCHChange?.Invoke(res);
                    }
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(2).GetComponent<InputField>();
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_cl = res;
                        onCLChange?.Invoke(res);
                    }
                }
            );

            LoadPreviousValues(radiusInputField, chInputField, clInputField);
        }
    };
};