using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaCylinder
    {
        public float radius { get; set; }
        public float height { get; set; }
        public float ch { get; set; }
        public float cl { get; set; }
        public float startingHeight { get; set; }

        public void BindInput(Transform panel)
        {
            // bind the radius
            var radiusInputField = panel.GetChild(1).GetComponent<InputField>();
            radiusInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        radius = asFloat;
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(3).GetComponent<InputField>();
            heightInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(5).GetComponent<InputField>();
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
            var clInputField = panel.GetChild(5).GetComponent<InputField>();
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        cl = asFloat;
                    }
                }
            );

            // bind the starting height
            var startingHeightInputField = panel.GetChild(9).GetComponent<InputField>();
            startingHeightInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        startingHeight = asFloat;
                    }
                }
            );
        }
    };
};