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
            var distanceFromProjectorInputField = panel.GetChild(0).GetComponent<InputField>();
            distanceFromProjectorInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat)) 
                    {
                        distanceFromProjector = asFloat;
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField.onValueChanged.AddListener(
                (string val) =>
                {                    
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                    }
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(2).GetComponent<InputField>();
            radiusInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        radius = asFloat;
                    }
                }
            );
        }
    };
};