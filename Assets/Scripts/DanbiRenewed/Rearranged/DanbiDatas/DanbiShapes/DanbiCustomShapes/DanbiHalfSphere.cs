using UnityEngine;
using System.Collections.Generic;

namespace Danbi
{
    public class DanbiHalfSphere : DanbiBaseShape
    {
        protected override void OnShapeChanged()
        {
            var MainCamTransform = transform.parent;
            transform.position = MainCamTransform.position +
                new Vector3(0, -(ShapeTransform.Distance + ShapeTransform.Height), 0);
        }
    }; // class ending.
}; // namespace Danbi
