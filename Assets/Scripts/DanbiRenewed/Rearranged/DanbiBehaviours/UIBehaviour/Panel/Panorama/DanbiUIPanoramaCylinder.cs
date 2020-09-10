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
                (val) =>
                {
                    radius = float.Parse(val);
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(3).GetComponent<InputField>();
            heightInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    height = float.Parse(val);
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(5).GetComponent<InputField>();
            chInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    ch = float.Parse(val);
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(5).GetComponent<InputField>();
            clInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    cl = float.Parse(val);
                }
            );

            // bind the starting height
            var startingHeightInputField = panel.GetChild(9).GetComponent<InputField>();
            startingHeightInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    startingHeight = float.Parse(val);
                }
            );
        }
    };
};