using UnityEngine;
// using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi
{
    public class DanbiCylinderPanorama : DanbiBaseShape
    {
        [SerializeField, Readonly, Space(20)]
        float radius;

        [SerializeField]
        DanbiPanoramaData ShapeData = new DanbiPanoramaData();

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(3.2f, 0.6718f, 3.2f);

        override protected void Awake()
        {
            base.Awake();
            DanbiUISync.onPanelUpdate += OnPanelUpdated;
        }

        protected override void OnShapeChanged()
        {
            Vector3 heightOffset = new Vector3(0, ShapeData.low, 0);
            transform.position = Camera.main.transform.position + (heightOffset * 0.01f);
            transform.localScale = new Vector3(radius / originalSize.x,
                                               (ShapeData.high - ShapeData.low) / originalSize.y,
                                               radius / originalSize.z) * 0.01f;
        }

        protected override void RebuildMesh(ref DanbiMeshData data,
                                                     out DanbiBaseShapeData shapeData)
        {
            BaseShapeData = ShapeData;
            base.RebuildMesh(ref data, out shapeData);
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIPanoramaScreenDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIPanoramaScreenDimensionPanelControl;

                radius = dimensionPanel.Cylinder.radius;
                ShapeData.high = dimensionPanel.Cylinder.ch;
                ShapeData.low = dimensionPanel.Cylinder.cl;

                OnShapeChanged();
            }

            if (control is DanbiUIPanoramaScreenOpticalPanelControl)
            {
                var opticalPanel = control as DanbiUIPanoramaScreenOpticalPanelControl;
                
                ShapeData.specular = new Vector3(opticalPanel.Cylinder.specularR, opticalPanel.Cylinder.specularG, opticalPanel.Cylinder.specularB);
                ShapeData.emission = new Vector3(opticalPanel.Cylinder.emissionR, opticalPanel.Cylinder.emissionG, opticalPanel.Cylinder.emissionB);
            }
        }
    };
};