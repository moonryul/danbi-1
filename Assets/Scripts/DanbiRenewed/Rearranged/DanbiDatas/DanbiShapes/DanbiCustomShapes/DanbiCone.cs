using UnityEngine;

// using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi
{
    public class DanbiCone : DanbiBaseShape
    {
        protected override void OnShapeChanged()
        {
            var mainCamTransform = transform.parent;
            transform.position = mainCamTransform.position +
                new Vector3(0, -(ShapeTransform.Distance + ShapeTransform.Height), 0);
        }
    }; // class ending
}; // namespace Danbi