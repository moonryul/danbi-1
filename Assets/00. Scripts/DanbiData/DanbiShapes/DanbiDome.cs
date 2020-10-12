using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Danbi
{
    public class DanbiDome : DanbiBaseShape
    {
        [SerializeField, Space(20)]
        DanbiDomeData ShapeData = new DanbiDomeData();

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(0.08f, 0.08f, 0.08f);

        protected override void Awake()
        {
            base.Awake();
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdated;
        }

        protected override void OnShapeChanged()
        {
            var heightOffset = new Vector3(0, -(ShapeData.Distance + ShapeData.Height), 0);
            transform.position = Camera.main.transform.position + heightOffset * 0.01f;
            transform.localScale = new Vector3(ShapeData.Radius / originalSize.x,
                                               ShapeData.Height / originalSize.y,
                                               ShapeData.Radius / originalSize.z) * 0.01f;
        }

        protected override void Caller_OnMeshRebuild(ref DanbiMeshData data,
                                                     out DanbiBaseShapeData shapeData)
        {
            BaseShapeData = ShapeData;
            base.Caller_OnMeshRebuild(ref data, out shapeData);
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIReflectorDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIReflectorDimensionPanelControl;

                // r = (h ^ 2 + (d ^ 2 / 4)) / 2h
                float h = dimensionPanel.Dome.height;
                float d = dimensionPanel.Dome.diameter;
                ShapeData.Radius = ((h * h) + (d * d / 4)) / (2 * h);

                // ShapeData.Radius = dimensionPanel.Dome.diameter;
                ShapeData.Height = dimensionPanel.Dome.height;
                ShapeData.Distance = dimensionPanel.Dome.distance;

                OnShapeChanged();
            }

            if (control is DanbiUIReflectorOpticalPanelControl)
            {
                var opticalPanel = control as DanbiUIReflectorOpticalPanelControl;

                // ShapeData.specular = Vector3.one; //new Vector3(0.1f, 0.1f, 0.1f); // TODO: Specular! 
                ShapeData.specular = new Vector3(opticalPanel.Dome.specularR, opticalPanel.Dome.specularG, opticalPanel.Dome.specularB);
                ShapeData.emission = new Vector3(opticalPanel.Dome.emissionR, opticalPanel.Dome.emissionG, opticalPanel.Dome.emissionB);
            }
        }
    }; // class ending.
}; // namespace Danbi
