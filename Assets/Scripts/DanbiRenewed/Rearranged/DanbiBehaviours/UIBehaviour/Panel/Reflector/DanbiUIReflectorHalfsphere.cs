
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIReflectorHalfsphere
    {
        [Readonly]
        public float distanceFromProjector;
        [Readonly]
        public float height;
        [Readonly]
        public float radius;

        DanbiUIReflectorShapePanelControl Owner;
        public DanbiUIReflectorHalfsphere(DanbiUIReflectorShapePanelControl owner)
        {
            Owner = owner;
        }        

        public void BindInput(Transform panel)
        {
            // bind the distance From Projector
            var distanceFromProjectorInputField = panel.GetChild(0).GetComponent<InputField>();
            float prevDistance = PlayerPrefs.GetFloat("ReflectorHalfsphere-distanceFromProjector", 0.0f);
            distanceFromProjector = prevDistance;
            distanceFromProjectorInputField.text = prevDistance.ToString();
            distanceFromProjectorInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        distanceFromProjector = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            float prevHeight = PlayerPrefs.GetFloat("ReflectorHalfsphere-height", 0.0f);
            heightInputField.text = prevHeight.ToString();
            height = prevHeight;
            heightInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(2).GetComponent<InputField>();
            float prevRadius = PlayerPrefs.GetFloat("ReflectorHalfsphere-radius", 0.0f);
            radiusInputField.text = prevRadius.ToString();
            radius = prevRadius;
            radiusInputField.onValueChanged.AddListener(
                (val) =>
                {
                    radius = float.Parse(val);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                }
            );
        }
    };
};