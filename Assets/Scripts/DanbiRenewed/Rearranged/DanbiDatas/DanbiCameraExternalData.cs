using UnityEngine;

namespace Danbi
{
    [System.Serializable]
    public class DanbiCameraExternalData
    {
        public float radialCoefficientX;
        public float radialCoefficientY;
        public float radialCoefficientZ;
        public float tangentialCoefficientX;
        public float tangentialCoefficientY;
        public float tangentialCoefficientZ;
        public float principalPointX;
        public float principalPointY;
        public float focalLengthX;
        public float focalLengthY;
        public float skewCoefficient;

        public int stride => 44; // 40
        public DanbiCameraExternalData_struct asStruct => new DanbiCameraExternalData_struct()
        {
            radialCoefficientX = this.radialCoefficientX,
            radialCoefficientY = this.radialCoefficientY,
            radialCoefficientZ = this.radialCoefficientZ,
            tangentialCoefficientX = this.tangentialCoefficientX,
            tangentialCoefficientY = this.tangentialCoefficientY,
            tangentialCoefficientZ = this.tangentialCoefficientZ,
            principalPointX = this.principalPointX,
            principalPointY = this.principalPointY,
            focalLengthX = this.focalLengthX,
            focalLengthY = this.focalLengthY,
            skewCoefficient = this.skewCoefficient
        };
    };

    [System.Serializable]
    public struct DanbiCameraExternalData_struct
    {
        public float radialCoefficientX;
        public float radialCoefficientY;
        public float radialCoefficientZ;
        public float tangentialCoefficientX;
        public float tangentialCoefficientY;
        public float tangentialCoefficientZ;
        public float principalPointX;
        public float principalPointY;
        public float focalLengthX;
        public float focalLengthY;
        public float skewCoefficient;
    }; // 216
};
