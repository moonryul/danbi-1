
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorHalfsphere
    {
        public float distanceFromProjector { get; set; }
        public float height { get; set; }
        public float radius { get; set; }
        // public bool isBound { get; set; } = false;

        DanbiUIReflectorShapePanelControl Owner;
        public DanbiUIReflectorHalfsphere(DanbiUIReflectorShapePanelControl owner)
        {
            Owner = owner;
        }

        public void BindInput(Transform panel)
        {            
            // bind the distance From Projector
            var distanceFromProjectorInputField = panel.GetChild(0).GetComponent<InputField>();
            distanceFromProjectorInputField.text = "";
            distanceFromProjectorInputField.onValueChanged.AddListener(
                (val) =>
                {
                    distanceFromProjector = float.Parse(val);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                }
            );

            // bind the height
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField.text = "";
            heightInputField.onValueChanged.AddListener(
                (val) =>
                {
                    height = float.Parse(val);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                }
            );

            // bind the radius
            var radiusInputField = panel.GetChild(2).GetComponent<InputField>();
            radiusInputField.text = "";
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