using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorCone
    {
        public float distanceFromProjector { get; set; }
        public float height { get; set; }
        public float radius { get; set; }
        public bool isBound { get; set; } = false;

        public void BindInput(Transform panel)
        {
            if (isBound)
            {
                return;
            }
            isBound = true;

            // bind the distance From Projector
            var distanceFromProjectorInputField = panel.GetChild(1).GetComponent<InputField>();
            distanceFromProjectorInputField.onValueChanged.AddListener(
                (val) =>
                {
                    distanceFromProjector = float.Parse(val);
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(3).GetComponent<InputField>();
            heightInputField.onValueChanged.AddListener(
                (val) =>
                {
                    height = float.Parse(val);
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(5).GetComponent<InputField>();
            radiusInputField.onValueChanged.AddListener(
                (val) =>
                {
                    radius = float.Parse(val);
                }
            );
        }
    };
};