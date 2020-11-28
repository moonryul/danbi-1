using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIPanoramaCubeDimension
    {
        [Readonly]
        public float m_width;
        [Readonly]
        public float m_depth;
        [Readonly]
        public float m_ch;
        [Readonly]
        public float m_cl;

        public delegate void OnWidthChange(float width);
        public static OnWidthChange onWidthChange;

        public delegate void OnDepthChange(float height);
        public static OnDepthChange onDepthChange;

        public delegate void OnCHChange(float ch);
        public static OnCHChange onCHChange;

        public delegate void OnCLChange(float cl);
        public static OnCLChange onCLChange;

        void LoadPreviousValues(params ILayoutElement[] uiElements)
        {
            float prevWidth = PlayerPrefs.GetFloat("PanoramaCubeDimension-width", default);
            m_width = prevWidth;
            (uiElements[0] as InputField).text = prevWidth.ToString();
            onWidthChange?.Invoke(m_width);

            var prevDepth = PlayerPrefs.GetFloat("PanoramaCubeDimension-depth", default);
            m_depth = prevDepth;
            (uiElements[1] as InputField).text = prevDepth.ToString();
            onDepthChange?.Invoke(m_depth);

            var prevCh = PlayerPrefs.GetFloat("PanoramaCubeDimension-ch", default);
            m_ch = prevCh;
            (uiElements[2] as InputField).text = prevCh.ToString();
            onCHChange?.Invoke(m_ch);

            float prevCl = PlayerPrefs.GetFloat("PanoramaCubeDimension-cl", default);
            m_cl = prevCl;
            (uiElements[3] as InputField).text = prevCl.ToString();
            onCLChange?.Invoke(m_cl);
        }

        public void BindInput(Transform panel)
        {
            // bind the width
            var widthInputField = panel.GetChild(0).GetComponent<InputField>();
            widthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_width = res;
                        onWidthChange?.Invoke(res);
                    }
                }
            );

            // bind the depth
            var depthInputField = panel.GetChild(1).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_depth = res;
                        onDepthChange?.Invoke(res);
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(2).GetComponent<InputField>();
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
            var clInputField = panel.GetChild(3).GetComponent<InputField>();
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

            LoadPreviousValues(widthInputField, depthInputField, chInputField, clInputField);
        }
    };
};