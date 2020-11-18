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
        float originalRadius = 8.0f; // unit is cm

        protected override void Awake()
        {
            base.Awake();
            DanbiUISync.onPanelUpdate += OnPanelUpdated;
        }

        protected override void OnShapeChanged()
        {
            if (ShapeData.radius <= 0.0f)
            {
                return;
            }

            transform.position = Camera.main.transform.position 
                + new Vector3(0, -(ShapeData.distance + ShapeData.radius), 0) * 0.01f;
            transform.localScale = new Vector3(ShapeData.radius / originalRadius,
                                               ShapeData.radius / originalRadius,
                                               ShapeData.radius / originalRadius); // unit is cm
        }

        protected override void RebuildMesh(ref DanbiMeshData data,
                                            out DanbiBaseShapeData shapeData)
        {
            BaseShapeData = ShapeData;
            base.RebuildMesh(ref data, out shapeData);
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIReflectorDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIReflectorDimensionPanelControl;

                float h = dimensionPanel.Dome.height;
                float dr = dimensionPanel.Dome.bottomRadius;
                float maskingRatio = dimensionPanel.Dome.maskingRatio * 0.01f;

                ShapeData.radius = ((h * h) + (dr * dr)) / (2 * h);
                ShapeData.usedHeight = (1.0f - maskingRatio) * h;                
                ShapeData.distance = dimensionPanel.Dome.distance;

                OnShapeChanged();
            }

            if (control is DanbiUIReflectorOpticalPanelControl)
            {
                var opticalPanel = control as DanbiUIReflectorOpticalPanelControl;

                ShapeData.specular = new Vector3(opticalPanel.Dome.specularR, opticalPanel.Dome.specularG, opticalPanel.Dome.specularB);
                ShapeData.emission = new Vector3(opticalPanel.Dome.emissionR, opticalPanel.Dome.emissionG, opticalPanel.Dome.emissionB);
            }
        }
    }; // class ending.
}; // namespace Danbi
