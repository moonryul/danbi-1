using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaCube
    {
        public float width { get; set; }
        public float depth { get; set; }
        public float ch { get; set; }
        public float cl { get; set; }
        DanbiUIPanoramaScreenShapePanelControl Owner;

        public DanbiUIPanoramaCube(DanbiUIPanoramaScreenShapePanelControl owner)
        {
            Owner = owner;
        }

        void OnDisable()
        {
            PlayerPrefs.SetFloat("PanoramaCube-width", width);
            PlayerPrefs.SetFloat("PanoramaCube-depth", depth);
            PlayerPrefs.SetFloat("PanoramaCube-ch", ch);
            PlayerPrefs.SetFloat("PanoramaCube-cl", cl);
        }

        public void BindInput(Transform panel)
        {
            // bind the width
            var widthInputField = panel.GetChild(0).GetComponent<InputField>();
            float prevWidth = PlayerPrefs.GetFloat("PanoramaCube-width", default);
            widthInputField.text = prevWidth.ToString();
            width = prevWidth;
            widthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        width = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the depth
            var depthInputField = panel.GetChild(1).GetComponent<InputField>();
            var prevDepth = PlayerPrefs.GetFloat("PanoramaCube-depth", default);
            depthInputField.text = prevDepth.ToString();
            depth = prevDepth;
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        depth = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(2).GetComponent<InputField>();
            var prevCh = PlayerPrefs.GetFloat("PanoramaCube-ch", default);
            chInputField.text = prevCh.ToString();
            ch = prevCh;
            chInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        ch = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(3).GetComponent<InputField>();
            float prevCl = PlayerPrefs.GetFloat("PanoramaCube-cl", default);
            clInputField.text = prevCl.ToString();
            cl = prevCl;
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        cl = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );
        }
    };
};