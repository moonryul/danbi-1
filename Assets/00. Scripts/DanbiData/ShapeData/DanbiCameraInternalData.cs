using UnityEngine;

namespace Danbi
{
    [System.Serializable]
    public class DanbiCameraInternalData
    {
        public float radialCoefficientX;
        public float radialCoefficientY;
        public float radialCoefficientZ;
        public float tangentialCoefficientX;
        public float tangentialCoefficientY;
        public float principalPointX;
        public float principalPointY;
        public float focalLengthX;
        public float focalLengthY;
        public float skewCoefficient;

        public int stride => 40;
        public DanbiCameraInternalData_struct asStruct => new DanbiCameraInternalData_struct()
        {
            radialCoefficientX = this.radialCoefficientX,
            radialCoefficientY = this.radialCoefficientY,
            radialCoefficientZ = this.radialCoefficientZ,
            tangentialCoefficientX = this.tangentialCoefficientX,
            tangentialCoefficientY = this.tangentialCoefficientY,
            principalPointX = this.principalPointX,
            principalPointY = this.principalPointY,
            focalLengthX = this.focalLengthX,
            focalLengthY = this.focalLengthY,
            skewCoefficient = this.skewCoefficient
        };
    };

    [System.Serializable]
    public struct DanbiCameraInternalData_struct
    {
        public float radialCoefficientX;
        public float radialCoefficientY;
        public float radialCoefficientZ;
        public float tangentialCoefficientX;
        public float tangentialCoefficientY;
        public float principalPointX;
        public float principalPointY;
        public float focalLengthX;
        public float focalLengthY;
        public float skewCoefficient;
    }; // 216
};
