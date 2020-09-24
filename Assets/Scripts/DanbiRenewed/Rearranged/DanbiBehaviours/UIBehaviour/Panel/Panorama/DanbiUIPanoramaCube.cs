using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaCube
    {
        public float width { get; set; }
        public float height { get; set; }
        public float depth { get; set; }
        public float ch { get; set; }
        public float cl { get; set; }

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
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                    }
                }
            );

            // bind the depth
            var depthInputField = panel.GetChild(2).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        depth = asFloat;
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(3).GetComponent<InputField>();
            chInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        ch = asFloat;
                    }
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(4).GetComponent<InputField>();
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        cl = asFloat;
                    }
                }
            );
        }
    };
};