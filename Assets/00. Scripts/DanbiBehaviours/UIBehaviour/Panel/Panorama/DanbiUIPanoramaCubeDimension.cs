using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIPanoramaCubeDimension
    {
        [Readonly]
        public float width;
        [Readonly]
        public float depth;
        [Readonly]
        public float ch;
        [Readonly]
        public float cl;
        readonly DanbiUIPanoramaScreenDimensionPanelControl Owner;

        public DanbiUIPanoramaCubeDimension(DanbiUIPanoramaScreenDimensionPanelControl owner) => Owner = owner;

        void LoadPreviousValues(params ILayoutElement[] uiElements)
        {
            float prevWidth = PlayerPrefs.GetFloat("PanoramaCubeDimension-width", default);
            width = prevWidth;
            (uiElements[0] as InputField).text = prevWidth.ToString();

            var prevDepth = PlayerPrefs.GetFloat("PanoramaCubeDimension-depth", default);
            depth = prevDepth;
            (uiElements[1] as InputField).text = prevDepth.ToString();

            var prevCh = PlayerPrefs.GetFloat("PanoramaCubeDimension-ch", default);
            ch = prevCh;
            (uiElements[2] as InputField).text = prevCh.ToString();

            float prevCl = PlayerPrefs.GetFloat("PanoramaCubeDimension-cl", default);
            cl = prevCl;
            (uiElements[3] as InputField).text = prevCl.ToString();

            DanbiUISync.onPanelUpdated?.Invoke(Owner);
        }

        public void BindInput(Transform panel)
        {
            // bind the width
            var widthInputField = panel.GetChild(0).GetComponent<InputField>();
            widthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        width = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            // bind the depth
            var depthInputField = panel.GetChild(1).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        depth = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(2).GetComponent<InputField>();
            chInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        ch = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(3).GetComponent<InputField>();
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        cl = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(Owner);
                    }
                }
            );

            LoadPreviousValues(widthInputField, depthInputField, chInputField, clInputField);
        }
    };
};