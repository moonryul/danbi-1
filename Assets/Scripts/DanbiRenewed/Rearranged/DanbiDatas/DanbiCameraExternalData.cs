using UnityEngine;

namespace Danbi
{
    [CreateAssetMenu(fileName = "Camera External Data Scriptable Object", menuName = "Danbi Scriptable Objects/Camera External Data")]
    public class DanbiCameraExternalData : ScriptableObject
    {
        public Vector3 RadialCoefficient;  // 4 * 3 = 12
        public Vector3 TangentialCoefficient; // 4 * 2 = 12
        public Vector2 PrincipalPoint; // 4 * 2 = 8
        public Vector2 FocalLength; // 4 * 2 = 8
        public float SkewCoefficient;  // 4 

        public int stride => (4 * 3) + (4 * 3) +
                             (4 * 2) + (4 * 2) +
                             4; // 40
        public DanbiCameraExternalData_struct asStruct => new DanbiCameraExternalData_struct()
        {
            radialCoefficient = this.RadialCoefficient,
            tangentialCoefficient = this.TangentialCoefficient,
            principalPoint = this.PrincipalPoint,
            focalLength = this.FocalLength,
            skewCoefficient = this.SkewCoefficient
        };
    }; // 12 + 8 + 8 + 8 + 4 = 40

    [System.Serializable]
    public struct DanbiCameraExternalData_struct
    {
        public Vector3 radialCoefficient;  // 4 * 3 = 12
        public Vector3 tangentialCoefficient; // 4 * 3 = 12
        public Vector2 principalPoint; // 4 * 2 = 8
        public Vector2 focalLength; // 4 * 2 = 8
        public float skewCoefficient;  // 4 
    };
};
