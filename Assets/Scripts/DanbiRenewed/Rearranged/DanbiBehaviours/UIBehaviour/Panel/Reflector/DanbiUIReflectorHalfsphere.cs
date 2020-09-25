
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorHalfsphere
    {
        public float distanceFromProjector { get; set; }
        public float height { get; set; }
        public float radius { get; set; }

        DanbiUIReflectorShapePanelControl Owner;
        public DanbiUIReflectorHalfsphere(DanbiUIReflectorShapePanelControl owner)
        {
            Owner = owner;
        }

        void OnDisable()
        {
            PlayerPrefs.SetFloat("ReflectorHalfsphere-distanceFromProjector", distanceFromProjector);
            PlayerPrefs.SetFloat("ReflectorHalfsphere-height", height);
            PlayerPrefs.SetFloat("ReflectorHalfsphere-radius", radius);
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