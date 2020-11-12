using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public sealed class DanbiCubePanorama : DanbiBaseShape
    {
        [SerializeField, Readonly, Space(20)]
        float width;

        [SerializeField, Readonly]
        float depth;

        [SerializeField]
        DanbiPanoramaData ShapeData = new DanbiPanoramaData();
        public DanbiPanoramaData shapeData => ShapeData;

        [SerializeField, Readonly]
        Vector3 originalSize = new Vector3(3.2f, 0.6748f, 3.2f);

        override protected void Awake()
        {
            base.Awake();
            DanbiUISync.onPanelUpdate += this.OnPanelUpdate;
        }        

        protected override void OnShapeChanged()
        {
            var heightOffset = new Vector3(0, ShapeData.low, 0);
            transform.position = Camera.main.transform.position + (heightOffset * 0.01f);
            transform.localScale = new Vector3(width / originalSize.x,
                                               (ShapeData.high - ShapeData.low) / originalSize.y,
                                               depth / originalSize.z) * 0.01f;
        }

        protected override void RebuildMesh(ref DanbiMeshData data,
                                                     out DanbiBaseShapeData shapeData)
        {
            BaseShapeData = ShapeData;
            base.RebuildMesh(ref data, out shapeData);
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            // control is the parent of all the control which related to ui panel control.
            // if ((DanbiUIPanoramaScreenDimensionPanelControl)control != null)
            if (control is DanbiUIPanoramaScreenDimensionPanelControl)
            {
                var dimensionPanel = control as DanbiUIPanoramaScreenDimensionPanelControl;

                width = dimensionPanel.Cube.width;
                depth = dimensionPanel.Cube.depth;
                ShapeData.high = dimensionPanel.Cube.ch;
                ShapeData.low = dimensionPanel.Cube.cl;

                OnShapeChanged();
            }

            if (control is DanbiUIPanoramaScreenOpticalPanelControl)
            {
                var opticalPanel = control as DanbiUIPanoramaScreenOpticalPanelControl;
                ShapeData.specular = new Vector3(opticalPanel.Cube.specularR, opticalPanel.Cube.specularG, opticalPanel.Cube.specularB);
                ShapeData.emission = new Vector3(opticalPanel.Cube.emissionR, opticalPanel.Cube.emissionG, opticalPanel.Cube.emissionB);
            }
        }
    };
};
